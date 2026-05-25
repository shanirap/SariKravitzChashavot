namespace AccountingProject.Domain
{
    /// <summary>
    /// Maps Gregorian calendar months to Hebrew academic year labels stored in employment records.
    /// Israeli school year: September through August (months 9–12 then 1–8).
    /// Hebrew civil year corresponding to Sep of Gregorian year G is approximated as G + 3761 for labeling via HebrewAcademicYear.Format.
    /// </summary>
    public static class SchoolYearGregorian
    {
        /// <summary>Returns Hebrew academic year label (e.g. תשפ\"ו) for the school year containing this Gregorian month/year.</summary>
        public static string GetSchoolYearFromGregorianMonth(int month, int gregorianYear)
        {
            if (month is < 1 or > 12)
                throw new ArgumentOutOfRangeException(nameof(month));
            var hebrewYearNumber = month >= 9 ? gregorianYear + 3761 : gregorianYear + 3760;
            return HebrewAcademicYear.Format(hebrewYearNumber);
        }

        /// <summary>Gregorian calendar year of September that starts the school year containing this month/year.</summary>
        public static int GetSeptemberGregorianYearForSchoolYearContaining(int month, int gregorianYear) =>
            month >= 9 ? gregorianYear : gregorianYear - 1;

        /// <summary>Ordered list (Sep…Aug) of (month, gregorianYear) pairs for one school year starting at septemberGregorianYear.</summary>
        public static IReadOnlyList<(int Month, int GregorianYear)> GetSchoolYearMonthSequence(int septemberGregorianYear)
        {
            var list = new List<(int, int)>();
            for (var m = 9; m <= 12; m++)
                list.Add((m, septemberGregorianYear));
            var janYear = septemberGregorianYear + 1;
            for (var m = 1; m <= 8; m++)
                list.Add((m, janYear));
            return list;
        }
    }
}
