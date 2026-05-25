using AccountingProject.Contracts;
using AccountingProject.Domain;
using AccountingProject.Models;

namespace AccountingProject.Tests;

public sealed class EmploymentSlotPersistenceTests
{
    [Fact]
    public void ShouldPersistSlot_Dto_WithSymbolOrHoursOrSupplementary()
    {
        Assert.True(EmploymentSlotPersistence.ShouldPersistSlot(new EmploymentDataSlotDto
        {
            GradeBand = 1, SlotIndex = 1, InstitutionSymbol = "ABC",
        }));
        Assert.True(EmploymentSlotPersistence.ShouldPersistSlot(new EmploymentDataSlotDto
        {
            GradeBand = 1, SlotIndex = 1, WeeklyHours = 5.5m,
        }));
        Assert.True(EmploymentSlotPersistence.ShouldPersistSlot(new EmploymentDataSlotDto
        {
            GradeBand = 1, SlotIndex = 2, SupplementaryParentSlotIndex = 1,
        }));
        Assert.False(EmploymentSlotPersistence.ShouldPersistSlot(new EmploymentDataSlotDto
        {
            GradeBand = 1, SlotIndex = 3,
        }));
        Assert.False(EmploymentSlotPersistence.ShouldPersistSlot(new EmploymentDataSlotDto
        {
            GradeBand = 1, SlotIndex = 4, InstitutionSymbol = "  ", WeeklyHours = 0m,
        }));
    }

    [Fact]
    public void ShouldPersistSlot_Entity_SupplementaryRowPersisted()
    {
        var slot = new EmploymentDataSlot
        {
            GradeBand = 1,
            SlotIndex = 2,
            SupplementaryParentSlotIndex = 1,
        };
        Assert.True(EmploymentSlotPersistence.ShouldPersistSlot(slot));
    }
}
