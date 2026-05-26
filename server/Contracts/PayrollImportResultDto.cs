namespace AccountingProject.Contracts
{
    public class PayrollImportResultDto
    {
        public int BatchId { get; set; }
        public int EmployerId { get; set; }
        public string AcademicYear { get; set; } = string.Empty;
        public int Month { get; set; }
        public int GregorianYear { get; set; }
        public int RowsCount { get; set; }
        public string OriginalFileName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
