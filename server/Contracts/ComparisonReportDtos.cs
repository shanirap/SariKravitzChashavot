namespace AccountingProject.Contracts
{
    public sealed record FieldMismatch(string FieldLabel, string? ExcelDisplay, string? DbDisplay);

    public sealed record UploadedComparisonRow(
        int SourceExcelRowNumber,
        string? IdNumber,
        int? EmployeeNumber,
        int GregorianMonth,
        int GregorianYear,
        string ResolvedAcademicYearNormalized,
        IReadOnlyDictionary<string, string?> CellsByNormalizedHeader);

    public sealed record MonthlyComparisonResult(
        string MonthColumnLabel,
        bool Matches,
        IReadOnlyList<FieldMismatch> Mismatches);

    public sealed record ComparisonReportRow(
        string? InstitutionSymbol,
        string EmployeeDisplayName,
        string? RoleDisplay,
        string? HoursSummaryDisplay,
        string? EducationDisplay,
        string? HoursValidityNote,
        IReadOnlyDictionary<string, string> MonthMarksByColumnLabel,
        IReadOnlySet<string> MonthMismatchHighlightLabels,
        string NotesCombined);
}
