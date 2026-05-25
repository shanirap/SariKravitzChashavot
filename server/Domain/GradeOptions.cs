namespace AccountingProject.Domain
{
    public static class GradeOptions
    {
        private static readonly string[] CoreGrades =
        [
            "ב",
            "בכיר",
            "גננת מוסמכת",
            "ד\"ר",
            "מ.א.",
            "מורה מוסמך"
        ];

        private static readonly string[] OfekGrades =
        [
            "1",
            "1.5",
            "2",
            "2.5",
            "3",
            "3.5",
            "4",
            "4.5",
            "5",
            "5.5",
            "6",
            "6.5",
            "7",
            "7.5",
            "8",
            "8.5",
            "9"
        ];

        private static readonly Dictionary<string, string[]> OptionsByGradeName = new(StringComparer.Ordinal)
        {
            ["יסודי וגנים"] = CoreGrades,
            ["אחיד"] = ["תומכת חינוך", "תומכת חינוך חנ\"מ"],
            ["עוז לתמורה"] = CoreGrades,
            ["אופק חדש"] = OfekGrades,
            ["אופק גנים"] = OfekGrades
        };

        private static readonly Dictionary<string, string[]> RolesByGradeName = new(StringComparer.Ordinal)
        {
            ["יסודי וגנים"] = ["גננת ראשית", "גננת משלימה", "גננת שילוב", "מורה מחנך", "מורה מקצועי", "מנהל"],
            ["אחיד"] = ["סייעת ראשית", "סייעת משלימה", "סייעת שניה"],
            ["עוז לתמורה"] = ["מורה מחנך", "מורה מקצועי", "מנהל"],
            ["אופק חדש"] = ["גננת ראשית", "גננת משלימה", "גננת שילוב", "מורה מחנך", "מורה מקצועי", "מנהל", "פרא רפואי"],
            ["אופק גנים"] = ["גננת ראשית", "גננת עמיתה", "פרא רפואי"]
        };

        public static IReadOnlyDictionary<string, string[]> Options => OptionsByGradeName;
        public static IReadOnlyDictionary<string, string[]> Roles => RolesByGradeName;

        public static IReadOnlyList<string> GradeNames => OptionsByGradeName.Keys.ToArray();

        public static bool IsKnownGradeName(string? gradeName) =>
            !string.IsNullOrWhiteSpace(gradeName) && OptionsByGradeName.ContainsKey(gradeName.Trim());

        public static bool IsValidGrade(string? gradeName, string? grade)
        {
            if (string.IsNullOrWhiteSpace(grade))
                return true;

            if (string.IsNullOrWhiteSpace(gradeName))
                return false;

            return OptionsByGradeName.TryGetValue(gradeName.Trim(), out var grades)
                   && grades.Contains(grade.Trim(), StringComparer.Ordinal);
        }

        public static bool IsValidRole(string? gradeName, string? role)
        {
            if (string.IsNullOrWhiteSpace(role))
                return true;

            if (string.IsNullOrWhiteSpace(gradeName))
                return false;

            return RolesByGradeName.TryGetValue(gradeName.Trim(), out var roles)
                   && roles.Contains(role.Trim(), StringComparer.Ordinal);
        }

        public static bool IsValidSeniority(string? seniority)
        {
            if (string.IsNullOrWhiteSpace(seniority))
                return true;

            return int.TryParse(seniority.Trim(), out var value) && value >= 0;
        }

        public static string? GetGradeBandValidationError(int band, string? gradeName, string? grade, string? role, string? seniority)
        {
            if (!string.IsNullOrWhiteSpace(gradeName) && !IsKnownGradeName(gradeName))
                return $"שם הדירוג בדרגה {band} אינו תקין.";
            if (!IsValidGrade(gradeName, grade))
                return $"הדרגה בדרגה {band} אינה תואמת לשם הדירוג.";
            if (!IsValidRole(gradeName, role))
                return $"התפקיד בדרגה {band} אינו תואם לשם הדירוג.";
            if (!IsValidSeniority(seniority))
                return $"ותק בדרגה {band} חייב להיות מספר שלם גדול או שווה ל-0.";
            return null;
        }
    }
}
