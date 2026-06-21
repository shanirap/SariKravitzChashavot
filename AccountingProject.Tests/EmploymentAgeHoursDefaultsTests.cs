using AccountingProject.Domain;

namespace AccountingProject.Tests;

public sealed class EmploymentAgeHoursDefaultsTests
{
    private static readonly DateOnly RefDate = new(2025, 9, 1);

    [Theory]
    [InlineData(1976, 9, 2, 0)]   // age 49
    [InlineData(1973, 1, 15, 2)]  // age 52
    [InlineData(1968, 8, 31, 4)]  // age 57
    public void Compute_ReturnsExpectedHoursByAge(int year, int month, int day, decimal expected)
    {
        var result = EmploymentAgeHoursDefaults.Compute(new DateOnly(year, month, day), RefDate);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Compute_NullBirthDate_ReturnsNull()
    {
        Assert.Null(EmploymentAgeHoursDefaults.Compute(null, RefDate));
    }
}
