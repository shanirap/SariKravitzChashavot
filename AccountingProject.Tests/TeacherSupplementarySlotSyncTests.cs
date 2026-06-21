using AccountingProject.Contracts;
using AccountingProject.Domain;

namespace AccountingProject.Tests;

public sealed class TeacherSupplementarySlotSyncTests
{
    [Fact]
    public void Sync_YisodiGanenetRashit_CreatesThreeHourChildSlot()
    {
        var dto = CreateDto(
            "יסודי וגנים",
            "גננת ראשית",
            parentSymbol: "471136",
            parentHours: 20.50m,
            parentJobBase: 30m);

        TeacherSupplementarySlotSync.Sync(dto);

        var child = dto.Slots!.Single(s => s.GradeBand == 1 && s.SlotIndex == 2);
        Assert.Equal(1, child.SupplementaryParentSlotIndex);
        Assert.Equal("471136", child.InstitutionSymbol);
        Assert.Equal(TeacherSupplementarySlotSync.TeacherExtraHoursWeekly, child.WeeklyHours);
        Assert.Equal(30m, child.JobBase);
    }

    [Fact]
    public void Sync_NonQualifyingRole_ClearsSupplementarySlot()
    {
        var dto = CreateDto(
            "יסודי וגנים",
            "מורה מקצועי",
            parentSymbol: "471136",
            parentHours: 20.50m,
            parentJobBase: 30m);
        dto.Slots!.Add(new EmploymentDataSlotDto
        {
            GradeBand = 1,
            SlotIndex = 2,
            InstitutionSymbol = "471136",
            WeeklyHours = 3m,
            JobBase = 30m,
            SupplementaryParentSlotIndex = 1,
        });

        TeacherSupplementarySlotSync.Sync(dto);

        var child = dto.Slots.Single(s => s.GradeBand == 1 && s.SlotIndex == 2);
        Assert.Null(child.SupplementaryParentSlotIndex);
        Assert.Null(child.WeeklyHours);
    }

    [Fact]
    public void Sync_OzLeTmuraMorehMehanek_CreatesSupplementarySlot()
    {
        var dto = CreateDto(
            "עוז לתמורה",
            "מורה מחנך",
            parentSymbol: "SYM-1",
            parentHours: 18m,
            parentJobBase: 38m);

        TeacherSupplementarySlotSync.Sync(dto);

        var child = dto.Slots!.Single(s => s.GradeBand == 1 && s.SlotIndex == 2);
        Assert.Equal(1, child.SupplementaryParentSlotIndex);
        Assert.Equal(3m, child.WeeklyHours);
    }

    private static EmploymentDataDto CreateDto(
        string gradeName,
        string role,
        string parentSymbol,
        decimal parentHours,
        decimal parentJobBase)
    {
        return new EmploymentDataDto
        {
            Grade1GradeName = gradeName,
            Grade1Role = role,
            Slots =
            [
                new EmploymentDataSlotDto
                {
                    GradeBand = 1,
                    SlotIndex = 1,
                    InstitutionSymbol = parentSymbol,
                    WeeklyHours = parentHours,
                    JobBase = parentJobBase,
                },
            ],
        };
    }
}
