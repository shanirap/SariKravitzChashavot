namespace AccountingProject.Infrastructure
{
    /// <summary>Named authorization policy for privileged account management endpoints.</summary>
    public static class AuthPolicies
    {
        public const string AdminOnly = nameof(AdminOnly);
    }
}
