namespace AccountingProject.Domain
{
    /// <summary>
    /// תוספת אם: נקבה + ילד עד גיל 14 (ביחס לתחילת שנת לימודים) + משרה בסיסית מעל 79%.
    /// </summary>
    public static class MotherBenefitRules
    {
        public const int ChildMaxAgeInclusive = 14;
        public const decimal BaseJobPercentThreshold = 79m;

        private static readonly Dictionary<string, decimal> RateByGradeName = new(StringComparer.Ordinal)
        {
            ["יסודי וגנים"] = 10m,
            ["עוז לתמורה"] = 7m,
            ["אופק חדש"] = 10m,
            ["אופק גנים"] = 10m,
        };

        public static bool TryGetRateForGradeName(string? gradeName, out decimal rate)
        {
            rate = default;
            var gn = GradeOptions.NormalizeGradeName(gradeName) ?? string.Empty;
            if (gn.Length == 0 || gn == GradeOptions.UnifiedEducationSupportGradeName)
                return false;
            return RateByGradeName.TryGetValue(gn, out rate);
        }

        public static bool HasChildUpToAgeInclusive(IReadOnlyList<DateOnly?> childBirthDates, DateOnly refDate, int maxAgeInclusive = ChildMaxAgeInclusive)
        {
            foreach (var birth in childBirthDates)
            {
                if (!birth.HasValue) continue;
                var age = AgeInFullYearsAtDate(birth.Value, refDate);
                if (age <= maxAgeInclusive) return true;
            }
            return false;
        }

        public static int AgeInFullYearsAtDate(DateOnly birth, DateOnly refDate)
        {
            var age = refDate.Year - birth.Year;
            if (refDate.Month < birth.Month || (refDate.Month == birth.Month && refDate.Day < birth.Day))
                age--;
            return age;
        }

        /// <summary>
        /// null = שם דירוג ריק / לא במפת אחוזים; 0 = לא זכאית; otherwise rate.
        /// </summary>
        public static decimal? ComputePercent(
            string? gradeName,
            bool isFemaleEmployee,
            IReadOnlyList<DateOnly?> childBirthDates,
            DateOnly refDate,
            decimal? baseJobPercent)
        {
            var gn = GradeOptions.NormalizeGradeName(gradeName) ?? string.Empty;
            if (gn.Length == 0)
                return null;
            if (gn == GradeOptions.UnifiedEducationSupportGradeName)
                return 0m;
            if (!TryGetRateForGradeName(gn, out var rate))
                return null;
            if (!isFemaleEmployee)
                return 0m;
            if (!HasChildUpToAgeInclusive(childBirthDates, refDate))
                return 0m;
            if (!baseJobPercent.HasValue || baseJobPercent.Value <= BaseJobPercentThreshold)
                return 0m;
            return rate;
        }
    }
}
