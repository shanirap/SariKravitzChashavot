using System.Globalization;
using System.Text.Json;
using AccountingProject.Contracts;
using AccountingProject.Models;

namespace AccountingProject.Services;

internal static class AnnualComparisonRowComputer
{
    internal sealed record ComputedRow(
        int SlotId,
        byte GradeBand,
        string InstitutionSymbol,
        string FullName,
        string Role,
        string SugMisraFromPayroll,
        string Grade,
        string Seniority,
        decimal? WeeklyHours,
        decimal? JobBase,
        decimal? JobPercent,
        decimal DoubleGeneral,
        Dictionary<string, string> MonthCells);

    internal sealed record DisplayRow(
        string InstitutionSymbol,
        string FullName,
        string Role,
        string SugMisraFromPayroll,
        string Grade,
        string Seniority,
        decimal? WeeklyHours,
        decimal? JobBase,
        decimal? JobPercent,
        decimal DoubleGeneral,
        Dictionary<string, string> MonthCells);

    public static ComputedRow Compute(
        EmploymentData ed,
        EmploymentDataSlot slot,
        IReadOnlyList<(int Month, int GregorianYear)> monthSequence,
        IAnnualComparisonMonthSource monthSource)
    {
        var g1 = slot.GradeBand == 1;
        var monthCells = new Dictionary<string, string>();
        foreach (var (month, gregYear) in monthSequence)
        {
            var key = $"{month}.{gregYear}";
            if (!monthSource.IsMonthCaptured(month, gregYear))
                monthCells[key] = AnnualComparisonReportBuilder.NotCapturedInInput;
            else
            {
                var input = monthSource.ResolveInput(ed, slot, month, gregYear);
                monthCells[key] = AnnualComparisonReportBuilder.BuildMonthCell(ed, slot, input);
            }
        }

        return new ComputedRow(
            slot.Id,
            slot.GradeBand,
            slot.InstitutionSymbol?.Trim() ?? "",
            ed.Employee?.FullName ?? "",
            (g1 ? ed.Grade1Role : ed.Grade2Role) ?? "",
            ResolvePayrollSugMisraDisplay(ed, slot, monthSequence, monthSource),
            (g1 ? ed.Grade1Grade : ed.Grade2Grade) ?? "",
            (g1 ? ed.Grade1Seniority : ed.Grade2Seniority) ?? "",
            slot.WeeklyHours,
            slot.JobBase,
            g1 ? ed.Grade1JobPercent : ed.Grade2JobPercent,
            0m,
            monthCells);
    }

    public static DisplayRow ToDisplay(ComputedRow computed, AnnualComparisonReportRowOverride? ovr)
    {
        if (ovr == null)
            return new DisplayRow(
                computed.InstitutionSymbol,
                computed.FullName,
                computed.Role,
                computed.SugMisraFromPayroll,
                computed.Grade,
                computed.Seniority,
                computed.WeeklyHours,
                computed.JobBase,
                computed.JobPercent,
                computed.DoubleGeneral,
                new Dictionary<string, string>(computed.MonthCells));

        var monthOverrides = ParseMonthCellsJson(ovr.MonthCellsJson);
        var monthCells = new Dictionary<string, string>(computed.MonthCells);
        foreach (var (key, value) in monthOverrides)
            monthCells[key] = value;

        return new DisplayRow(
            ovr.InstitutionSymbol ?? computed.InstitutionSymbol,
            ovr.FullName ?? computed.FullName,
            ovr.Role ?? computed.Role,
            ovr.SugMisraFromPayroll ?? computed.SugMisraFromPayroll,
            ovr.Grade ?? computed.Grade,
            ovr.Seniority ?? computed.Seniority,
            ovr.WeeklyHours ?? computed.WeeklyHours,
            ovr.JobBase ?? computed.JobBase,
            ovr.JobPercent ?? computed.JobPercent,
            ovr.DoubleGeneral ?? computed.DoubleGeneral,
            monthCells);
    }

    public static Dictionary<string, string> ParseMonthCellsJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new Dictionary<string, string>();

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, string>>(json)
                   ?? new Dictionary<string, string>();
        }
        catch
        {
            return new Dictionary<string, string>();
        }
    }

    public static string? SerializeMonthCellsJson(IReadOnlyDictionary<string, string>? monthCells)
    {
        if (monthCells == null || monthCells.Count == 0)
            return null;

        return JsonSerializer.Serialize(monthCells);
    }

    public static bool HasAnyOverride(ComputedRow computed, AnnualComparisonReportRowOverride entity)
    {
        if (!string.IsNullOrEmpty(entity.InstitutionSymbol)
            && !TextEqual(entity.InstitutionSymbol, computed.InstitutionSymbol))
            return true;
        if (!string.IsNullOrEmpty(entity.FullName) && !TextEqual(entity.FullName, computed.FullName))
            return true;
        if (!string.IsNullOrEmpty(entity.Role) && !TextEqual(entity.Role, computed.Role))
            return true;
        if (!string.IsNullOrEmpty(entity.SugMisraFromPayroll)
            && !TextEqual(entity.SugMisraFromPayroll, computed.SugMisraFromPayroll))
            return true;
        if (!string.IsNullOrEmpty(entity.Grade) && !TextEqual(entity.Grade, computed.Grade))
            return true;
        if (!string.IsNullOrEmpty(entity.Seniority) && !TextEqual(entity.Seniority, computed.Seniority))
            return true;
        if (entity.WeeklyHours.HasValue
            && !PayrollComparisonUploadSupport.DecimalsEqual(entity.WeeklyHours, computed.WeeklyHours))
            return true;
        if (entity.JobBase.HasValue
            && !PayrollComparisonUploadSupport.DecimalsEqual(entity.JobBase, computed.JobBase))
            return true;
        if (entity.JobPercent.HasValue
            && !PayrollComparisonUploadSupport.DecimalsEqual(entity.JobPercent, computed.JobPercent))
            return true;
        if (entity.DoubleGeneral.HasValue
            && !PayrollComparisonUploadSupport.DecimalsEqual(entity.DoubleGeneral, computed.DoubleGeneral))
            return true;

        var monthOverrides = ParseMonthCellsJson(entity.MonthCellsJson);
        foreach (var (key, value) in monthOverrides)
        {
            if (!computed.MonthCells.TryGetValue(key, out var computedValue)
                || !TextEqual(value, computedValue))
                return true;
        }

        return false;
    }

    public static AnnualComparisonReportRowOverride BuildOverrideEntity(
        int employerId,
        string academicYear,
        ComputedRow computed,
        AnnualComparisonOverrideRowSaveDto save,
        AnnualComparisonReportRowOverride? existing)
    {
        var entity = existing ?? new AnnualComparisonReportRowOverride
        {
            EmployerId = employerId,
            AcademicYear = academicYear,
            SlotId = save.SlotId,
            CreatedAtUtc = DateTime.UtcNow,
        };

        entity.InstitutionSymbol = Differs(save.InstitutionSymbol, computed.InstitutionSymbol)
            ? NullIfEmpty(save.InstitutionSymbol)
            : null;
        entity.FullName = Differs(save.FullName, computed.FullName) ? NullIfEmpty(save.FullName) : null;
        entity.Role = Differs(save.Role, computed.Role) ? NullIfEmpty(save.Role) : null;
        entity.SugMisraFromPayroll = Differs(save.SugMisraFromPayroll, computed.SugMisraFromPayroll)
            ? NullIfEmpty(save.SugMisraFromPayroll)
            : null;
        entity.Grade = Differs(save.Grade, computed.Grade) ? NullIfEmpty(save.Grade) : null;
        entity.Seniority = Differs(save.Seniority, computed.Seniority) ? NullIfEmpty(save.Seniority) : null;
        entity.WeeklyHours = DiffersDecimal(save.WeeklyHours, computed.WeeklyHours) ? save.WeeklyHours : null;
        entity.JobBase = DiffersDecimal(save.JobBase, computed.JobBase) ? save.JobBase : null;
        entity.JobPercent = DiffersDecimal(save.JobPercent, computed.JobPercent) ? save.JobPercent : null;
        entity.DoubleGeneral = DiffersDecimal(save.DoubleGeneral, computed.DoubleGeneral)
            ? save.DoubleGeneral
            : null;

        var monthOverrides = new Dictionary<string, string>();
        if (save.MonthCells != null)
        {
            foreach (var (key, value) in save.MonthCells)
            {
                var computedValue = computed.MonthCells.GetValueOrDefault(key) ?? "";
                if (!TextEqual(value, computedValue))
                    monthOverrides[key] = value?.Trim() ?? "";
            }
        }

        entity.MonthCellsJson = SerializeMonthCellsJson(monthOverrides);
        entity.IsManualEdited = HasAnyOverride(computed, entity);
        entity.ManualEditNote = NullIfEmpty(save.ManualEditNote);
        entity.UpdatedAtUtc = DateTime.UtcNow;

        return entity;
    }

    private static string ResolvePayrollSugMisraDisplay(
        EmploymentData ed,
        EmploymentDataSlot slot,
        IReadOnlyList<(int Month, int GregorianYear)> monthSequence,
        IAnnualComparisonMonthSource monthSource)
    {
        foreach (var (month, gregYear) in monthSequence)
        {
            if (!monthSource.IsMonthCaptured(month, gregYear))
                continue;
            var input = monthSource.ResolveInput(ed, slot, month, gregYear);
            var sugMisra = input?.Role;
            if (!string.IsNullOrWhiteSpace(sugMisra))
                return sugMisra.Trim();
        }

        return "";
    }

    private static bool Differs(string? display, string computed) =>
        !TextEqual(display ?? "", computed);

    private static bool DiffersDecimal(decimal? display, decimal? computed) =>
        !PayrollComparisonUploadSupport.DecimalsEqual(display, computed);

    private static string? NullIfEmpty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static bool TextEqual(string? a, string? b) =>
        PayrollComparisonUploadSupport.TextEqual(a, b);

    internal static string FormatDecimal(decimal? value) =>
        value.HasValue
            ? value.Value.ToString(CultureInfo.InvariantCulture)
            : "";

    internal static string FormatDecimal(decimal value) =>
        value.ToString(CultureInfo.InvariantCulture);
}
