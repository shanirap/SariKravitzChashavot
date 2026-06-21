namespace AccountingProject.Domain
{
    /// <summary>
    /// ברירות מחדל לבסיס משרה לפי שם דירוג ותפקיד — תואם ללוגיקת ה-frontend.
    /// </summary>
    public static class EmploymentJobBaseDefaults
    {
        private static readonly Dictionary<string, decimal> DefaultByGradeName =
            new(StringComparer.Ordinal)
            {
                ["יסודי וגנים"] = 30m,
                [GradeOptions.UnifiedEducationSupportGradeName] = 40m,
                ["עוז לתמורה"] = 38m,
                ["אופק חדש"] = 36m,
                ["אופק גנים"] = 36m,
            };

        private static readonly Dictionary<string, decimal> ByRole =
            new(StringComparer.Ordinal)
            {
                ["גננת ראשית"] = 30m,
                ["גננת עמיתה"] = 33.8m,
            };

        private static readonly Dictionary<string, decimal> ByOfekGanimRole =
            new(StringComparer.Ordinal)
            {
                ["גננת ראשית"] = 30.4m,
                ["גננת עמיתה"] = 33.8m,
                ["פרא רפואי"] = 33.8m,
            };

        public static decimal? GetJobBaseValue(string? gradeName, string? role)
        {
            var gn = GradeOptions.NormalizeGradeName(gradeName);
            var r = role?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(gn) || string.IsNullOrWhiteSpace(r))
                return null;

            if (gn == "אופק גנים" && ByOfekGanimRole.TryGetValue(r, out var ofekValue))
                return ofekValue;
            if (ByRole.TryGetValue(r, out var roleValue))
                return roleValue;
            if (DefaultByGradeName.TryGetValue(gn, out var gradeValue))
                return gradeValue;
            return null;
        }
    }
}
