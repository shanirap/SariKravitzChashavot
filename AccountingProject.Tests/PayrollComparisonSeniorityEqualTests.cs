using AccountingProject.Services;

namespace AccountingProject.Tests;

public sealed class PayrollComparisonSeniorityEqualTests
{
    [Theory]
    [InlineData("5", "5", true)]
    [InlineData("5.5", "5.5", true)]
    [InlineData("5.5", "5.50", true)]
    [InlineData("5", "6", false)]
    [InlineData("", null, true)]
    public void SeniorityEqual_compares_numeric_values(string? a, string? b, bool expected)
    {
        Assert.Equal(expected, PayrollComparisonUploadSupport.SeniorityEqual(a, b));
    }
}
