using AccountingProject.Services;
using AccountingProject.Tests.TestHelpers;
using ClosedXML.Excel;

namespace AccountingProject.Tests;

public sealed class PayrollComparisonUploadSupportTests
{
    private const string Year = "תשפ\"ו";

    [Theory]
    [InlineData("12-345-678", "12345678")]
    [InlineData(" 99 88 77 ", "998877")]
    [InlineData(null, "")]
    public void NormalizeIdNumber_StripsSeparators(string? raw, string expected)
    {
        Assert.Equal(expected, PayrollComparisonUploadSupport.NormalizeIdNumber(raw));
    }

    [Fact]
    public void DecimalsEqual_TreatsDbFractionAndExcelPercentAsEqual()
    {
        Assert.True(PayrollComparisonUploadSupport.DecimalsEqual(0.5m, 50m));
    }

    [Fact]
    public void DecimalsEqual_TreatsEqualValuesAsMatch()
    {
        Assert.True(PayrollComparisonUploadSupport.DecimalsEqual(100m, 100m));
        Assert.True(PayrollComparisonUploadSupport.DecimalsEqual(null, 0m));
        Assert.True(PayrollComparisonUploadSupport.DecimalsEqual(null, null));
    }

    [Fact]
    public void DecimalsEqual_RejectsLargeMismatch()
    {
        Assert.False(PayrollComparisonUploadSupport.DecimalsEqual(100m, 50m));
    }

    [Fact]
    public void ParseLayout_MissingHeaders_ThrowsHeaderNotFound()
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("נתונים");
        ws.Cell(1, 1).Value = "לא קשור";

        var ex = Assert.Throws<InvalidOperationException>(() =>
            PayrollComparisonUploadSupport.ParseLayout(ws));

        Assert.Contains("לא נמצאה שורת כותרות", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ParseInputRowsForMonth_DualBandRow_CreatesTwoInputRows()
    {
        using var upload = MonthlyComparisonUploadWorkbook.Create(
            "111222333", 5001, "Dual Band", 9, 2025, b => b.Band1().Band2());
        using var wb = new XLWorkbook(upload);
        var sheet = wb.Worksheet(1);
        var layout = PayrollComparisonUploadSupport.ParseLayout(sheet);

        var rows = PayrollComparisonUploadSupport.ParseInputRowsForMonth(
            sheet, layout, 9, 2025, Year, includeRawCellsJson: false);

        Assert.Equal(2, rows.Count);
        Assert.Contains(rows, r => r.GradeBand == 1);
        Assert.Contains(rows, r => r.GradeBand == 2);
    }

    [Fact]
    public void ResolveExpectedGregorianYear_SeptemberAndJanuary_DifferByCalendarYear()
    {
        int ParseYear(string y) => 2025;

        var sep = PayrollComparisonUploadSupport.ResolveExpectedGregorianYear(Year, 9, ParseYear);
        var jan = PayrollComparisonUploadSupport.ResolveExpectedGregorianYear(Year, 1, ParseYear);

        Assert.Equal(2025, sep);
        Assert.Equal(2026, jan);
    }

    [Theory]
    [InlineData("גננת", "גננת", true)]
    [InlineData("גננת", "  גננת  ", true)]
    [InlineData("גננת", "מורה", false)]
    [InlineData(null, "", true)]
    public void TextEqual_NormalizesWhitespace(string? a, string? b, bool expected)
    {
        Assert.Equal(expected, PayrollComparisonUploadSupport.TextEqual(a, b));
    }

    [Fact]
    public void CanonAcademicYear_TrimsInput()
    {
        var canon = PayrollComparisonUploadSupport.CanonAcademicYear("  תשפ\"ו  ");
        Assert.False(string.IsNullOrWhiteSpace(canon));
        Assert.Contains("תשפ", canon, StringComparison.Ordinal);
    }
}
