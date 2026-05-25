namespace AccountingProject.Models
{
    public interface ISoftDeletable
    {
        bool IsDeleted { get; set; }
        DateTimeOffset? DeletedAtUtc { get; set; }
    }
}
