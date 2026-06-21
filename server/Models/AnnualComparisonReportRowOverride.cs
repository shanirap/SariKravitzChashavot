using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AccountingProject.Models;

/// <summary>Manual display overrides for annual comparison report rows (per employment slot).</summary>
[Table("דריסות_דוח_השוואה_שנתי")]
[Index(nameof(EmployerId), nameof(AcademicYear), nameof(SlotId), IsUnique = true)]
public class AnnualComparisonReportRowOverride
{
    [Key]
    [Column("מזהה")]
    public int Id { get; set; }

    [Column("מזהה_מעסיק")]
    public int EmployerId { get; set; }

    [Required]
    [MaxLength(20)]
    [Column("שנת_לימודים")]
    public string AcademicYear { get; set; } = string.Empty;

    [Column("מזהה_מקטע")]
    public int SlotId { get; set; }

    [MaxLength(50)]
    [Column("סמל_מוסד")]
    public string? InstitutionSymbol { get; set; }

    [MaxLength(200)]
    [Column("שם_מלא")]
    public string? FullName { get; set; }

    [MaxLength(100)]
    [Column("תפקיד")]
    public string? Role { get; set; }

    [MaxLength(100)]
    [Column("סוג_משרה_מעוקץ")]
    public string? SugMisraFromPayroll { get; set; }

    [MaxLength(50)]
    [Column("דרגה")]
    public string? Grade { get; set; }

    [MaxLength(50)]
    [Column("ותק")]
    public string? Seniority { get; set; }

    [Column("שעות_שבועיות")]
    public decimal? WeeklyHours { get; set; }

    [Column("בסיס_משרה")]
    public decimal? JobBase { get; set; }

    [Column("אחוז_משרה")]
    public decimal? JobPercent { get; set; }

    [Column("הכפלה_כללית")]
    public decimal? DoubleGeneral { get; set; }

    [Column("תאי_חודש_json")]
    public string? MonthCellsJson { get; set; }

    [Column("נערך_ידנית")]
    public bool IsManualEdited { get; set; }

    [MaxLength(500)]
    [Column("הערת_עריכה")]
    public string? ManualEditNote { get; set; }

    [Column("נוצר_בתאריך")]
    public DateTime CreatedAtUtc { get; set; }

    [Column("עודכן_בתאריך")]
    public DateTime? UpdatedAtUtc { get; set; }

    [ForeignKey(nameof(SlotId))]
    public EmploymentDataSlot? Slot { get; set; }

    [ForeignKey(nameof(EmployerId))]
    public Employer? Employer { get; set; }
}
