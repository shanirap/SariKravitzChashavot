using AccountingProject.Infrastructure;

namespace AccountingProject.Tests;

public sealed class PasswordRulesTests
{
    [Fact]
    public void ValidationError_ValidPassword_ReturnsNull()
    {
        Assert.Null(PasswordRules.ValidationError("Aa1!aaaaaaaaaa"));
    }

    [Fact]
    public void ValidationError_Null_ReturnsRequired()
    {
        Assert.Equal("Password is required.", PasswordRules.ValidationError(null));
    }

    [Fact]
    public void ValidationError_EmptyOrWhitespace_ReturnsRequired()
    {
        Assert.Equal("Password is required.", PasswordRules.ValidationError(""));
        Assert.Equal("Password is required.", PasswordRules.ValidationError("   "));
    }

    [Fact]
    public void ValidationError_LeadingOrTrailingWhitespace_ReturnsWhitespaceError()
    {
        Assert.Equal(
            "Password must not contain leading or trailing whitespace.",
            PasswordRules.ValidationError(" Aa1!aaaaaaaaaa"));

        Assert.Equal(
            "Password must not contain leading or trailing whitespace.",
            PasswordRules.ValidationError("Aa1!aaaaaaaaaa "));
    }

    [Fact]
    public void ValidationError_ShortPassword_ReturnsLengthMessage()
    {
        var err = PasswordRules.ValidationError("Aa1!aaaaaaa");
        Assert.NotNull(err);
        Assert.Contains($"{PasswordRules.MinimumLength}", err);
    }

    [Theory]
    [InlineData("aaaaaaaaaaaa")] // no upper, digit, special
    [InlineData("AAAAAAAAAAAA")] // no lower, digit, special
    [InlineData("Aa!aaaaaaaaaa")] // no digit
    [InlineData("Aa1aaaaaaaaaa")] // no special (letters+digits only)
    [InlineData("Aa1 aaaaaaaaaa")] // space counts as neither symbol nor allowed combo per rules
    public void ValidationError_MissingCharacterClass_ReturnsComplexityMessage(string password)
    {
        var err = PasswordRules.ValidationError(password);
        Assert.NotNull(err);
        Assert.Contains("uppercase", err, StringComparison.OrdinalIgnoreCase);
    }
}
