using AccountingProject.Domain;

namespace AccountingProject.Tests;

public sealed class TeacherSupplementarySlotRulesTests
{
    [Theory]
    [InlineData("יסודי וגנים", "גננת ראשית", true)]
    [InlineData("עוז לתמורה", "מורה מחנך", true)]
    [InlineData("יסודי וגנים", "גננת", false)]
    [InlineData("עוז לתמורה", "מורה", false)]
    public void Qualifies_MatchesExpectedPairs(string gradeName, string role, bool expected)
    {
        Assert.Equal(expected, TeacherSupplementarySlotRules.Qualifies(gradeName, role));
    }
}
