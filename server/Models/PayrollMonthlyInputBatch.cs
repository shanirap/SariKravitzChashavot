using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AccountingProject.Models
{
    /// <summary>Uploaded Okets payroll file metadata for one employer, academic year, and calendar month.</summary>
    [Table("קלט_עוקץ_חודשי_אצווה")]
    [Index(nameof(EmployerId), nameof(AcademicYear), nameof(Month), nameof(GregorianYear))]
    public class PayrollMonthlyInputBatch
    {
        [Key]
        [Column("מזהה_אצווה")]
        public int Id { get; set; }

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

        [MaxLength(500)]
        [Column("שם_קובץ_מקורי")]
        public string OriginalFileName { get; set; } = string.Empty;

        [Column("הועלה_בתאריך")]
        public DateTime UploadedAtUtc { get; set; }

        [MaxLength(200)]
        [Column("הועלה_על_ידי")]
        public string? UploadedBy { get; set; }

        [Column("מספר_שורות")]
        public int RowsCount { get; set; }

        [Column("פעיל")]
        public bool IsActive { get; set; } = true;

        [Column("נמחק")]
        public bool IsDeleted { get; set; }

        [Column("נמחק_בתאריך")]
        public DateTime? DeletedAtUtc { get; set; }

        [Column("נוצר_בתאריך")]
        public DateTime CreatedAtUtc { get; set; }

        [Column("עודכן_בתאריך")]
        public DateTime? UpdatedAtUtc { get; set; }

        [ForeignKey(nameof(EmployerId))]
        public Employer? Employer { get; set; }

        public ICollection<PayrollMonthlyInputRow> Rows { get; set; } = new List<PayrollMonthlyInputRow>();
    }
}
