using System.ComponentModel.DataAnnotations;
using AccountingProject.Models;

namespace AccountingProject.Contracts
{
    public class LoginRequestDto
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTimeOffset ExpiresAtUtc { get; set; }
    }

    public class CreateUserDto
    {
        [Required]
        [MaxLength(128)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [MaxLength(64)]
        public string Role { get; set; } = UserRoles.Admin;
    }
}
