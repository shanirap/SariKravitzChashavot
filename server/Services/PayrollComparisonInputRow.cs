namespace AccountingProject.Services;

/// <summary>Parsed Okets payroll row values decoupled from report generation.</summary>
internal sealed class PayrollComparisonInputRow
{
    public int Month { get; init; }
    public int GregorianYear { get; init; }
    public string? InstitutionSymbol { get; init; }
    public string? OketzEmployeeNumber { get; init; }
    public string? IdNumber { get; init; }
    public string? FullName { get; init; }
    public string? Role { get; init; }
    public string? Grade { get; init; }
    public decimal? Seniority { get; init; }
    public decimal? WeeklyHours { get; init; }
    public decimal? JobBase { get; init; }
    public decimal? JobPercent { get; init; }
    public decimal? AgeHours { get; init; }
    public decimal? TrainingBenefits { get; init; }
    public decimal? DoubleDegree { get; init; }
    public decimal? TrainingFund { get; init; }
    public decimal? GeneralMultiplier { get; init; }
    public int SourceExcelRowNumber { get; init; }
    public string? RawCellsJson { get; init; }

    /// <summary>1 = first grade band column group, 2 = second (duplicate headers in Excel).</summary>
    public byte GradeBand { get; init; }
}
