using AccountingProject.Domain;

namespace AccountingProject.Tests;

public sealed class EmploymentJobBaseAdjustmentsTests
{
    [Fact]
    public void NetJobBaseAfterAgeHours_SubtractsAgeHours()
    {
        var net = EmploymentJobBaseAdjustments.NetJobBaseAfterAgeHours(100m, 12m);
        Assert.Equal(88m, net);
    }

    [Fact]
    public void NetJobBaseAfterAgeHours_NullGross_ReturnsNull()
    {
        Assert.Null(EmploymentJobBaseAdjustments.NetJobBaseAfterAgeHours(null, 5m));
    }

    [Fact]
    public void NetJobBaseAfterAgeHours_DoesNotGoBelowZero()
    {
        Assert.Equal(0m, EmploymentJobBaseAdjustments.NetJobBaseAfterAgeHours(5m, 20m));
    }
}
