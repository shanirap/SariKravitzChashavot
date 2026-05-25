using Microsoft.AspNetCore.Http;

namespace AccountingProject.Infrastructure
{
    /// <summary>
    /// Central rules for validating Excel uploads (.xlsx only). Client-provided filenames are never used for filesystem paths.
    /// </summary>
    public static class ExcelUploadRules
    {
        public const long BulkImportMaxBytes = 10L * 1024 * 1024;

        /// <summary>
        /// ~50&nbsp;MiB; keep in sync with the monthly-payroll comparison route <c>RequestSizeLimit</c>.
        /// </summary>
        public const long ComparisonMonthlyPayrollMaxBytes = 52_428_800;

        /// <summary>
        /// Validates presence, extension (.xlsx only), explicit rejection of .xls/.xlsm, and maximum size.
        /// </summary>
        public static bool TryValidateStrictXlsx(IFormFile? file, long maxBytes, out string? errorMessageHebrew)
        {
            errorMessageHebrew = null;

            if (file == null || file.Length == 0)
            {
                errorMessageHebrew = "לא הועלה קובץ תקין.";
                return false;
            }

            // Only use the suffix for validation — never combine with paths or persist the uploaded name.
            var ext = Path.GetExtension(file.FileName);
            if (string.IsNullOrWhiteSpace(ext))
            {
                errorMessageHebrew = "לא ניתן לזהות סוג הקובץ. יש להעלות קובץ עם סיומת .xlsx בלבד.";
                return false;
            }

            ext = ext.Trim();

            if (ext.Equals(".xlsm", StringComparison.OrdinalIgnoreCase)
                || ext.Equals(".xls", StringComparison.OrdinalIgnoreCase))
            {
                errorMessageHebrew =
                    "סוג הקובץ אינו נתמך. ניתן להעלות רק קובץ Excel בפורמט .xlsx (לא .xlsm ולא .xls ישנים).";
                return false;
            }

            if (!ext.Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                errorMessageHebrew = "סוג הקובץ אינו נתמך. יש להעלות קובץ .xlsx בלבד.";
                return false;
            }

            if (file.Length > maxBytes)
            {
                var megabytesRounded = Math.Max(1, (int)Math.Ceiling(maxBytes / (1024.0 * 1024.0)));
                errorMessageHebrew = $"הקובץ גדול מדי. הגודל המרבי המותר הוא {megabytesRounded}MB.";
                return false;
            }

            return true;
        }
    }
}
