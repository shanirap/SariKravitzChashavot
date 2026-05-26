namespace AccountingProject.Contracts
{
    public class PayrollMonthlyInputRowEditDto
    {
        public string? InstitutionSymbol { get; set; }
        public string? OketzEmployeeNumber { get; set; }
        public string? IdNumber { get; set; }
        public string? FullName { get; set; }
        public string? Role { get; set; }
        public string? Grade { get; set; }
        public decimal? Seniority { get; set; }
        public decimal? WeeklyHours { get; set; }
        public decimal? JobBase { get; set; }
        public decimal? JobPercent { get; set; }
        public decimal? AgeHours { get; set; }
        public decimal? TrainingBenefits { get; set; }
        public decimal? DoubleDegree { get; set; }
        public decimal? TrainingFund { get; set; }
        public decimal? GeneralMultiplier { get; set; }
        public string? ManualEditNote { get; set; }
    }
}
