using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccountingProject.Models
{
    [Table("נתוני_העסקה")]
    public class EmploymentData : IAuditableEntity, ISoftDeletable
    {
        [Key][Column("מזהה_נתון_העסקה")] public int Id { get; set; }
        [Column("מזהה_עובד")]             public int EmployeeId { get; set; }
        [Column("מזהה_מעסיק")]            public int EmployerId { get; set; }

        /// <summary>שנת לימודים עברית (למשל תשפ"ו)</summary>
        [MaxLength(20)]
        [Column("שנת_לימודים")]
        public string AcademicYear { get; set; } = string.Empty;

        // סיכומים — דרגה 1
        [Column("דרגה1_סהכ")]
        public decimal? Grade1Total { get; set; }
        [Column("דרגה1_אחוז_משרה")]
        public decimal? Grade1JobPercent { get; set; }
        [Column("דרגה1_קרן_השתלמות_אחוז")]
        public decimal? Grade1TrainingFundPercent { get; set; }
        [Column("דרגה1_שעות_גיל")]
        public decimal? Grade1AgeHours { get; set; }
        [Column("דרגה1_אחוז_תוספת_אם")]
        public decimal? Grade1MotherBenefitPercent { get; set; }
        [Column("דרגה1_גמולי_השתלמות")]
        public decimal? Grade1TrainingBenefits { get; set; }
        [Column("דרגה1_כפל_תואר")]
        public decimal? Grade1DoubleDegree { get; set; }

        // סיכומים — דרגה 2
        [Column("דרגה2_סהכ")]
        public decimal? Grade2Total { get; set; }
        [Column("דרגה2_אחוז_משרה")]
        public decimal? Grade2JobPercent { get; set; }
        [Column("דרגה2_קרן_השתלמות_אחוז")]
        public decimal? Grade2TrainingFundPercent { get; set; }
        [Column("דרגה2_שעות_גיל")]
        public decimal? Grade2AgeHours { get; set; }
        [Column("דרגה2_אחוז_תוספת_אם")]
        public decimal? Grade2MotherBenefitPercent { get; set; }
        [Column("דרגה2_גמולי_השתלמות")]
        public decimal? Grade2TrainingBenefits { get; set; }
        [Column("דרגה2_כפל_תואר")]
        public decimal? Grade2DoubleDegree { get; set; }

        /// <summary>שם הדירוג — משותף לכל מקטעי דרגה 1</summary>
        [Column("דרגה1_שם_הדירוג")]
        public string? Grade1GradeName { get; set; }

        [Column("דרגה1_דרגה")]
        public string? Grade1Grade { get; set; }

        [Column("דרגה1_תפקיד")]
        public string? Grade1Role { get; set; }

        [Column("דרגה1_ותק")]
        public string? Grade1Seniority { get; set; }

        [Column("דרגה2_שם_הדירוג")]
        public string? Grade2GradeName { get; set; }

        [Column("דרגה2_דרגה")]
        public string? Grade2Grade { get; set; }

        [Column("דרגה2_תפקיד")]
        public string? Grade2Role { get; set; }

        [Column("דרגה2_ותק")]
        public string? Grade2Seniority { get; set; }

        public DateTimeOffset CreatedAtUtc { get; set; }
        public DateTimeOffset UpdatedAtUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTimeOffset? DeletedAtUtc { get; set; }

        [ForeignKey("EmployeeId")] public Employee? Employee { get; set; }
        [ForeignKey("EmployerId")] public Employer? Employer { get; set; }

        public ICollection<EmploymentDataSlot> Slots { get; set; } = new List<EmploymentDataSlot>();

        [NotMapped] public string PeriodDisplay => AcademicYear;
    }
}






