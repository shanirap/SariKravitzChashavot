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

    private static IFormFile BuildFile(string fileName, string content)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(content);
        var stream = new MemoryStream(bytes);
        return new FormFile(stream, 0, bytes.Length, "file", fileName);
    }
}
