namespace AccountingProject.Contracts;

public sealed class AnnualComparisonFieldDto
{
    public string? Computed { get; set; }
    public string? Display { get; set; }
    public bool IsOverridden { get; set; }
}

public sealed class AnnualComparisonPreviewRowDto
{
    public int SlotId { get; set; }
    public int GradeBand { get; set; }
    public AnnualComparisonFieldDto InstitutionSymbol { get; set; } = new();
    public AnnualComparisonFieldDto FullName { get; set; } = new();
    public AnnualComparisonFieldDto Role { get; set; } = new();
    public AnnualComparisonFieldDto SugMisraFromPayroll { get; set; } = new();
    public AnnualComparisonFieldDto Grade { get; set; } = new();
    public AnnualComparisonFieldDto Seniority { get; set; } = new();
    public AnnualComparisonFieldDto WeeklyHours { get; set; } = new();
    public AnnualComparisonFieldDto JobBase { get; set; } = new();
    public AnnualComparisonFieldDto JobPercent { get; set; } = new();
    public AnnualComparisonFieldDto DoubleGeneral { get; set; } = new();
    public Dictionary<string, AnnualComparisonFieldDto> MonthCells { get; set; } = new();
    public bool IsManualEdited { get; set; }
    public string? ManualEditNote { get; set; }
}

public sealed class AnnualComparisonPreviewDto
{
    public string AcademicYear { get; set; } = string.Empty;
    public List<string> MonthHeaders { get; set; } = [];
    public List<AnnualComparisonPreviewRowDto> Rows { get; set; } = [];
}

public sealed class AnnualComparisonOverrideSaveRequest
{
    public int EmployerId { get; set; }
    public string AcademicYear { get; set; } = string.Empty;
    public List<AnnualComparisonOverrideRowSaveDto> Rows { get; set; } = [];
}

public sealed class AnnualComparisonOverrideRowSaveDto
{
    public int SlotId { get; set; }
    public string? InstitutionSymbol { get; set; }
    public string? FullName { get; set; }
    public string? Role { get; set; }
    public string? SugMisraFromPayroll { get; set; }
    public string? Grade { get; set; }
    public string? Seniority { get; set; }
    public decimal? WeeklyHours { get; set; }
    public decimal? JobBase { get; set; }
    public decimal? JobPercent { get; set; }
    public decimal? DoubleGeneral { get; set; }
    public Dictionary<string, string>? MonthCells { get; set; }
    public string? ManualEditNote { get; set; }
}

public sealed class AnnualComparisonClearOverridesRequest
{
    public int EmployerId { get; set; }
    public string AcademicYear { get; set; } = string.Empty;
    public int? SlotId { get; set; }
}
