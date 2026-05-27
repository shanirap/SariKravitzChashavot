using AccountingProject.Models;
using AccountingProject.Services;
using AccountingProject.Tests.TestHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace AccountingProject.Tests;

public sealed class BulkImportEmployersTests
{
    private static BulkImportService CreateService(PayrollDbContext db) =>
        new(db, NullLogger<BulkImportService>.Instance);

    [Fact]
    public async Task ImportEmployers_NewRow_ImportsSuccessfully()
    {
        await using var db = DbTestFactory.CreateContext();
        await using var stream = BulkImportEmployerWorkbook.Create(("512345678", "Employer A"));
        var file = FormFileFromStream(stream, "employers.xlsx");

        var result = await CreateService(db).ImportEmployersAsync(file);

        Assert.Equal(1, result.Imported);
        var employer = await db.Employers.SingleAsync(e => e.BusinessNumber == "512345678");
        Assert.Equal("Employer A", employer.Name);
    }

    [Fact]
    public async Task ImportEmployers_DuplicateBusinessNumberInFile_SkipsSecondRow()
    {
        await using var db = DbTestFactory.CreateContext();
        await using var stream = BulkImportEmployerWorkbook.Create(
            ("611111111", "First"),
            ("611111111", "Second"));
        var file = FormFileFromStream(stream, "dup.xlsx");

        var result = await CreateService(db).ImportEmployersAsync(file);

        Assert.Equal(1, result.Imported);
        Assert.Contains(result.Rows, r => !r.Success && r.Message.Contains("כבר מופיע בקובץ"));
    }

    [Fact]
    public async Task ImportEmployers_ExistingBusinessNumber_SkipsRow()
    {
        await using var db = DbTestFactory.CreateContext();
        db.Employers.Add(new Employer { Name = "Existing", BusinessNumber = "722222222" });
        await db.SaveChangesAsync();
        await using var stream = BulkImportEmployerWorkbook.Create(("722222222", "Duplicate"));
        var file = FormFileFromStream(stream, "exists.xlsx");

        var result = await CreateService(db).ImportEmployersAsync(file);

        Assert.Equal(0, result.Imported);
        Assert.Contains(result.Rows, r => !r.Success && r.Message.Contains("כבר קיים"));
    }

    [Fact]
    public async Task ImportEmployers_EmptyBusinessNumber_SkipsRow()
    {
        await using var db = DbTestFactory.CreateContext();
        await using var stream = BulkImportEmployerWorkbook.Create(("", "No BN"));
        var file = FormFileFromStream(stream, "empty.xlsx");

        var result = await CreateService(db).ImportEmployersAsync(file);

        Assert.Equal(0, result.Imported);
        Assert.Contains(result.Rows, r => !r.Success && r.Message.Contains("ח.פ. ריק"));
    }

    [Fact]
    public async Task ImportEmployers_SoftDeletedEmployer_RestoresWithMessage()
    {
        await using var db = DbTestFactory.CreateContext();
        var deleted = new Employer
        {
            Name = "Old Name",
            BusinessNumber = "833333333",
            IsDeleted = true,
            DeletedAtUtc = DateTime.UtcNow,
        };
        db.Employers.Add(deleted);
        await db.SaveChangesAsync();

        await using var stream = BulkImportEmployerWorkbook.Create(("833333333", "Restored Name"));
        var file = FormFileFromStream(stream, "restore.xlsx");

        var result = await CreateService(db).ImportEmployersAsync(file);

        Assert.Equal(1, result.Imported);
        await db.Entry(deleted).ReloadAsync();
        Assert.False(deleted.IsDeleted);
        Assert.Equal("Restored Name", deleted.Name);
        Assert.Contains(result.Rows, r => r.Message.Contains("שוחזר בהצלחה"));
    }

    private static IFormFile FormFileFromStream(Stream stream, string fileName)
    {
        if (stream.CanSeek)
            stream.Position = 0;
        return new FormFile(stream, 0, stream.Length, "file", fileName);
    }
}
