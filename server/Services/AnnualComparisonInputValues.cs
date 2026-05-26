namespace AccountingProject.Services;

/// <summary>Normalized Okets input fields used by annual comparison month cells.</summary>
internal readonly record struct AnnualComparisonInputValues(
    string? Role,
    string? Grade,
    string? Seniority,
    decimal? JobBase,
    bool CompareJobBase,
    decimal? JobPercent,
    decimal? WeeklyHours,
    decimal? GeneralMultiplier);
