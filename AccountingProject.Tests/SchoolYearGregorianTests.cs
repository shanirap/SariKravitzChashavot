using AccountingProject.Domain;

namespace AccountingProject.Tests;

public sealed class SchoolYearGregorianTests
{
    [Fact]
    public void GetSchoolYearFromGregorianMonth_September_UsesHigherHebrewYear()
    {
        var sep = SchoolYearGregorian.GetSchoolYearFromGregorianMonth(9, 2025);
        var jan = SchoolYearGregorian.GetSchoolYearFromGregorianMonth(1, 2026);
        Assert.Equal(sep, jan);
    }

    [Fact]
    public void GetSchoolYearMonthSequence_ReturnsTwelveMonthsSepToAug()
    {
        var seq = SchoolYearGregorian.GetSchoolYearMonthSequence(2025);
        Assert.Equal(12, seq.Count);
        Assert.Equal((9, 2025), seq[0]);
        Assert.Equal((8, 2026), seq[11]);
    }

    [Fact]
    public void GetSeptemberGregorianYear_January_IsPreviousCalendarYear()
    {
        Assert.Equal(2024, SchoolYearGregorian.GetSeptemberGregorianYearForSchoolYearContaining(1, 2025));
        Assert.Equal(2025, SchoolYearGregorian.GetSeptemberGregorianYearForSchoolYearContaining(9, 2025));
    }
}
