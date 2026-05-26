using AccountingProject.Domain;

namespace AccountingProject.Tests;

public sealed class DomainInstitutionTypesTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Resolve_Empty_DefaultsToOther(string? value)
    {
        var (type, error) = InstitutionTypes.Resolve(value);
        Assert.Equal(InstitutionTypes.Other, type);
        Assert.Null(error);
    }

    [Theory]
    [InlineData(InstitutionTypes.School)]
    [InlineData(InstitutionTypes.Kindergarten)]
    [InlineData(InstitutionTypes.Other)]
    public void Resolve_Valid_ReturnsTrimmed(string value)
    {
        var (type, error) = InstitutionTypes.Resolve($"  {value}  ");
        Assert.Equal(value, type);
        Assert.Null(error);
    }

    [Fact]
    public void Resolve_Invalid_ReturnsError()
    {
        var (type, error) = InstitutionTypes.Resolve("גן חובה");
        Assert.Equal(InstitutionTypes.Other, type);
        Assert.Contains("סוג מוסד", error);
    }

    [Fact]
    public void All_ContainsExactlyThreeAllowedValues()
    {
        Assert.Equal(3, InstitutionTypes.All.Count);
        Assert.Equal(3, InstitutionTypes.Allowed.Count);
    }
}
