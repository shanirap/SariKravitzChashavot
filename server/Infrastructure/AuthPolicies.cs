using Microsoft.AspNetCore.Authorization;

namespace AccountingProject.Infrastructure
{
    /// <summary>Named authorization policy for privileged account management endpoints.</summary>
    public static class AuthPolicies
    {
        public const string AdminOnly = nameof(AdminOnly);
    }

    /// <summary>Restricts create/update/delete/import endpoints to Admin role.</summary>
    public sealed class AdminWriteAttribute : AuthorizeAttribute
    {
        public AdminWriteAttribute() => Policy = AuthPolicies.AdminOnly;
    }
}
