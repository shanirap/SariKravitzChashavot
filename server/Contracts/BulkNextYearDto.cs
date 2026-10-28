namespace AccountingProject.Contracts
{
    /// <summary>בקשה להוסיף שנת לימודים לכלל העובדים של מעסיק.</summary>
    public class BulkNextYearRequestDto
    {
        /// <summary>שנת לימודים יעד (חדשה) — למשל תשפ"ז</summary>
        public string TargetAcademicYear { get; set; } = string.Empty;

        /// <summary>שנת לימודים מקור (אופציונלי) — ברירת המחדל: השנה האחרונה לכל עובד</summary>
        public string? SourceAcademicYear { get; set; }
    }

    public class BulkNextYearResultDto
    {
        public int Created { get; set; }
        public int Skipped { get; set; }
        public int Errors { get; set; }
        public List<BulkNextYearEmployeeResult> Details { get; set; } = [];
    }

    public class BulkNextYearEmployeeResult
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        /// <summary>created | skipped | error</summary>
        public string Status { get; set; } = string.Empty;
        public string? Message { get; set; }
    }
}
