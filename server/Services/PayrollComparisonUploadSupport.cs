using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using AccountingProject.Domain;
using AccountingProject.Infrastructure;
using ClosedXML.Excel;

namespace AccountingProject.Services;

/// <summary>קריאה ונרמול משותפים לקובץ עוקץ (דוח השוואה חודשי/שנתי).</summary>
internal static class PayrollComparisonUploadSupport
{
    internal const decimal NumericTolerance = 0.01m;

    internal static PayrollUploadLayout ParseLayout(IXLWorksheet sheet)
    {
        var lastRow = sheet.LastRowUsed()?.RowNumber() ?? 1;
        var headerRowNum = FindHeaderRow(sheet, lastRow)
                           ?? throw new InvalidOperationException("לא נמצאה שורת כותרות בקובץ (מספר עובד / ת\"ז / חודש משכורת).");

        var layout = new PayrollUploadLayout { HeaderRowNumber = headerRowNum };
        var headerRow = sheet.Row(headerRowNum);
        var bandOcc = new Dictionary<string, int>(StringComparer.Ordinal);

        foreach (var cell in headerRow.CellsUsed())
        {
            var col = cell.Address.ColumnNumber;
            var key = NormalizeHeaderKey(ExcelCellText.Get(cell));
            if (string.IsNullOrEmpty(key)) continue;

            if (MatchesAny(key, "מספר_עובד", "מספר_עובד_בעוקץ"))
                layout.ColEmployeeNumber = col;
            else if (MatchesAny(key, "תז", "ת\"ז", "ת.ז.", "ת._ז."))
                layout.ColTz = col;
            else if (MatchesAny(key, "שם_כולל", "שם_מלא"))
                layout.ColFullName = col;
            else if (MatchesAny(key, "חודש_משכורת", "חודש"))
                layout.ColMonth = col;
            else if (MatchesAny(key, "שנה"))
                layout.ColYear = col;
            else if (MatchesAny(key, "סוג_משרה"))
                layout.ColSugMisra = col;
            else if (TryParseMisraHoursKey(key, out var hIdx))
                AssignBandColumn(bandOcc, $"misra_h_{hIdx}", col, layout, (b, c) => b.MisraHours[hIdx - 1] = c);
            else if (TryParseMisraBaseKey(key, out var bIdx))
                AssignBandColumn(bandOcc, $"misra_b_{bIdx}", col, layout, (b, c) => b.MisraBase[bIdx - 1] = c);
            else if (MatchesAny(key, "שם_הדירוג"))
                AssignBandColumn(bandOcc, "grade_name", col, layout, (b, c) => b.GradeName = c);
            else if (MatchesAny(key, "דרגה"))
                AssignBandColumn(bandOcc, "grade", col, layout, (b, c) => b.Grade = c);
            else if (key.Contains("ותק", StringComparison.Ordinal))
                AssignBandColumn(bandOcc, "seniority", col, layout, (b, c) => b.Seniority = c);
            else if (MatchesAny(key, "אחוז_משרה_מחושב", "אחוז_משרה"))
                AssignBandColumn(bandOcc, "job_percent", col, layout, (b, c) => b.JobPercent = c);
            else if (MatchesAny(key, "אחוז_תוספת_אם"))
                AssignBandColumn(bandOcc, "mother_benefit", col, layout, (b, c) => b.MotherBenefit = c);
            else if (MatchesAny(key, "אחוז_הפרשה_לקרן_השתלמות", "קרן_השתלמות"))
                AssignBandColumn(bandOcc, "training_fund", col, layout, (b, c) => b.TrainingFund = c);
            else if (MatchesAny(key, "שעות_גיל_מחושב", "שעות_גיל"))
                AssignBandColumn(bandOcc, "age_hours", col, layout, (b, c) => b.AgeHours = c);
            else if (key.Contains("הכפלה_כללית", StringComparison.Ordinal))
                AssignBandColumn(bandOcc, "double_general", col, layout, (b, c) => b.DoubleGeneral = c);
            else if (MatchesAny(key, "מס'_גמולים", "גמולי_השתלמות", "גמולים"))
                AssignBandColumn(bandOcc, "training_benefits", col, layout, (b, c) => b.TrainingBenefits = c);
            else if (MatchesAny(key, "כפל_תואר"))
                AssignBandColumn(bandOcc, "double_degree", col, layout, (b, c) => b.DoubleDegree = c);
        }

        return layout;
    }

    internal static List<PayrollUploadRow> ParseRowsForMonth(
        IXLWorksheet sheet,
        PayrollUploadLayout layout,
        int month,
        int expectedGregorianYear,
        string academicYear)
    {
        var canonYear = CanonAcademicYear(academicYear);
        var list = new List<PayrollUploadRow>();
        var lastRow = sheet.LastRowUsed()?.RowNumber() ?? layout.HeaderRowNumber;

        var monthAllowed = new[] { (month, expectedGregorianYear) };
        for (var r = layout.HeaderRowNumber + 1; r <= lastRow; r++)
        {
            var row = sheet.Row(r);
            if (!TryParseDataRow(row, layout, out var parsed, monthAllowed)) continue;
            if (parsed.Month != month) continue;

            if (layout.ColYear.HasValue)
            {
                if (parsed.GregorianYear != expectedGregorianYear) continue;
                var rowAy = CanonAcademicYear(
                    SchoolYearGregorian.GetSchoolYearFromGregorianMonth(parsed.Month, parsed.GregorianYear));
                if (!string.Equals(rowAy, canonYear, StringComparison.Ordinal)) continue;
            }

            list.Add(parsed);
        }

        return list;
    }

    internal static List<PayrollUploadRow> ParseAllRows(
        IXLWorksheet sheet,
        PayrollUploadLayout layout,
        string academicYear,
        IReadOnlyList<(int Month, int GregorianYear)> allowedMonths)
    {
        var canonYear = CanonAcademicYear(academicYear);
        var allowed = allowedMonths.ToHashSet();
        var list = new List<PayrollUploadRow>();
        var lastRow = sheet.LastRowUsed()?.RowNumber() ?? layout.HeaderRowNumber;

        for (var r = layout.HeaderRowNumber + 1; r <= lastRow; r++)
        {
            var row = sheet.Row(r);
            if (!TryParseDataRow(row, layout, out var parsed, allowedMonths)) continue;
            if (!allowed.Contains((parsed.Month, parsed.GregorianYear))) continue;

            var rowAy = CanonAcademicYear(
                SchoolYearGregorian.GetSchoolYearFromGregorianMonth(parsed.Month, parsed.GregorianYear));
            if (!string.Equals(rowAy, canonYear, StringComparison.Ordinal)) continue;

            list.Add(parsed);
        }

        return list;
    }

    internal static List<PayrollComparisonInputRow> ParseInputRowsForMonth(
        IXLWorksheet sheet,
        PayrollUploadLayout layout,
        int month,
        int expectedGregorianYear,
        string academicYear,
        bool includeRawCellsJson = false) =>
        MapUploadRowsToInputRows(
            ParseRowsForMonth(sheet, layout, month, expectedGregorianYear, academicYear),
            layout,
            includeRawCellsJson);

    internal static List<PayrollComparisonInputRow> ParseAllInputRows(
        IXLWorksheet sheet,
        PayrollUploadLayout layout,
        string academicYear,
        IReadOnlyList<(int Month, int GregorianYear)> allowedMonths) =>
        MapUploadRowsToInputRows(
            ParseAllRows(sheet, layout, academicYear, allowedMonths),
            layout);

    internal static PayrollComparisonInputRow MapInputRow(
        IXLRow row,
        PayrollUploadLayout layout,
        byte gradeBand,
        byte slotIndex,
        int month,
        int gregorianYear,
        string? rawIdNumber,
        int? employeeNumber,
        bool includeRawCellsJson = false)
    {
        var band = BandCols(layout, gradeBand);
        var empNumRaw = employeeNumber?.ToString(CultureInfo.InvariantCulture)
                        ?? GetCell(row, layout.ColEmployeeNumber);
        var tzRaw = rawIdNumber ?? GetCell(row, layout.ColTz);

        return new PayrollComparisonInputRow
        {
            Month = month,
            GregorianYear = gregorianYear,
            InstitutionSymbol = null,
            OketzEmployeeNumber = NullIfWhiteSpace(empNumRaw),
            IdNumber = NullIfWhiteSpace(tzRaw),
            FullName = NullIfWhiteSpace(GetCell(row, layout.ColFullName)),
            Role = NullIfWhiteSpace(GetCell(row, layout.ColSugMisra)),
            Grade = NullIfWhiteSpace(GetCell(row, band.Grade)),
            Seniority = ParseDecimal(GetCell(row, band.Seniority)),
            WeeklyHours = SumMisraHours(row, band),
            JobBase = ReadSlotJobBase(row, band, slotIndex),
            JobPercent = ParseDecimal(GetCell(row, band.JobPercent)),
            AgeHours = ParseDecimal(GetCell(row, band.AgeHours)),
            TrainingBenefits = ReadOptionalDecimal(row, band.TrainingBenefits),
            DoubleDegree = ReadOptionalDecimal(row, band.DoubleDegree),
            TrainingFund = ParseDecimal(GetCell(row, band.TrainingFund)),
            GeneralMultiplier = ParseDecimal(GetCell(row, band.DoubleGeneral)),
            SourceExcelRowNumber = row.RowNumber(),
            RawCellsJson = includeRawCellsJson ? BuildRawCellsJson(row, layout, band) : null,
            GradeBand = gradeBand,
        };
    }

    internal static bool TryParseDataRow(
        IXLRow row,
        PayrollUploadLayout layout,
        out PayrollUploadRow parsed,
        IReadOnlyList<(int Month, int GregorianYear)>? allowedMonths = null)
    {
        parsed = null!;
        if (IsEmptyDataRow(row, layout)) return false;
        if (!TryParseMonth(row, layout, out var month)) return false;

        var gregYear = layout.ColYear.HasValue && TryParseYear(row, layout, out var y) ? y : 0;
        if (layout.ColYear.HasValue && gregYear == 0) return false;
        if (gregYear == 0 && allowedMonths != null)
            gregYear = InferGregorianYearForMonth(month, allowedMonths);
        if (gregYear == 0) return false;

        var tzRaw = GetCell(row, layout.ColTz);
        var empRaw = GetCell(row, layout.ColEmployeeNumber);
        int? empNum = int.TryParse(empRaw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var en) ? en : null;
        if (string.IsNullOrWhiteSpace(tzRaw) && empNum == null) return false;

        parsed = new PayrollUploadRow(
            row,
            string.IsNullOrWhiteSpace(tzRaw) ? null : tzRaw.Trim(),
            NormalizeIdNumber(tzRaw),
            empNum,
            month,
            gregYear);
        return true;
    }

    internal static PayrollUploadRow? ResolveForEmployee(
        Models.Employee? emp,
        int month,
        int gregorianYear,
        IReadOnlyDictionary<(string TzKey, int Month, int Year), PayrollUploadRow> byTzMonth,
        IReadOnlyDictionary<(int EmpNum, int Month, int Year), PayrollUploadRow> byNumMonth)
    {
        if (emp == null) return null;
        var tz = NormalizeIdNumber(emp.IdNumber);
        if (!string.IsNullOrEmpty(tz) && byTzMonth.TryGetValue((tz, month, gregorianYear), out var r1))
            return r1;
        if (emp.EmployeeNumber.HasValue
            && byNumMonth.TryGetValue((emp.EmployeeNumber.Value, month, gregorianYear), out var r2))
            return r2;
        return null;
    }

    internal static Dictionary<(string TzKey, int Month, int Year), PayrollUploadRow> IndexByTzMonth(
        IEnumerable<PayrollUploadRow> rows) =>
        rows.Where(r => !string.IsNullOrEmpty(r.NormalizedIdNumber))
            .GroupBy(r => (r.NormalizedIdNumber!, r.Month, r.GregorianYear))
            .ToDictionary(g => g.Key, g => g.First());

    internal static Dictionary<(int EmpNum, int Month, int Year), PayrollUploadRow> IndexByEmpNumMonth(
        IEnumerable<PayrollUploadRow> rows) =>
        rows.Where(r => r.EmployeeNumber.HasValue)
            .GroupBy(r => (r.EmployeeNumber!.Value, r.Month, r.GregorianYear))
            .ToDictionary(g => g.Key, g => g.First());

    internal static bool SlotIsEmpty(Models.EmploymentDataSlot slot) =>
        string.IsNullOrWhiteSpace(slot.InstitutionSymbol)
        && (!slot.WeeklyHours.HasValue || slot.WeeklyHours.Value == 0);

    internal static PayrollBandColumns BandCols(PayrollUploadLayout layout, byte gradeBand) =>
        gradeBand == 2 ? layout.Band2 : layout.Band1;

    internal static string GetCell(IXLRow row, int? col) =>
        col.HasValue ? ExcelCellText.Get(row.Cell(col.Value)).Trim() : string.Empty;

    internal static decimal SumMisraHours(PayrollUploadRow row, PayrollBandColumns band) =>
        SumMisraHours(row.Row, band);

    internal static decimal SumMisraHours(IXLRow row, PayrollBandColumns band)
    {
        decimal sum = 0;
        for (var i = 0; i < 6; i++)
        {
            var c = band.MisraHours[i];
            if (!c.HasValue) continue;
            sum += ParseDecimal(GetCell(row, c)) ?? 0;
        }
        return sum;
    }

    internal static decimal? ReadSlotJobBase(IXLRow row, PayrollBandColumns bandCols, byte slotIndex)
    {
        if (slotIndex is < 1 or > 6) return null;
        var col = bandCols.MisraBase[slotIndex - 1];
        if (!col.HasValue) return null;
        return ParseDecimal(GetCell(row, col));
    }

    internal static decimal? ReadOptionalDecimal(IXLRow row, int? col)
    {
        if (!col.HasValue) return null;
        return ParseDecimal(GetCell(row, col));
    }

    internal static decimal SumSystemWeeklyHours(Models.EmploymentData ed, byte gradeBand) =>
        ed.Slots.Where(s => s.GradeBand == gradeBand && !SlotIsEmpty(s))
            .Sum(s => s.WeeklyHours ?? 0);

    internal static bool TextEqual(string? a, string? b)
    {
        var na = NormalizeText(a);
        var nb = NormalizeText(b);
        return string.Equals(na, nb, StringComparison.Ordinal);
    }

    internal static string NormalizeText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        return Regex.Replace(value.Trim(), @"\s+", " ");
    }

    internal static decimal? ParseDecimal(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        return decimal.TryParse(raw.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : null;
    }

    internal static bool DecimalsEqual(decimal? db, decimal? excel)
    {
        if (!db.HasValue && (excel is null or 0)) return true;
        if (!db.HasValue || excel is null) return false;

        var a = db.Value;
        var b = excel.Value;
        if (Math.Abs(a - b) <= NumericTolerance) return true;

        if (b > 0 && b < 1 && a >= 1 && Math.Abs(a - b * 100m) <= NumericTolerance) return true;
        if (a > 0 && a < 1 && b >= 1 && Math.Abs(b - a * 100m) <= NumericTolerance) return true;

        return false;
    }

    internal static string FormatDecimal(decimal? v) =>
        v.HasValue ? v.Value.ToString(CultureInfo.InvariantCulture) : "";

    internal static string NormalizeIdNumber(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return string.Empty;
        return raw.Trim().Replace("-", "", StringComparison.Ordinal).Replace(" ", "", StringComparison.Ordinal);
    }

    internal static string CanonAcademicYear(string? stored)
    {
        var n = HebrewAcademicYear.Normalize(stored?.Trim());
        return string.IsNullOrWhiteSpace(n) ? (stored ?? "").Trim() : n.Trim();
    }

    private static int InferGregorianYearForMonth(
        int month,
        IReadOnlyList<(int Month, int GregorianYear)> allowedMonths)
    {
        var match = allowedMonths.FirstOrDefault(m => m.Month == month);
        return match.GregorianYear > 0 ? match.GregorianYear : 0;
    }

    internal static int ResolveExpectedGregorianYear(
        string academicYear, int month, Func<string, int> parseSeptemberGregorianYear)
    {
        var sepYear = parseSeptemberGregorianYear(academicYear);
        var seq = SchoolYearGregorian.GetSchoolYearMonthSequence(sepYear);
        var match = seq.FirstOrDefault(p => p.Month == month);
        return match.GregorianYear > 0 ? match.GregorianYear : sepYear;
    }

    private static void AssignBandColumn(
        Dictionary<string, int> bandOcc,
        string fieldKey,
        int col,
        PayrollUploadLayout layout,
        Action<PayrollBandColumns, int> assign)
    {
        bandOcc.TryGetValue(fieldKey, out var n);
        var target = n == 0 ? layout.Band1 : layout.Band2;
        assign(target, col);
        bandOcc[fieldKey] = n + 1;
    }

    private static int? FindHeaderRow(IXLWorksheet sheet, int lastRow)
    {
        var scanTo = Math.Min(lastRow, 25);
        for (var r = 1; r <= scanTo; r++)
        {
            var keys = new HashSet<string>(StringComparer.Ordinal);
            foreach (var cell in sheet.Row(r).CellsUsed())
            {
                var k = NormalizeHeaderKey(ExcelCellText.Get(cell));
                if (!string.IsNullOrEmpty(k)) keys.Add(k);
            }

            var hasId = keys.Any(k => MatchesAny(k, "מספר_עובד", "מספר_עובד_בעוקץ", "תז", "ת\"ז", "ת.ז.", "ת._ז."));
            var hasMonth = keys.Any(k => MatchesAny(k, "חודש_משכורת", "חודש"));
            if (hasId && hasMonth) return r;
        }

        return null;
    }

    private static bool IsEmptyDataRow(IXLRow row, PayrollUploadLayout layout)
    {
        int?[] cols = [layout.ColTz, layout.ColEmployeeNumber, layout.ColFullName, layout.ColMonth];
        foreach (var c in cols)
        {
            if (c.HasValue && !string.IsNullOrWhiteSpace(GetCell(row, c))) return false;
        }
        return true;
    }

    private static bool TryParseMonth(IXLRow row, PayrollUploadLayout layout, out int month)
    {
        month = 0;
        if (!layout.ColMonth.HasValue) return false;
        return int.TryParse(GetCell(row, layout.ColMonth), NumberStyles.Integer, CultureInfo.InvariantCulture, out month)
               && month is >= 1 and <= 12;
    }

    private static bool TryParseYear(IXLRow row, PayrollUploadLayout layout, out int year)
    {
        year = 0;
        if (!layout.ColYear.HasValue) return false;
        return int.TryParse(GetCell(row, layout.ColYear), NumberStyles.Integer, CultureInfo.InvariantCulture, out year)
               && year >= 1900;
    }

    private static string NormalizeHeaderKey(string raw)
    {
        var parts = raw.Trim().Split([' ', '\t', '\u00A0'], StringSplitOptions.RemoveEmptyEntries);
        return string.Join("_", parts).Trim();
    }

    private static bool MatchesAny(string key, params string[] patterns) =>
        patterns.Any(p => string.Equals(key, NormalizeHeaderKey(p), StringComparison.OrdinalIgnoreCase));

    private static readonly Regex MisraHoursRegex = new(
        @"^(מישרה|משרה)_(\d+)[-_]?שעות$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex MisraBaseRegex = new(
        @"^(מישרה|משרה)_(\d+)[-_]?מתוך[_-]?שעות$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static bool TryParseMisraHoursKey(string key, out int index)
    {
        index = 0;
        var m = MisraHoursRegex.Match(key);
        if (!m.Success) return false;
        return int.TryParse(m.Groups[2].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out index)
               && index is >= 1 and <= 6;
    }

    private static bool TryParseMisraBaseKey(string key, out int index)
    {
        index = 0;
        var m = MisraBaseRegex.Match(key);
        if (!m.Success) return false;
        return int.TryParse(m.Groups[2].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out index)
               && index is >= 1 and <= 6;
    }

    private static List<PayrollComparisonInputRow> MapUploadRowsToInputRows(
        IReadOnlyList<PayrollUploadRow> uploadRows,
        PayrollUploadLayout layout,
        bool includeRawCellsJson = false)
    {
        var list = new List<PayrollComparisonInputRow>();
        foreach (var upload in uploadRows)
        {
            var bands = new List<byte>();
            if (BandRowHasData(upload.Row, layout, layout.Band1)) bands.Add(1);
            if (BandRowHasData(upload.Row, layout, layout.Band2)) bands.Add(2);
            if (bands.Count == 0) bands.Add(1);

            foreach (var gradeBand in bands)
            {
                var slotIndex = ResolveJobBaseSlotIndex(upload.Row, BandCols(layout, gradeBand));
                list.Add(MapInputRow(
                    upload.Row,
                    layout,
                    gradeBand,
                    slotIndex,
                    upload.Month,
                    upload.GregorianYear,
                    upload.RawIdNumber,
                    upload.EmployeeNumber,
                    includeRawCellsJson));
            }
        }

        return list;
    }

    private static bool BandRowHasData(IXLRow row, PayrollUploadLayout layout, PayrollBandColumns band)
    {
        int?[] scalarCols =
        [
            band.GradeName, band.Grade, band.Seniority, band.JobPercent, band.MotherBenefit,
            band.TrainingFund, band.AgeHours, band.DoubleGeneral, band.TrainingBenefits, band.DoubleDegree,
        ];
        foreach (var col in scalarCols)
        {
            if (col.HasValue && !string.IsNullOrWhiteSpace(GetCell(row, col))) return true;
        }

        for (var i = 0; i < 6; i++)
        {
            if (band.MisraHours[i].HasValue && !string.IsNullOrWhiteSpace(GetCell(row, band.MisraHours[i]))) return true;
            if (band.MisraBase[i].HasValue && !string.IsNullOrWhiteSpace(GetCell(row, band.MisraBase[i]))) return true;
        }

        return false;
    }

    private static byte ResolveJobBaseSlotIndex(IXLRow row, PayrollBandColumns band)
    {
        for (byte i = 1; i <= 6; i++)
        {
            if (ReadSlotJobBase(row, band, i).HasValue) return i;
        }

        return 1;
    }

    private static string? NullIfWhiteSpace(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? BuildRawCellsJson(IXLRow row, PayrollUploadLayout layout, PayrollBandColumns band)
    {
        var cells = new Dictionary<string, string?>(StringComparer.Ordinal);
        void Add(string key, int? col)
        {
            if (!col.HasValue) return;
            cells[key] = GetCell(row, col);
        }

        Add("employee_number", layout.ColEmployeeNumber);
        Add("id_number", layout.ColTz);
        Add("full_name", layout.ColFullName);
        Add("month", layout.ColMonth);
        Add("year", layout.ColYear);
        Add("role", layout.ColSugMisra);
        Add("grade_name", band.GradeName);
        Add("grade", band.Grade);
        Add("seniority", band.Seniority);
        Add("job_percent", band.JobPercent);
        Add("mother_benefit", band.MotherBenefit);
        Add("training_fund", band.TrainingFund);
        Add("age_hours", band.AgeHours);
        Add("general_multiplier", band.DoubleGeneral);
        Add("training_benefits", band.TrainingBenefits);
        Add("double_degree", band.DoubleDegree);
        for (var i = 0; i < 6; i++)
        {
            Add($"misra_hours_{i + 1}", band.MisraHours[i]);
            Add($"misra_base_{i + 1}", band.MisraBase[i]);
        }

        return JsonSerializer.Serialize(cells);
    }
}

internal sealed class PayrollUploadLayout
{
    public int HeaderRowNumber { get; set; }
    public int? ColEmployeeNumber { get; set; }
    public int? ColTz { get; set; }
    public int? ColFullName { get; set; }
    public int? ColMonth { get; set; }
    public int? ColYear { get; set; }
    public int? ColSugMisra { get; set; }
    public PayrollBandColumns Band1 { get; } = new();
    public PayrollBandColumns Band2 { get; } = new();
}

internal sealed class PayrollBandColumns
{
    public int? GradeName { get; set; }
    public int? Grade { get; set; }
    public int? Seniority { get; set; }
    public int? JobPercent { get; set; }
    public int? MotherBenefit { get; set; }
    public int? TrainingFund { get; set; }
    public int? AgeHours { get; set; }
    public int? DoubleGeneral { get; set; }
    public int? TrainingBenefits { get; set; }
    public int? DoubleDegree { get; set; }
    public int?[] MisraHours { get; } = new int?[6];
    public int?[] MisraBase { get; } = new int?[6];
}

internal sealed record PayrollUploadRow(
    IXLRow Row,
    string? RawIdNumber,
    string NormalizedIdNumber,
    int? EmployeeNumber,
    int Month,
    int GregorianYear);
