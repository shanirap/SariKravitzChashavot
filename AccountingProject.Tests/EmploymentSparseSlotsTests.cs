using AccountingProject.Contracts;
using AccountingProject.Data;
using AccountingProject.Models;
using AccountingProject.Services;
using AccountingProject.Tests.TestHelpers;

namespace AccountingProject.Tests;

public sealed class EmploymentSparseSlotsTests
{
    [Fact]
    public async Task Create_PersistsOnlyFilledSlots_WithCorrectSlotIndex()
    {
        await using var db = DbTestFactory.CreateContext();
        var (employer, employee) = SeedEmployerAndEmployee(db);
        db.EmployerInstitutionSymbols.AddRange(
            new EmployerInstitutionSymbol { EmployerId = employer.Id, InstitutionSymbol = "111" },
            new EmployerInstitutionSymbol { EmployerId = employer.Id, InstitutionSymbol = "222" });
        db.SaveChanges();

        var slots = AllEmptySlots();
        slots[0] = new EmploymentDataSlotDto
        {
            GradeBand = 1,
            SlotIndex = 1,
            InstitutionSymbol = "111",
            WeeklyHours = 10m,
            JobBase = 100m,
        };
        slots[5] = new EmploymentDataSlotDto
        {
            GradeBand = 1,
            SlotIndex = 6,
            InstitutionSymbol = "222",
            WeeklyHours = 5m,
            JobBase = 50m,
        };

        var sut = ServiceTestFactory.CreateEmploymentDataService(db);
        var dto = new EmploymentDataDto
        {
            EmployeeId = employee.Id,
            EmployerId = employer.Id,
            AcademicYear = "תשפ\"ו",
            Grade1GradeName = "עוז לתמורה",
            Grade1Grade = "ב",
            Grade1Role = "מורה מקצועי",
            Grade1Seniority = "1",
            Slots = slots,
        };

        var (record, message) = await sut.CreateAsync(dto);

        Assert.Null(message);
        Assert.NotNull(record);
        Assert.Equal(2, record!.Slots.Count);
        Assert.Contains(record.Slots, s => s.GradeBand == 1 && s.SlotIndex == 1 && s.InstitutionSymbol == "111");
        Assert.Contains(record.Slots, s => s.GradeBand == 1 && s.SlotIndex == 6 && s.InstitutionSymbol == "222");

        var saved = await db.EmploymentData
            .Include(e => e.Slots)
            .FirstAsync(e => e.Id == record.Id);
        Assert.Equal(2, saved.Slots.Count);
    }

    [Fact]
    public async Task Update_RemovingSlotContent_DeletesRowFromDb()
    {
        await using var db = DbTestFactory.CreateContext();
        var (employer, employee) = SeedEmployerAndEmployee(db, "222333444");
        db.EmploymentData.Add(new EmploymentData
        {
            EmployeeId = employee.Id,
            EmployerId = employer.Id,
            AcademicYear = "תשפ\"ו",
            Grade1GradeName = "עוז לתמורה",
            Grade1Grade = "ב",
            Grade1Role = "מורה מחנך",
            Grade1Seniority = "1",
            Slots =
            [
                new EmploymentDataSlot
                {
                    GradeBand = 1,
                    SlotIndex = 1,
                    InstitutionSymbol = "111",
                    WeeklyHours = 10m,
                }
            ],
        });
        db.SaveChanges();
        var existing = db.EmploymentData.Include(e => e.Slots).First();

        var sut = ServiceTestFactory.CreateEmploymentDataService(db);
        var dto = new EmploymentDataDto
        {
            EmployeeId = employee.Id,
            EmployerId = employer.Id,
            AcademicYear = "תשפ\"ו",
            Grade1GradeName = "עוז לתמורה",
            Grade1Grade = "ב",
            Grade1Role = "מורה מחנך",
            Grade1Seniority = "1",
            Slots = AllEmptySlots(),
        };

        var (record, message) = await sut.UpdateAsync(existing.Id, dto);

        Assert.Null(message);
        Assert.NotNull(record);
        Assert.Empty(record!.Slots);

        var saved = await db.EmploymentDataSlots.Where(s => s.EmploymentDataId == existing.Id).ToListAsync();
        Assert.Empty(saved);
    }

    [Fact]
    public async Task Create_SupplementaryAndParent_BothPersisted()
    {
        await using var db = DbTestFactory.CreateContext();
        var (employer, employee) = SeedEmployerAndEmployee(db, "333444555");
        db.EmployerInstitutionSymbols.Add(new EmployerInstitutionSymbol
        {
            EmployerId = employer.Id,
            InstitutionSymbol = "G1",
            InstitutionSymbolName = "גן",
        });
        db.SaveChanges();

        var slots = AllEmptySlots();
        slots[0] = new EmploymentDataSlotDto
        {
            GradeBand = 1,
            SlotIndex = 1,
            InstitutionSymbol = "G1",
            WeeklyHours = 20m,
            JobBase = 80m,
        };
        slots[1] = new EmploymentDataSlotDto
        {
            GradeBand = 1,
            SlotIndex = 2,
            InstitutionSymbol = "G1",
            WeeklyHours = 3m,
            JobBase = 80m,
            SupplementaryParentSlotIndex = 1,
        };
        for (var i = 2; i < 12; i++)
            slots[i] = EmptySlotDto(i);

        var sut = ServiceTestFactory.CreateEmploymentDataService(db);
        var dto = new EmploymentDataDto
        {
            EmployeeId = employee.Id,
            EmployerId = employer.Id,
            AcademicYear = "תשפ\"ו",
            Grade1GradeName = "יסודי וגנים",
            Grade1Grade = "ב",
            Grade1Role = "גננת ראשית",
            Grade1Seniority = "1",
            Slots = slots,
        };

        var (record, message) = await sut.CreateAsync(dto);

        Assert.Null(message);
        Assert.Equal(2, record!.Slots.Count);
        Assert.Contains(record.Slots, s => s.SlotIndex == 1 && s.SupplementaryParentSlotIndex == null);
        Assert.Contains(record.Slots, s => s.SlotIndex == 2 && s.SupplementaryParentSlotIndex == 1);
    }

    private static List<EmploymentDataSlotDto> AllEmptySlots()
    {
        var list = new List<EmploymentDataSlotDto>();
        for (var b = 1; b <= 2; b++)
        for (var s = 1; s <= 6; s++)
            list.Add(EmptySlotDto((b - 1) * 6 + (s - 1), b, s));
        return list;
    }

    private static EmploymentDataSlotDto EmptySlotDto(int listIndex, int? band = null, int? slot = null)
    {
        var b = band ?? (listIndex / 6 + 1);
        var s = slot ?? (listIndex % 6 + 1);
        return new EmploymentDataSlotDto { GradeBand = b, SlotIndex = s };
    }

    private static (Employer Employer, Employee Employee) SeedEmployerAndEmployee(
        PayrollDbContext db, string idNumber = "123456789")
    {
        var employer = new Employer { Name = "Test Employer" };
        db.Employers.Add(employer);
        db.SaveChanges();
        var employee = new Employee
        {
            EmployerId = employer.Id,
            IdNumber = idNumber,
            FirstName = "Test",
            LastName = "User",
            Gender = "נקבה",
        };
        db.Employees.Add(employee);
        db.SaveChanges();
        return (employer, employee);
    }
}
