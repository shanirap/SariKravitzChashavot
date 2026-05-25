namespace AccountingProject.Contracts
{
    public class ImportRowResult
    {
        public int Row { get; set; }
        public string? IdNumber { get; set; }
        public string? EmployerName { get; set; }
        public string? BusinessNumber { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
