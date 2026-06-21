using System.Globalization;
using AccountingProject.Infrastructure;
using ClosedXML.Excel;

namespace AccountingProject.Tests;

public sealed class ExcelDateParserTests
{
    public static IEnumerable<object[]> SupportedTextFormats =>
    [
        ["1990-01-15", new DateOnly(1990, 1, 15)],
        ["1990-1-5", new DateOnly(1990, 1, 5)],
        ["25/08/1970", new DateOnly(1970, 8, 25)],
        ["5/3/2015", new DateOnly(2015, 3, 5)],
        ["15/03/2015", new DateOnly(2015, 3, 15)],
        ["25-08-1970", new DateOnly(1970, 8, 25)],
        ["5-3-2015", new DateOnly(2015, 3, 5)],
        ["25.08.1970", new DateOnly(1970, 8, 25)],
    ];

    [Theory]
    [MemberData(nameof(SupportedTextFormats))]
    public void TryParseText_ParsesKnownFormats_UnderEnUsCulture(string input, DateOnly expected)
    {
        using var _ = CultureScope.Create("en-US");
        Assert.True(ExcelDateParser.TryParseText(input, out var date));
        Assert.Equal(expected, date);
    }

    [Theory]
    [MemberData(nameof(SupportedTextFormats))]
    public void TryParseText_ParsesKnownFormats_UnderHeIlCulture(string input, DateOnly expected)
    {
        using var _ = CultureScope.Create("he-IL");
        Assert.True(ExcelDateParser.TryParseText(input, out var date));
        Assert.Equal(expected, date);
    }

    [Fact]
    public void TryParseText_IsoRoundtripFromExcelCellText_UnderEnUsCulture()
    {
        using var _ = CultureScope.Create("en-US");
        var iso = new DateTime(1970, 8, 25).ToString("o");
        Assert.True(ExcelDateParser.TryParseText(iso, out var date));
        Assert.Equal(new DateOnly(1970, 8, 25), date);
    }

    [Fact]
    public void TryParseText_BareTryParseFailsOnEnUs_ForDdMmYyyy()
    {
        using var _ = CultureScope.Create("en-US");
        Assert.False(DateOnly.TryParse("25/08/1970", out var unused));
        Assert.True(ExcelDateParser.TryParseText("25/08/1970", out var date));
        Assert.Equal(new DateOnly(1970, 8, 25), date);
    }

    [Fact]
    public void TryParse_FromDateTimeCell_UnderEnUsCulture()
    {
        using var _ = CultureScope.Create("en-US");
        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("dates");
        ws.Cell(1, 1).Value = new DateTime(2015, 3, 15);

        Assert.True(ExcelDateParser.TryParse(ws.Cell(1, 1), out var date));
        Assert.Equal(new DateOnly(2015, 3, 15), date);
    }

    [Fact]
    public void TryParse_FromTextCell_UnderEnUsCulture()
    {
        using var _ = CultureScope.Create("en-US");
        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("dates");
        ws.Cell(1, 1).Value = "25/08/1970";

        Assert.True(ExcelDateParser.TryParse(ws.Cell(1, 1), out var date));
        Assert.Equal(new DateOnly(1970, 8, 25), date);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("לא-תאריך")]
    [InlineData("4242")]
    public void TryParseText_ReturnsFalse_ForInvalidOrNonDate(string input)
    {
        using var _ = CultureScope.Create("en-US");
        Assert.False(ExcelDateParser.TryParseText(input, out var unused));
    }

    private sealed class CultureScope : IDisposable
    {
        private readonly CultureInfo _previousCulture;
        private readonly CultureInfo _previousUiCulture;

        private CultureScope(CultureInfo previousCulture, CultureInfo previousUiCulture)
        {
            _previousCulture = previousCulture;
            _previousUiCulture = previousUiCulture;
        }

        public static CultureScope Create(string name)
        {
            var previousCulture = CultureInfo.CurrentCulture;
            var previousUiCulture = CultureInfo.CurrentUICulture;
            var culture = new CultureInfo(name);
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
            return new CultureScope(previousCulture, previousUiCulture);
        }

        public void Dispose()
        {
            CultureInfo.CurrentCulture = _previousCulture;
            CultureInfo.CurrentUICulture = _previousUiCulture;
        }
    }
}
