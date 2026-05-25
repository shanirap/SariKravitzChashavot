using AccountingProject.Contracts;
using AccountingProject.Services;
using AccountingProject.Tests.TestHelpers;
using Microsoft.EntityFrameworkCore;

namespace AccountingProject.Tests;

/// <summary>
/// CreateAsync restores a soft-deleted employer when ח.פ. matches; never restores without BusinessNumber.
/// </summary>
public sealed class EmployerRestoreTests
{
    [Fact]
    public async Task CreateAsync_WithDeletedSameBusinessNumber_RestoresSameRowAndUpdatesFields()
    {
        await using var db = DbTestFactory.CreateContext();
        var sut = new EmployerService(db);

        var created = await sut.CreateAsync(new EmployerDto
        {
            Name = "Old Name",
            BusinessNumber = "514123456",
            BeneficiarySymbol = "OLD-BEN",
            EketzNumber = "OLD-EK",
        });
        var id = created.Id;

        var del = await sut.DeleteAsync(id);
        Assert.True(del.Success);

        var pagedAfterDelete = await sut.GetPagedAsync(null, 1, 50);
        Assert.DoesNotContain(pagedAfterDelete.Items, e => e.Id == id);

        var restored = await sut.CreateAsync(new EmployerDto
        {
            Name = "New Name",
            BusinessNumber = "514123456",
            BeneficiarySymbol = "NEW-BEN",
            EketzNumber = "NEW-EK",
        });

        Assert.Equal(id, restored.Id);
        Assert.Equal("New Name", restored.Name);
        Assert.Equal("514123456", restored.BusinessNumber);
        Assert.Equal("NEW-BEN", restored.BeneficiarySymbol);
        Assert.Equal("NEW-EK", restored.EketzNumber);
        Assert.False(restored.IsDeleted);
        Assert.Null(restored.DeletedAtUtc);

        var all = await db.Employers.IgnoreQueryFilters().Where(e => e.BusinessNumber == "514123456").ToListAsync();
        Assert.Single(all);

        var paged = await sut.GetPagedAsync(null, 1, 50);
        var back = Assert.Single(paged.Items, e => e.Id == id);
        Assert.Equal("New Name", back.Name);
        Assert.False(back.IsDeleted);
    }

    [Fact]
    public async Task CreateAsync_DuplicateActiveBusinessNumber_StillThrows()
    {
        await using var db = DbTestFactory.CreateContext();
        var sut = new EmployerService(db);

        await sut.CreateAsync(new EmployerDto
        {
            Name = "A",
            BusinessNumber = "999888777",
        });

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.CreateAsync(new EmployerDto
            {
                Name = "B",
                BusinessNumber = "999888777",
            }));

        Assert.Contains("999888777", ex.Message);
        Assert.Contains("כבר קיים", ex.Message);
    }

    [Fact]
    public async Task CreateAsync_WithoutBusinessNumber_DeletedThenNew_DoesNotRestore_CreatesSecondRow()
    {
        await using var db = DbTestFactory.CreateContext();
        var sut = new EmployerService(db);

        var first = await sut.CreateAsync(new EmployerDto
        {
            Name = "Same Name No BN",
            BusinessNumber = null,
        });
        var firstId = first.Id;

        Assert.True((await sut.DeleteAsync(firstId)).Success);

        var second = await sut.CreateAsync(new EmployerDto
        {
            Name = "Different Name Still No BN",
            BusinessNumber = "",
        });

        Assert.NotEqual(firstId, second.Id);

        var totalIncludingDeleted = await db.Employers.IgnoreQueryFilters().CountAsync();
        Assert.Equal(2, totalIncludingDeleted);

        var paged = await sut.GetPagedAsync(null, 1, 50);
        Assert.Single(paged.Items);
        Assert.Equal(second.Id, paged.Items[0].Id);
    }
}
