using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccountingProject.Models
{
    [Table("Users")]
    public class User
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(128)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [MaxLength(64)]
        public string Role { get; set; } = UserRoles.Viewer;

        public bool IsActive { get; set; } = true;

        public DateTimeOffset CreatedAt { get; set; }
    }
}
