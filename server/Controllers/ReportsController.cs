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
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(IReportExportService reports, ILogger<ReportsController> logger)
        {
            _reports = reports;
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
            try
            {
                var bytes = await _reports.KindergartenAnnualAsync(employerId, academicYear.Trim());
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
            try
            {
                var bytes = await _reports.SchoolAnnualAsync(employerId, academicYear.Trim());
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
        public async Task<IActionResult> MonthlyComparison(
            [FromQuery] int employerId,
            [FromQuery] string academicYear,
            [FromQuery] int month,
            IFormFile? file)
        {
            if (employerId <= 0 || string.IsNullOrWhiteSpace(academicYear) || month < 1 || month > 12)
                return BadRequest(new { message = "employerId, academicYear וחודש תקין (1–12) נדרשים." });
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "יש לצרף קובץ Excel (.xlsx) עם נתוני עוקץ." });
            try
            {
                await using var stream = file.OpenReadStream();
                var bytes = await _reports.MonthlyComparisonAsync(employerId, academicYear.Trim(), month, stream);
                return File(bytes, XlsxMime, $"השוואה_חודשית_{month}_{employerId}_{DateTime.UtcNow:yyyyMMdd}.xlsx");
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
        public async Task<IActionResult> AnnualComparison(
            [FromQuery] int employerId,
            [FromQuery] string academicYear,
            IFormFile? file)
        {
            if (employerId <= 0 || string.IsNullOrWhiteSpace(academicYear))
                return BadRequest(new { message = "employerId ו-academicYear נדרשים." });
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "יש לצרף קובץ Excel (.xlsx) עם נתוני עוקץ." });
            try
            {
                await using var stream = file.OpenReadStream();
                var bytes = await _reports.AnnualComparisonAsync(employerId, academicYear.Trim(), stream);
                return File(bytes, XlsxMime, $"השוואה_שנתית_{employerId}_{DateTime.UtcNow:yyyyMMdd}.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AnnualComparison report failed for employer {EmployerId}.", employerId);
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
            try
            {
                var bytes = await _reports.InstitutionHoursAsync(employerId, academicYear.Trim(), institutionSymbol.Trim());
                var safeSym = string.Concat(institutionSymbol.Trim().Take(20).Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
                return File(bytes, XlsxMime, $"שעות_סמל_{safeSym}_{employerId}_{DateTime.UtcNow:yyyyMMdd}.xlsx");
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
            try
            {
                var bytes = await _reports.EmployeesEmploymentDataAsync(employerId, academicYear.Trim());
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
