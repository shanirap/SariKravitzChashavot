namespace AccountingProject.Contracts
{
    public class PayrollMonthStatusDto
    {
        public int Month { get; set; }
        public int GregorianYear { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int? BatchId { get; set; }
        public int RowsCount { get; set; }
        public DateTime? UploadedAtUtc { get; set; }
        public string? OriginalFileName { get; set; }
    }
}
