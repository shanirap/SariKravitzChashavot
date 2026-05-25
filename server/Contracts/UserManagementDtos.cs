namespace AccountingProject.Contracts
{
    /// <summary>Returned by user management endpoints; excludes secrets.</summary>
    public sealed class UserSummaryDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }

    /// <summary>Creates a local office account (Admin API only).</summary>
    public sealed class AdminCreateUserRequestDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    /// <summary>Sets a user's password hash (Admin API only).</summary>
    public sealed class SetPasswordRequestDto
    {
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>Toggles login eligibility for an account (uses IsActive).</summary>
    public sealed class SetUserActiveRequestDto
    {
        public bool IsActive { get; set; }
    }
}
