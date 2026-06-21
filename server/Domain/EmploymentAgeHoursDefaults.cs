namespace AccountingProject.Domain
{
    /// <summary>
    /// שעות גיל לפי גיל העובד בתחילת שנת הלימודים — תואם ל-frontend.
    /// </summary>
    public static class EmploymentAgeHoursDefaults
    {
        public static decimal? Compute(DateOnly? birthDate, DateOnly refDate)
        {
            if (!birthDate.HasValue)
                return null;

            var age = AgeInFullYearsAtDate(birthDate.Value, refDate);
            if (!age.HasValue)
                return null;
            if (age.Value < 50)
                return 0m;
            if (age.Value < 55)
                return 2m;
            return 4m;
        }

        public static int? AgeInFullYearsAtDate(DateOnly birthDate, DateOnly refDate)
        {
            var age = refDate.Year - birthDate.Year;
            if (refDate.Month < birthDate.Month
                || (refDate.Month == birthDate.Month && refDate.Day < birthDate.Day))
            {
                age--;
            }

            return age;
        }
    }
}
