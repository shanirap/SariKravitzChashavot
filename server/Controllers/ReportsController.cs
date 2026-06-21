using AccountingProject.Contracts;
using AccountingProject.Domain;
using AccountingProject.Infrastructure;
using AccountingProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccountingProject.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/reports")]
    public class ReportsController : ControllerBase
    {
        private const string XlsxMime = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        private readonly IReportExportService _reports;
        private readonly IAnnualComparisonSavedReportService _annualSavedReport;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(
            IReportExportService reports,
            IAnnualComparisonSavedReportService annualSavedReport,
            ILogger<ReportsController> logger)
        {
            _reports = reports;
            _annualSavedReport = annualSavedReport;
            _logger = logger;
        }

        // GET /api/reports/kindergarten-annual?employerId=&academicYear=
        [HttpGet("kindergarten-annual")]
        public async Task<IActionResult> KindergartenAnnual(
            [FromQuery] int employerId,
            [FromQuery] string academicYear)
        {
            if (employerId <= 0 || string.IsNullOrWhiteSpace(academicYear))
                return BadRequest(new { message = "employerId ו-academicYear נדרשים." });
            if (!HebrewAcademicYear.TryValidateAndCanonicalize(academicYear, out var canonicalYear))
                return BadRequest(new { message = HebrewAcademicYear.InvalidMessage });
            try
            {
                var bytes = await _reports.KindergartenAnnualAsync(employerId, canonicalYear);
                return File(bytes, XlsxMime, $"מצבת_גנים_{employerId}_{DateTime.UtcNow:yyyyMMdd}.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "KindergartenAnnual report failed for employer {EmployerId}.", employerId);
                return StatusCode(500, new { message = "שגיאה בהפקת הדוח." });
            }
        }

        // GET /api/reports/school-annual?employerId=&academicYear=
        [HttpGet("school-annual")]
        public async Task<IActionResult> SchoolAnnual(
            [FromQuery] int employerId,
            [FromQuery] string academicYear)
        {
            if (employerId <= 0 || string.IsNullOrWhiteSpace(academicYear))
                return BadRequest(new { message = "employerId ו-academicYear נדרשים." });
            if (!HebrewAcademicYear.TryValidateAndCanonicalize(academicYear, out var canonicalYear))
                return BadRequest(new { message = HebrewAcademicYear.InvalidMessage });
            try
            {
                var bytes = await _reports.SchoolAnnualAsync(employerId, canonicalYear);
                return File(bytes, XlsxMime, $"מצבת_בית_ספר_{employerId}_{DateTime.UtcNow:yyyyMMdd}.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SchoolAnnual report failed for employer {EmployerId}.", employerId);
                return StatusCode(500, new { message = "שגיאה בהפקת הדוח." });
            }
        }

        // POST /api/reports/monthly-comparison?employerId=&academicYear=&month=
        // Body: multipart/form-data with "file" (.xlsx) containing monthly עוקץ data for comparison.
        [HttpPost("monthly-comparison")]
        [AdminWrite]
        [RequestSizeLimit(ExcelUploadRules.ComparisonMonthlyPayrollMaxBytes)]
        public async Task<IActionResult> MonthlyComparison(
            [FromQuery] int employerId,
            [FromQuery] string academicYear,
            [FromQuery] int month,
            IFormFile? file)
        {
            if (employerId <= 0 || string.IsNullOrWhiteSpace(academicYear) || month < 1 || month > 12)
                return BadRequest(new { message = "employerId, academicYear וחודש תקין (1–12) נדרשים." });
            if (!HebrewAcademicYear.TryValidateAndCanonicalize(academicYear, out var canonicalYear))
                return BadRequest(new { message = HebrewAcademicYear.InvalidMessage });
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "יש לצרף קובץ Excel (.xlsx) עם נתוני עוקץ." });
            try
            {
                await using var stream = file.OpenReadStream();
                var bytes = await _reports.MonthlyComparisonAsync(employerId, canonicalYear, month, stream);
                return File(bytes, XlsxMime, $"השוואה_חודשית_{month}_{employerId}_{DateTime.UtcNow:yyyyMMdd}.xlsx");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "MonthlyComparison rejected for employer {EmployerId} month {Month}.", employerId, month);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MonthlyComparison report failed for employer {EmployerId} month {Month}.", employerId, month);
                return StatusCode(500, new { message = "שגיאה בהפקת הדוח." });
            }
        }

        // POST /api/reports/annual-comparison?employerId=&academicYear=
        // Body: multipart/form-data with "file" (.xlsx) containing annual עוקץ data for comparison.
        [HttpPost("annual-comparison")]
        [AdminWrite]
        [RequestSizeLimit(ExcelUploadRules.ComparisonMonthlyPayrollMaxBytes)]
        public async Task<IActionResult> AnnualComparison(
            [FromQuery] int employerId,
            [FromQuery] string academicYear,
            IFormFile? file)
        {
            if (employerId <= 0 || string.IsNullOrWhiteSpace(academicYear))
                return BadRequest(new { message = "employerId ו-academicYear נדרשים." });
            if (!HebrewAcademicYear.TryValidateAndCanonicalize(academicYear, out var canonicalYear))
                return BadRequest(new { message = HebrewAcademicYear.InvalidMessage });
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "יש לצרף קובץ Excel (.xlsx) עם נתוני עוקץ." });
            try
            {
                await using var stream = file.OpenReadStream();
                var bytes = await _reports.AnnualComparisonAsync(employerId, canonicalYear, stream);
                return File(bytes, XlsxMime, $"השוואה_שנתית_{employerId}_{DateTime.UtcNow:yyyyMMdd}.xlsx");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "AnnualComparison rejected for employer {EmployerId}.", employerId);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AnnualComparison report failed for employer {EmployerId}.", employerId);
                return StatusCode(500, new { message = "שגיאה בהפקת הדוח." });
            }
        }

        // GET /api/reports/annual-comparison-saved/preview?employerId=&academicYear=
        [HttpGet("annual-comparison-saved/preview")]
        public async Task<IActionResult> AnnualComparisonSavedPreview(
            [FromQuery] int employerId,
            [FromQuery] string academicYear)
        {
            if (employerId <= 0 || string.IsNullOrWhiteSpace(academicYear))
                return BadRequest(new { message = "employerId ו-academicYear נדרשים." });
            if (!HebrewAcademicYear.TryValidateAndCanonicalize(academicYear, out var canonicalYear))
                return BadRequest(new { message = HebrewAcademicYear.InvalidMessage });
            try
            {
                var preview = await _annualSavedReport.GetPreviewAsync(employerId, canonicalYear);
                return Ok(preview);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "AnnualComparisonSavedPreview rejected for employer {EmployerId}.", employerId);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AnnualComparisonSavedPreview failed for employer {EmployerId}.", employerId);
                return StatusCode(500, new { message = "שגיאה בטעינת תצוגת הדוח." });
            }
        }

        // PUT /api/reports/annual-comparison-saved/overrides
        [HttpPut("annual-comparison-saved/overrides")]
        [AdminWrite]
        public async Task<IActionResult> SaveAnnualComparisonOverrides(
            [FromBody] AnnualComparisonOverrideSaveRequest request)
        {
            if (request == null
                || request.EmployerId <= 0
                || string.IsNullOrWhiteSpace(request.AcademicYear)
                || request.Rows == null)
                return BadRequest(new { message = "employerId, academicYear ו-rows נדרשים." });
            if (!HebrewAcademicYear.TryValidateAndCanonicalize(request.AcademicYear, out var canonicalYear))
                return BadRequest(new { message = HebrewAcademicYear.InvalidMessage });
            try
            {
                await _annualSavedReport.SaveOverridesAsync(
                    request.EmployerId,
                    canonicalYear,
                    request.Rows);
                return Ok(new { message = "השינויים נשמרו." });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "SaveAnnualComparisonOverrides rejected for employer {EmployerId}.", request.EmployerId);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SaveAnnualComparisonOverrides failed for employer {EmployerId}.", request.EmployerId);
                return StatusCode(500, new { message = "שגיאה בשמירת העריכות." });
            }
        }

        // DELETE /api/reports/annual-comparison-saved/overrides?employerId=&academicYear=&slotId=
        [HttpDelete("annual-comparison-saved/overrides")]
        [AdminWrite]
        public async Task<IActionResult> ClearAnnualComparisonOverrides(
            [FromQuery] int employerId,
            [FromQuery] string academicYear,
            [FromQuery] int? slotId)
        {
            if (employerId <= 0 || string.IsNullOrWhiteSpace(academicYear))
                return BadRequest(new { message = "employerId ו-academicYear נדרשים." });
            if (!HebrewAcademicYear.TryValidateAndCanonicalize(academicYear, out var canonicalYear))
                return BadRequest(new { message = HebrewAcademicYear.InvalidMessage });
            try
            {
                await _annualSavedReport.ClearOverridesAsync(employerId, canonicalYear, slotId);
                return Ok(new { message = slotId.HasValue ? "השורה אופסה." : "כל העריכות לשנה אופסו." });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "ClearAnnualComparisonOverrides rejected for employer {EmployerId}.", employerId);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ClearAnnualComparisonOverrides failed for employer {EmployerId}.", employerId);
                return StatusCode(500, new { message = "שגיאה באיפוס העריכות." });
            }
        }

        // GET /api/reports/annual-comparison-saved?employerId=&academicYear=
        [HttpGet("annual-comparison-saved")]
        public async Task<IActionResult> AnnualComparisonFromSavedData(
            [FromQuery] int employerId,
            [FromQuery] string academicYear)
        {
            if (employerId <= 0 || string.IsNullOrWhiteSpace(academicYear))
                return BadRequest(new { message = "employerId ו-academicYear נדרשים." });
            if (!HebrewAcademicYear.TryValidateAndCanonicalize(academicYear, out var canonicalYear))
                return BadRequest(new { message = HebrewAcademicYear.InvalidMessage });
            try
            {
                var bytes = await _reports.AnnualComparisonFromSavedDataAsync(employerId, canonicalYear);
                return File(bytes, XlsxMime, $"השוואה_שנתית_שמור_{employerId}_{DateTime.UtcNow:yyyyMMdd}.xlsx");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "AnnualComparisonFromSavedData rejected for employer {EmployerId}.", employerId);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AnnualComparisonFromSavedData report failed for employer {EmployerId}.", employerId);
                return StatusCode(500, new { message = "שגיאה בהפקת הדוח." });
            }
        }

        // GET /api/reports/institution-hours?employerId=&academicYear=&institutionSymbol=
        [HttpGet("institution-hours")]
        public async Task<IActionResult> InstitutionHours(
            [FromQuery] int employerId,
            [FromQuery] string academicYear,
            [FromQuery] string institutionSymbol)
        {
            if (employerId <= 0 || string.IsNullOrWhiteSpace(academicYear) || string.IsNullOrWhiteSpace(institutionSymbol))
                return BadRequest(new { message = "employerId, academicYear ו-institutionSymbol נדרשים." });
            if (!HebrewAcademicYear.TryValidateAndCanonicalize(academicYear, out var canonicalYear))
                return BadRequest(new { message = HebrewAcademicYear.InvalidMessage });
            try
            {
                var symbol = institutionSymbol.Trim();
                var bytes = await _reports.InstitutionHoursAsync(employerId, canonicalYear, symbol);
                var fileLabel = ReportExportService.IsInstitutionHoursAllSymbols(symbol)
                    ? "כל_הסמלים"
                    : string.Concat(symbol.Take(20).Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
                return File(bytes, XlsxMime, $"שעות_סמל_{fileLabel}_{employerId}_{DateTime.UtcNow:yyyyMMdd}.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "InstitutionHours report failed for employer {EmployerId} symbol {Symbol}.", employerId, institutionSymbol);
                return StatusCode(500, new { message = "שגיאה בהפקת הדוח." });
            }
        }

        // GET /api/reports/employees-personal?employerId=
        [HttpGet("employees-personal")]
        public async Task<IActionResult> EmployeesPersonal([FromQuery] int employerId)
        {
            if (employerId <= 0)
                return BadRequest(new { message = "employerId נדרש." });
            try
            {
                var bytes = await _reports.EmployeesPersonalAsync(employerId);
                return File(bytes, XlsxMime, $"עובדים_אישיים_{employerId}_{DateTime.UtcNow:yyyyMMdd}.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EmployeesPersonal report failed for employer {EmployerId}.", employerId);
                return StatusCode(500, new { message = "שגיאה בהפקת הדוח." });
            }
        }

        // GET /api/reports/employees-employment-data?employerId=&academicYear=
        [HttpGet("employees-employment-data")]
        public async Task<IActionResult> EmployeesEmploymentData(
            [FromQuery] int employerId,
            [FromQuery] string academicYear)
        {
            if (employerId <= 0 || string.IsNullOrWhiteSpace(academicYear))
                return BadRequest(new { message = "employerId ו-academicYear נדרשים." });
            if (!HebrewAcademicYear.TryValidateAndCanonicalize(academicYear, out var canonicalYear))
                return BadRequest(new { message = HebrewAcademicYear.InvalidMessage });
            try
            {
                var bytes = await _reports.EmployeesEmploymentDataAsync(employerId, canonicalYear);
                return File(bytes, XlsxMime, $"עובדים_נתוני_העסקה_{employerId}_{DateTime.UtcNow:yyyyMMdd}.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EmployeesEmploymentData report failed for employer {EmployerId}.", employerId);
                return StatusCode(500, new { message = "שגיאה בהפקת הדוח." });
            }
        }
    }
}
