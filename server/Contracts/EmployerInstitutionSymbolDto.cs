namespace AccountingProject.Contracts
{
    public class EmployerInstitutionSymbolDto
    {
        public string InstitutionSymbol { get; set; } = string.Empty;
        public string? InstitutionSymbolName { get; set; }
        public string? InstitutionType { get; set; }
    }

    public class EmployerInstitutionSymbolUpdateDto
    {
        public string? InstitutionSymbolName { get; set; }
        public string? InstitutionType { get; set; }
    }
}
