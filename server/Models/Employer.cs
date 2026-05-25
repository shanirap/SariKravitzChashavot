using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AccountingProject.Models
{
    [Table("מעסיקים")]
    [Index(nameof(Name))]
    [Index(nameof(BusinessNumber))]
    public class Employer : IAuditableEntity, ISoftDeletable
    {
        [Key]
        [Column("מזהה_מעסיק")]
        public int Id { get; set; }

        [Required]
        [Column("שם_מעסיק")]
        [Display(Name = "שם מעסיק")]
        public string Name { get; set; } = string.Empty;

        [Column("חפ")]
        [Display(Name = "ח.פ.")]
        public string? BusinessNumber { get; set; }

        [Column("סמל_מוטב")]
        [Display(Name = "סמל מוטב")]
        public string? BeneficiarySymbol { get; set; }

        [Column("מספר_עוקץ")]
        [Display(Name = "מספר עוקץ")]
        public string? EketzNumber { get; set; }

        public DateTimeOffset CreatedAtUtc { get; set; }
        public DateTimeOffset UpdatedAtUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTimeOffset? DeletedAtUtc { get; set; }

        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
        public ICollection<EmploymentData> EmploymentData { get; set; } = new List<EmploymentData>();
    }
}
