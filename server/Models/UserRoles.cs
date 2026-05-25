namespace AccountingProject.Models
{
    public static class UserRoles
    {
        public const string Admin = nameof(Admin);
        public const string PayrollManager = nameof(PayrollManager);
        public const string Viewer = nameof(Viewer);

        public static IReadOnlyList<string> All { get; } = [Admin, PayrollManager, Viewer];
    }
}
