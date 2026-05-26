namespace AccountingProject.Domain
{
    /// <summary>ערכי סוג מוסד מותרים לסמלי מוסד של מעסיק.</summary>
    public static class InstitutionTypes
    {
        public const string School = "בית ספר";
        public const string Kindergarten = "גן";
        public const string Other = "אחר";
        public const string Default = Other;

        public static readonly IReadOnlySet<string> Allowed = new HashSet<string>(StringComparer.Ordinal)
        {
            School, Kindergarten, Other,
        };

        public static readonly IReadOnlyList<string> All = [School, Kindergarten, Other];

        /// <summary>מחזיר סוג מוסד תקין; null/ריק → ברירת מחדל. ערך לא חוקי → שגיאה.</summary>
        public static (string Type, string? Error) Resolve(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return (Default, null);

            var trimmed = value.Trim();
            if (Allowed.Contains(trimmed))
                return (trimmed, null);

            return (Default, $"סוג מוסד חייב להיות אחד מ: {School}, {Kindergarten}, {Other}.");
        }
    }
}
