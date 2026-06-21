namespace AccountingProject.Contracts
{
    public class EmploymentDataDto
    {
        public int EmployeeId { get; set; }
        public int EmployerId { get; set; }
        public string? AcademicYear { get; set; }

        public decimal? Grade1Total { get; set; }
        public decimal? Grade1JobPercent { get; set; }
        public decimal? Grade1TrainingFundPercent { get; set; }
        public decimal? Grade1AgeHours { get; set; }
        public decimal? Grade1MotherBenefitPercent { get; set; }
        public decimal? Grade1TrainingBenefits { get; set; }
        public decimal? Grade1DoubleDegree { get; set; }

        public decimal? Grade2Total { get; set; }
        public decimal? Grade2JobPercent { get; set; }
        public decimal? Grade2TrainingFundPercent { get; set; }
        public decimal? Grade2AgeHours { get; set; }
        public decimal? Grade2MotherBenefitPercent { get; set; }
        public decimal? Grade2TrainingBenefits { get; set; }
        public decimal? Grade2DoubleDegree { get; set; }

        public string? Grade1GradeName { get; set; }
        public string? Grade1Grade { get; set; }
        public string? Grade1Role { get; set; }
        public string? Grade1Seniority { get; set; }

        public string? Grade2GradeName { get; set; }
        public string? Grade2Grade { get; set; }
        public string? Grade2Role { get; set; }
        public string? Grade2Seniority { get; set; }

        public List<EmploymentDataSlotDto>? Slots { get; set; }
    }

    public class EmploymentDataSlotDto
    {
        public int GradeBand { get; set; }
        public int SlotIndex { get; set; }
        public string? InstitutionSymbol { get; set; }
        public decimal? WeeklyHours { get; set; }

        /// <summary>בסיס משרה נטו במקטע (גולמי לפי דירוג/תפקיד פחות שעות גיל); השרת שומר כפי שנשלח מהלקוח.</summary>
        public decimal? JobBase { get; set; }

        /// <summary>1–5: שורה זו היא שעות נוספות (3) למחנך/גננת מול מקטע זה באותה דרגה.</summary>
        public int? SupplementaryParentSlotIndex { get; set; }
    }
}
