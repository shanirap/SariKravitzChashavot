namespace AccountingProject.Infrastructure
{
    public interface ICurrentUserService
    {
        string? UserId { get; }
        string? Username { get; }
        string? Role { get; }

        string GetAuditActor();
    }
}
