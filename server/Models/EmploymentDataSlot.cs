using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AccountingProject.Models
{
    [Table("נתוני_העסקה_מקטע")]
    [Index(nameof(EmploymentDataId), nameof(GradeBand), nameof(SlotIndex), IsUnique = true)]
    public class EmploymentDataSlot
    {
        [Key][Column("מזהה_מקטע")]
        public int Id { get; set; }

        [Column("מזהה_נתון_העסקה")]
        public int EmploymentDataId { get; set; }

        /// <summary>1 = דרגה 1, 2 = דרגה 2</summary>
        [Column("רמת_דרגה")]
        public byte GradeBand { get; set; }

        /// <summary>1–6</summary>
        [Column("אינדקס_מקטע")]
        public byte SlotIndex { get; set; }

        [Column("סמל_מוסד")]
        public string? InstitutionSymbol { get; set; }

        [Column("שעות_שבועיות")]
        public decimal? WeeklyHours { get; set; }

        /// <summary>בסיס משרה נטו במקטע — בסיס גולמי לפי דירוג/תפקיד פחות שעות גיל של אותה רמת דרגה.</summary>
        [Column("בסיס_משרה")]
        public decimal? JobBase { get; set; }

        /// <summary>
        /// אינדקס מקטע ההורה (1–5) בשורת השעות הנוספות; null = רגיל.
        /// כשלא null — <see cref="SlotIndex"/> חייב להיות ההורה+1, והשעות 3 למחנך/גננת לפי התניה.
        /// </summary>
        [Column("מקטע_הורה_שעות_נוספות")]
        public byte? SupplementaryParentSlotIndex { get; set; }

        [ForeignKey(nameof(EmploymentDataId))]
        public EmploymentData? EmploymentData { get; set; }
    }
}
