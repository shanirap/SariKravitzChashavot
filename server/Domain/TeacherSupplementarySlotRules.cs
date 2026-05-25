namespace AccountingProject.Domain
{
    /// <summary>
    /// שלוש שעות נוספות למחנך/גננת — רשומת מקטע משנה מתחת עם אותו סמל מוסד.
    /// </summary>
    public static class TeacherSupplementarySlotRules
    {
        public static bool Qualifies(string? gradeName, string? role)
        {
            var g = gradeName?.Trim() ?? string.Empty;
            var r = role?.Trim() ?? string.Empty;
            return g == "יסודי וגנים" && r == "גננת ראשית"
                   || g == "עוז לתמורה" && r == "מורה מחנך";
        }
    }
}
