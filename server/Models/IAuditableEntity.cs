namespace AccountingProject.Models
{
    public interface IAuditableEntity
    {
        DateTimeOffset CreatedAtUtc { get; set; }
        DateTimeOffset UpdatedAtUtc { get; set; }
    }
}
