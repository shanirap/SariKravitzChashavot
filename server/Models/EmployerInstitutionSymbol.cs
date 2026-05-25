using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AccountingProject.Models
{
    [Table("סמלי_מוסד_מעסיקים")]
    [Index(nameof(EmployerId), nameof(InstitutionSymbol), IsUnique = true)]
    public class EmployerInstitutionSymbol
    {
        [Key]
        [Column("מזהה_סמל_מוסד_מעסיק")]
        public int Id { get; set; }

        [Column("מזהה_מעסיק")]
        [Display(Name = "מזהה מעסיק")]
        public int EmployerId { get; set; }

        [Required]
        [Column("סמל_מוסד")]
        [Display(Name = "סמל מוסד")]
        public string InstitutionSymbol { get; set; } = string.Empty;

        [Column("שם_סמל_מוסד")]
        [Display(Name = "שם סמל מוסד")]
        public string? InstitutionSymbolName { get; set; }

        [ForeignKey(nameof(EmployerId))]
        public Employer? Employer { get; set; }
    }
}
