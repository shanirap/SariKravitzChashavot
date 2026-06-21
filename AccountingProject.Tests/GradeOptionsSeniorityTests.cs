using AccountingProject.Domain;

namespace AccountingProject.Tests;

public sealed class GradeOptionsSeniorityTests
{
    [Theory]
    [InlineData(null, true)]
    [InlineData("", true)]
    [InlineData("   ", true)]
    [InlineData("0", true)]
    [InlineData("5", true)]
    [InlineData("5.5", true)]
    [InlineData("12.25", true)]
    [InlineData("-1", false)]
    [InlineData("abc", false)]
    public void IsValidSeniority_accepts_non_negative_decimals(string? input, bool expected)
    {
        Assert.Equal(expected, GradeOptions.IsValidSeniority(input));
    }

    [Fact]
    public void GetGradeBandValidationError_rejects_invalid_seniority_with_decimal_friendly_message()
    {
        var err = GradeOptions.GetGradeBandValidationError(
            1,
            GradeOptions.UnifiedEducationSupportGradeName,
            "תומכת חינוך",
            "סייעת ראשית",
            "x");
        Assert.Contains("מספר", err, StringComparison.Ordinal);
        Assert.DoesNotContain("שלם", err!, StringComparison.Ordinal);
    }

    [Fact]
    public void GetGradeBandValidationError_accepts_decimal_seniority_for_valid_band()
    {
        var err = GradeOptions.GetGradeBandValidationError(
            1,
            GradeOptions.UnifiedEducationSupportGradeName,
            "תומכת חינוך",
            "סייעת ראשית",
            "5.5");
        Assert.Null(err);
    }

    [Fact]
    public void GetGradeBandValidationError_accepts_legacy_ahid_alias()
    {
        var err = GradeOptions.GetGradeBandValidationError(1, "אחיד", "תומכת חינוך", "סייעת ראשית", "5.5");

        Assert.Null(err);
    }

    [Fact]
    public void GradeNames_exposes_new_ahid_label_only()
    {
        Assert.Contains(GradeOptions.UnifiedEducationSupportGradeName, GradeOptions.GradeNames);
        Assert.DoesNotContain(GradeOptions.LegacyUnifiedGradeName, GradeOptions.GradeNames);
    }

    [Theory]
    [InlineData("יסודי וגנים", "גננת ראשית")]
    [InlineData("עוז לתמורה", "מורה מחנך")]
    public void IsValidGrade_BaDegree_acceptsForCoreGradeNames(string gradeName, string role)
    {
        Assert.True(GradeOptions.IsValidGrade(gradeName, "ב.א."));
        Assert.Null(GradeOptions.GetGradeBandValidationError(1, gradeName, "ב.א.", role, "1"));
    }
}
