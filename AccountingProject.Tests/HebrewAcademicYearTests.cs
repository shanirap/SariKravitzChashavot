using AccountingProject.Domain;

namespace AccountingProject.Tests;

public sealed class HebrewAcademicYearTests
{
    [Fact]
    public void Format_KnownYear_ProducesExpectedLabel()
    {
        Assert.Equal("תשפ\"ו", HebrewAcademicYear.Format(5786));
    }

    [Theory]
    [InlineData("5786", "תשפ\"ו")]
    public void Normalize_HebrewYearNumber_ReturnsFormatted(string input, string expected)
    {
        Assert.Equal(expected, HebrewAcademicYear.Normalize(input));
    }

    [Fact]
    public void Normalize_YearWithZeroRemainder_ReturnsEmptyString()
    {
        Assert.Equal(string.Empty, HebrewAcademicYear.Normalize("5000"));
    }

    [Fact]
    public void Normalize_Gregorian2000Range_Adds3760()
    {
        var result = HebrewAcademicYear.Normalize("2025");
        Assert.NotNull(result);
        Assert.Equal(HebrewAcademicYear.Format(2025 + 3760), result);
    }

    [Fact]
    public void Normalize_Invalid_ReturnsTrimmedOrNull()
    {
        Assert.Null(HebrewAcademicYear.Normalize(null));
        Assert.Equal("תשפ\"ו", HebrewAcademicYear.Normalize("  תשפ\"ו  "));
    }
}
