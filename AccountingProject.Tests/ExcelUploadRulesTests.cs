using AccountingProject.Infrastructure;
using Microsoft.AspNetCore.Http;

namespace AccountingProject.Tests;

public sealed class ExcelUploadRulesTests
{
    [Fact]
    public void TryValidateStrictXlsx_AcceptsXlsx()
    {
        var file = BuildFile("import.xlsx", "abc");

        var ok = ExcelUploadRules.TryValidateStrictXlsx(file, ExcelUploadRules.BulkImportMaxBytes, out var error);

        Assert.True(ok);
        Assert.Null(error);
    }

    [Fact]
    public void TryValidateStrictXlsx_RejectsXlsm()
    {
        var file = BuildFile("macro.xlsm", "abc");

        var ok = ExcelUploadRules.TryValidateStrictXlsx(file, ExcelUploadRules.BulkImportMaxBytes, out var error);

        Assert.False(ok);
        Assert.NotNull(error);
        Assert.Contains(".xlsx", error);
    }

    [Fact]
    public void TryValidateStrictXlsx_RejectsEmptyFile()
    {
        var file = BuildFile("empty.xlsx", string.Empty);

        var ok = ExcelUploadRules.TryValidateStrictXlsx(file, ExcelUploadRules.BulkImportMaxBytes, out var error);

        Assert.False(ok);
        Assert.NotNull(error);
        Assert.Contains("לא הועלה", error);
    }

    [Fact]
    public void TryValidateStrictXlsx_RejectsLegacyXls()
    {
        var file = BuildFile("legacy.xls", "abc");

        var ok = ExcelUploadRules.TryValidateStrictXlsx(file, ExcelUploadRules.BulkImportMaxBytes, out var error);

        Assert.False(ok);
        Assert.Contains(".xlsm", error, StringComparison.Ordinal);
    }

    [Fact]
    public void TryValidateStrictXlsx_RejectsCsvExtension()
    {
        var file = BuildFile("data.csv", "a,b,c");

        var ok = ExcelUploadRules.TryValidateStrictXlsx(file, ExcelUploadRules.BulkImportMaxBytes, out var error);

        Assert.False(ok);
        Assert.Contains(".xlsx בלבד", error, StringComparison.Ordinal);
    }

    [Fact]
    public void TryValidateStrictXlsx_RejectsMissingExtension()
    {
        var file = BuildFile("noext", "abc");

        var ok = ExcelUploadRules.TryValidateStrictXlsx(file, ExcelUploadRules.BulkImportMaxBytes, out var error);

        Assert.False(ok);
        Assert.Contains("לא ניתן לזהות סוג הקובץ", error, StringComparison.Ordinal);
    }

    [Fact]
    public void TryValidateStrictXlsx_RejectsOversizedFile()
    {
        var bytes = new byte[64];
        var stream = new MemoryStream(bytes);
        var file = new FormFile(stream, 0, bytes.Length, "file", "big.xlsx");

        var ok = ExcelUploadRules.TryValidateStrictXlsx(file, 32, out var error);

        Assert.False(ok);
        Assert.Contains("הקובץ גדול מדי", error, StringComparison.Ordinal);
    }

    private static IFormFile BuildFile(string fileName, string content)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(content);
        var stream = new MemoryStream(bytes);
        return new FormFile(stream, 0, bytes.Length, "file", fileName);
    }
}
