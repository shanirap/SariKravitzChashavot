namespace AccountingProject.Contracts
{
    public class EmployerDto
    {
        public string Name { get; set; } = string.Empty;
        public string? BusinessNumber { get; set; }
        public string? BeneficiarySymbol { get; set; }
        public string? EketzNumber { get; set; }
    }
}
