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

    [Fact]
    public void TryParseSeptemberGregorianYear_TashpaV_Returns2025()
    {
        Assert.True(HebrewAcademicYear.TryParseSeptemberGregorianYear("תשפ\"ו", out var year));
        Assert.Equal(2025, year);
    }

    [Fact]
    public void TryParseSeptemberGregorianYear_Invalid_ReturnsFalse()
    {
        Assert.False(HebrewAcademicYear.TryParseSeptemberGregorianYear("xyz123", out _));
        Assert.False(HebrewAcademicYear.TryParseSeptemberGregorianYear(null, out _));
        Assert.False(HebrewAcademicYear.TryParseSeptemberGregorianYear("", out _));
    }

    [Fact]
    public void TryValidateAndCanonicalize_ValidNumericYears_ReturnCanonicalHebrewYear()
    {
        Assert.True(HebrewAcademicYear.TryValidateAndCanonicalize("5786", out var fromHebrewNumber));
        Assert.Equal("תשפ\"ו", fromHebrewNumber);

        Assert.True(HebrewAcademicYear.TryValidateAndCanonicalize("2026", out var fromGregorian));
        Assert.Equal("תשפ\"ו", fromGregorian);
    }

    [Fact]
    public void TryValidateAndCanonicalize_InvalidInput_ReturnsFalse()
    {
        Assert.False(HebrewAcademicYear.TryValidateAndCanonicalize("xyz123", out _));
        Assert.False(HebrewAcademicYear.TryValidateAndCanonicalize("5000", out _));
    }

    [Fact]
    public void GetSchoolYearStartDate_InvalidYear_Throws()
    {
        Assert.Throws<ArgumentException>(() => HebrewAcademicYear.GetSchoolYearStartDate("xyz123"));
    }
}
