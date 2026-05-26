using System.Globalization;
using AccountingProject.Models;

namespace AccountingProject.Services;

internal interface IAnnualComparisonMonthSource
{
    bool IsMonthCaptured(int month, int gregorianYear);

    AnnualComparisonInputValues? ResolveInput(
        EmploymentData ed,
        EmploymentDataSlot slot,
        int month,
        int gregorianYear);
}

internal sealed class AnnualComparisonUploadMonthSource : IAnnualComparisonMonthSource
{
    private readonly PayrollUploadLayout _layout;
    private readonly IReadOnlyDictionary<(string TzKey, int Month, int Year), PayrollUploadRow> _byTz;
    private readonly IReadOnlyDictionary<(int EmpNum, int Month, int Year), PayrollUploadRow> _byNum;

    private AnnualComparisonUploadMonthSource(
        PayrollUploadLayout layout,
        IReadOnlyDictionary<(string TzKey, int Month, int Year), PayrollUploadRow> byTz,
        IReadOnlyDictionary<(int EmpNum, int Month, int Year), PayrollUploadRow> byNum)
    {
        _layout = layout;
        _byTz = byTz;
        _byNum = byNum;
    }

    public static AnnualComparisonUploadMonthSource FromParsedUpload(
        PayrollUploadLayout layout,
        IReadOnlyList<PayrollUploadRow> uploadRows) =>
        new(
            layout,
            PayrollComparisonUploadSupport.IndexByTzMonth(uploadRows),
            PayrollComparisonUploadSupport.IndexByEmpNumMonth(uploadRows));

    public bool IsMonthCaptured(int month, int gregorianYear) => true;

    public AnnualComparisonInputValues? ResolveInput(
        EmploymentData ed,
        EmploymentDataSlot slot,
        int month,
        int gregorianYear)
    {
        var upload = PayrollComparisonUploadSupport.ResolveForEmployee(
            ed.Employee, month, gregorianYear, _byTz, _byNum);
        if (upload == null)
            return null;

        var bandCols = PayrollComparisonUploadSupport.BandCols(_layout, slot.GradeBand);
        decimal? jobBase = null;
        var compareJobBase = false;
        if (slot.SlotIndex is >= 1 and <= 6)
        {
            var baseCol = bandCols.MisraBase[slot.SlotIndex - 1];
            if (baseCol.HasValue)
            {
                compareJobBase = true;
                jobBase = PayrollComparisonUploadSupport.ParseDecimal(
                    PayrollComparisonUploadSupport.GetCell(upload.Row, baseCol));
            }
        }

        return new AnnualComparisonInputValues(
            Role: PayrollComparisonUploadSupport.GetCell(upload.Row, _layout.ColSugMisra),
            Grade: PayrollComparisonUploadSupport.GetCell(upload.Row, bandCols.Grade),
            Seniority: PayrollComparisonUploadSupport.GetCell(upload.Row, bandCols.Seniority),
            JobBase: jobBase,
            CompareJobBase: compareJobBase,
            JobPercent: PayrollComparisonUploadSupport.ParseDecimal(
                PayrollComparisonUploadSupport.GetCell(upload.Row, bandCols.JobPercent)),
            WeeklyHours: PayrollComparisonUploadSupport.SumMisraHours(upload, bandCols),
            GeneralMultiplier: PayrollComparisonUploadSupport.ParseDecimal(
                PayrollComparisonUploadSupport.GetCell(upload.Row, bandCols.DoubleGeneral)));
    }
}

internal sealed class AnnualComparisonSavedMonthSource : IAnnualComparisonMonthSource
{
    private readonly HashSet<(int Month, int Year)> _activeMonths;
    private readonly IReadOnlyDictionary<(string TzKey, int Month, int Year), List<PayrollComparisonInputRow>> _byTz;
    private readonly IReadOnlyDictionary<(int EmpNum, int Month, int Year), List<PayrollComparisonInputRow>> _byNum;

    private AnnualComparisonSavedMonthSource(
        HashSet<(int Month, int Year)> activeMonths,
        IReadOnlyDictionary<(string TzKey, int Month, int Year), List<PayrollComparisonInputRow>> byTz,
        IReadOnlyDictionary<(int EmpNum, int Month, int Year), List<PayrollComparisonInputRow>> byNum)
    {
        _activeMonths = activeMonths;
        _byTz = byTz;
        _byNum = byNum;
    }

    public static AnnualComparisonSavedMonthSource FromSavedRows(
        IEnumerable<PayrollMonthlyInputBatch> activeBatches,
        IEnumerable<PayrollComparisonInputRow> comparisonRows)
    {
        var activeMonths = activeBatches
            .Select(b => (b.Month, b.GregorianYear))
            .ToHashSet();
        return new AnnualComparisonSavedMonthSource(
            activeMonths,
            IndexByTzMonth(comparisonRows),
            IndexByEmpNumMonth(comparisonRows));
    }

    public bool IsMonthCaptured(int month, int gregorianYear) =>
        _activeMonths.Contains((month, gregorianYear));

    public AnnualComparisonInputValues? ResolveInput(
        EmploymentData ed,
        EmploymentDataSlot slot,
        int month,
        int gregorianYear)
    {
        var saved = ResolveSavedRow(ed, slot.GradeBand, month, gregorianYear);
        if (saved == null)
            return null;

        return new AnnualComparisonInputValues(
            Role: saved.Role,
            Grade: saved.Grade,
            Seniority: FormatSavedSeniority(saved.Seniority),
            JobBase: saved.JobBase,
            CompareJobBase: true,
            JobPercent: saved.JobPercent,
            WeeklyHours: saved.WeeklyHours,
            GeneralMultiplier: saved.GeneralMultiplier);
    }

    private PayrollComparisonInputRow? ResolveSavedRow(
        EmploymentData ed,
        byte gradeBand,
        int month,
        int gregorianYear)
    {
        var candidates = new List<PayrollComparisonInputRow>();
        var tz = PayrollComparisonUploadSupport.NormalizeIdNumber(ed.Employee?.IdNumber);
        if (!string.IsNullOrEmpty(tz) && _byTz.TryGetValue((tz, month, gregorianYear), out var tzRows))
            candidates.AddRange(tzRows);

        if (ed.Employee?.EmployeeNumber is int empNum
            && _byNum.TryGetValue((empNum, month, gregorianYear), out var numRows))
        {
            foreach (var row in numRows)
            {
                if (!candidates.Contains(row))
                    candidates.Add(row);
            }
        }

        if (candidates.Count == 0)
            return null;

        var dbGrade = gradeBand == 1 ? ed.Grade1Grade : ed.Grade2Grade;
        return candidates.FirstOrDefault(r => PayrollComparisonUploadSupport.TextEqual(r.Grade, dbGrade))
               ?? candidates[0];
    }

    private static Dictionary<(string TzKey, int Month, int Year), List<PayrollComparisonInputRow>> IndexByTzMonth(
        IEnumerable<PayrollComparisonInputRow> rows) =>
        rows
            .Select(r => new { Row = r, Key = PayrollComparisonUploadSupport.NormalizeIdNumber(r.IdNumber) })
            .Where(x => !string.IsNullOrEmpty(x.Key))
            .GroupBy(x => (x.Key!, x.Row.Month, x.Row.GregorianYear))
            .ToDictionary(g => g.Key, g => g.Select(x => x.Row).ToList());

    private static Dictionary<(int EmpNum, int Month, int Year), List<PayrollComparisonInputRow>> IndexByEmpNumMonth(
        IEnumerable<PayrollComparisonInputRow> rows) =>
        rows
            .Select(r => new { Row = r, Num = ParseOketzEmployeeNumber(r.OketzEmployeeNumber) })
            .Where(x => x.Num.HasValue)
            .GroupBy(x => (x.Num!.Value, x.Row.Month, x.Row.GregorianYear))
            .ToDictionary(g => g.Key, g => g.Select(x => x.Row).ToList());

    private static int? ParseOketzEmployeeNumber(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        return int.TryParse(raw.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) ? n : null;
    }

    private static string FormatSavedSeniority(decimal? value) =>
        value.HasValue ? value.Value.ToString(CultureInfo.InvariantCulture) : string.Empty;
}

internal static class AnnualComparisonSavedRowMapper
{
    public static PayrollComparisonInputRow ToComparisonInput(Models.PayrollMonthlyInputRow entity) => new()
    {
        Month = entity.Month,
        GregorianYear = entity.GregorianYear,
        InstitutionSymbol = entity.InstitutionSymbol,
        OketzEmployeeNumber = entity.OketzEmployeeNumber,
        IdNumber = entity.IdNumber,
        FullName = entity.FullName,
        Role = entity.Role,
        Grade = entity.Grade,
        Seniority = entity.Seniority,
        WeeklyHours = entity.WeeklyHours,
        JobBase = entity.JobBase,
        JobPercent = entity.JobPercent,
        AgeHours = entity.AgeHours,
        TrainingBenefits = entity.TrainingBenefits,
        DoubleDegree = entity.DoubleDegree,
        TrainingFund = entity.TrainingFund,
        GeneralMultiplier = entity.GeneralMultiplier,
        SourceExcelRowNumber = entity.SourceExcelRowNumber ?? 0,
        RawCellsJson = entity.RawCellsJson,
        GradeBand = 1,
    };
}
