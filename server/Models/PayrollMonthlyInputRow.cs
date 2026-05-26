using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AccountingProject.Models
{
    /// <summary>One parsed row from an uploaded Okets monthly payroll Excel file.</summary>
    [Table("קלט_עוקץ_חודשי_שורה")]
    [Index(nameof(BatchId))]
    [Index(nameof(EmployerId), nameof(AcademicYear), nameof(Month), nameof(GregorianYear))]
    public class PayrollMonthlyInputRow
    {
        [Key]
        [Column("מזהה_שורה")]
        public int Id { get; set; }

        [Column("מזהה_אצווה")]
        public int BatchId { get; set; }

        [Column("מזהה_מעסיק")]
        public int EmployerId { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("שנת_לימודים")]
        public string AcademicYear { get; set; } = string.Empty;

        [Column("חודש")]
        public int Month { get; set; }

        [Column("שנה_גרגוריאנית")]
        public int GregorianYear { get; set; }

        [Column("מספר_שורה_באקסל")]
        public int? SourceExcelRowNumber { get; set; }

        [MaxLength(50)]
        [Column("סמל_מוסד")]
        public string? InstitutionSymbol { get; set; }

        [MaxLength(50)]
        [Column("מספר_עובד_בעוקץ")]
        public string? OketzEmployeeNumber { get; set; }

        [MaxLength(20)]
        [Column("תז")]
        public string? IdNumber { get; set; }

        [MaxLength(200)]
        [Column("שם_מלא")]
        public string? FullName { get; set; }

        [MaxLength(100)]
        [Column("תפקיד")]
        public string? Role { get; set; }

        [MaxLength(50)]
        [Column("דרגה")]
        public string? Grade { get; set; }

        [Column("ותק")]
        public decimal? Seniority { get; set; }

        [Column("שעות_שבועיות")]
        public decimal? WeeklyHours { get; set; }

        [Column("בסיס_משרה")]
        public decimal? JobBase { get; set; }

        [Column("אחוז_משרה")]
        public decimal? JobPercent { get; set; }

        [Column("שעות_גיל")]
        public decimal? AgeHours { get; set; }

        [Column("גמולי_השתלמות")]
        public decimal? TrainingBenefits { get; set; }

        [Column("כפל_תואר")]
        public decimal? DoubleDegree { get; set; }

        [Column("קרן_השתלמות")]
        public decimal? TrainingFund { get; set; }

        [Column("הכפלה_כללית")]
        public decimal? GeneralMultiplier { get; set; }

        [Column("נערך_ידנית")]
        public bool IsManualEdited { get; set; }

        [MaxLength(500)]
        [Column("הערת_עריכה_ידנית")]
        public string? ManualEditNote { get; set; }

        [Column("תאים_גולמיים_json")]
        public string? RawCellsJson { get; set; }

        [Column("נמחק")]
        public bool IsDeleted { get; set; }

        [Column("נמחק_בתאריך")]
        public DateTime? DeletedAtUtc { get; set; }

        [Column("נוצר_בתאריך")]
        public DateTime CreatedAtUtc { get; set; }

        [Column("עודכן_בתאריך")]
        public DateTime? UpdatedAtUtc { get; set; }

        [ForeignKey(nameof(BatchId))]
        public PayrollMonthlyInputBatch? Batch { get; set; }
    }
}
