namespace AccountingProject.Domain
{
    /// <summary>
    /// בסיס משרה לחישוב אחוז משרה: בסיס גולמי (שמור במקטע) פחות שעות גיל של אותה רמת דרגה.
    /// </summary>
    public static class EmploymentJobBaseAdjustments
    {
        public static decimal? NetJobBaseAfterAgeHours(decimal? grossJobBase, decimal? ageHours)
        {
            if (!grossJobBase.HasValue) return null;
            var deduct = ageHours ?? 0m;
            if (deduct < 0m) deduct = 0m;
            var net = grossJobBase.Value - deduct;
            if (net < 0m) net = 0m;
            return Math.Round(net, 2);
        }
    }
}
