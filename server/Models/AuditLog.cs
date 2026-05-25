using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccountingProject.Models
{
    [Table("AuditLogs")]
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string EntityName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Action { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? EntityKey { get; set; }

        public string? ChangesJson { get; set; }

        [MaxLength(100)]
        public string ChangedBy { get; set; } = "system";

        public DateTimeOffset ChangedAtUtc { get; set; }
    }
}
