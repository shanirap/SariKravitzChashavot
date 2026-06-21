using AccountingProject.Infrastructure;
using AccountingProject.Services;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AccountingProject.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/bulk-import")]
    public class BulkImportController : ControllerBase
    {
        private readonly IBulkImportService _bulkImportService;
        private readonly ILogger<BulkImportController> _logger;

        // Generic client-safe message when EF/database persistence fails (technical details are logged only).
        private const string DbPersistErrorUserMessageHebrew =
            "\u05D0\u05D9\u05E8\u05E2\u05D4 \u05E9\u05D2\u05D9\u05D0\u05D4 \u05D1\u05E9\u05DE\u05D9\u05E8\u05EA \u05D4\u05E0\u05EA\u05D5\u05E0\u05D9\u05DD. \u05E0\u05E1\u05D5 \u05E9\u05D5\u05D1 \u05D0\u05D5 \u05E4\u05E0\u05D5 \u05DC\u05DE\u05E0\u05D4\u05DC \u05D4\u05DE\u05E2\u05E8\u05DB\u05EA.";

        public BulkImportController(IBulkImportService bulkImportService, ILogger<BulkImportController> logger)
        {
            _bulkImportService = bulkImportService;
            _logger = logger;
        }

        // ════════════════════════════════════════════
        // POST /api/bulk-import/employees
        // ════════════════════════════════════════════
        [HttpPost("employees")]
        [AdminWrite]
        [RequestSizeLimit(ExcelUploadRules.BulkImportMaxBytes)]
        public async Task<IActionResult> ImportEmployees(IFormFile file)
        {
            if (!ExcelUploadRules.TryValidateStrictXlsx(file, ExcelUploadRules.BulkImportMaxBytes, out var err))
                return BadRequest(new { message = err });

            try
            {
                var result = await _bulkImportService.ImportEmployeesAsync(file!);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Employee bulk import rejected before or outside row loop.");
                return BadRequest(new { message = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Employee bulk import database update failed.");
                return BadRequest(new { message = DbPersistErrorUserMessageHebrew });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Employee bulk import failed unexpectedly.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "אירעה שגיאה פנימית בעת ייבוא הקובץ." });
            }
        }

        [HttpPost("employers/{employerId:int}/employees")]
        [AdminWrite]
        [RequestSizeLimit(ExcelUploadRules.BulkImportMaxBytes)]
        public async Task<IActionResult> ImportEmployeesForEmployer(int employerId, IFormFile file)
        {
            if (!ExcelUploadRules.TryValidateStrictXlsx(file, ExcelUploadRules.BulkImportMaxBytes, out var err))
                return BadRequest(new { message = err });

            try
            {
                var result = await _bulkImportService.ImportEmployeesAsync(file!, employerId);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Employee bulk import for employer {EmployerId} rejected.", employerId);
                return BadRequest(new { message = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Employee bulk import database update failed for employer {EmployerId}.", employerId);
                return BadRequest(new { message = DbPersistErrorUserMessageHebrew });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Employee bulk import failed unexpectedly for employer {EmployerId}.", employerId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "אירעה שגיאה פנימית בעת ייבוא הקובץ." });
            }
        }

        // ════════════════════════════════════════════
        // POST /api/bulk-import/employers
        // ════════════════════════════════════════════
        [HttpPost("employers")]
        [AdminWrite]
        [RequestSizeLimit(ExcelUploadRules.BulkImportMaxBytes)]
        public async Task<IActionResult> ImportEmployers(IFormFile file)
        {
            if (!ExcelUploadRules.TryValidateStrictXlsx(file, ExcelUploadRules.BulkImportMaxBytes, out var err))
                return BadRequest(new { message = err });

            try
            {
                var result = await _bulkImportService.ImportEmployersAsync(file!);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Employer bulk import rejected before or outside row loop.");
                return BadRequest(new { message = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Employer bulk import database update failed.");
                return BadRequest(new { message = DbPersistErrorUserMessageHebrew });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Employer bulk import failed unexpectedly.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "אירעה שגיאה פנימית בעת ייבוא הקובץ." });
            }
        }

        // ════════════════════════════════════════════
        // GET /api/bulk-import/template/employees
        // GET /api/bulk-import/template/employers
        // ════════════════════════════════════════════
        [HttpGet("template/employees")]
        public IActionResult TemplateEmployees([FromQuery] bool includeEmployerName = true, [FromQuery] int? employerId = null)
        {
            using var workbook = _bulkImportService.BuildEmployeesTemplate(includeEmployerName, employerId);
            return ExcelFile(workbook, "תבנית_ייבוא_עובדים.xlsx");
        }

        [HttpGet("template/employers")]
        public IActionResult TemplateEmployers()
        {
            using var workbook = _bulkImportService.BuildEmployersTemplate();
            return ExcelFile(workbook, "תבנית_ייבוא_מעסיקים.xlsx");
        }

        private IActionResult ExcelFile(XLWorkbook wb, string filename)
        {
            using var ms = new MemoryStream();
            wb.SaveAs(ms);

            // Set RTL on all sheets via OpenXml SDK — in-memory stream only (no persisted upload temp file).
            ms.Seek(0, SeekOrigin.Begin);
            using (var doc = SpreadsheetDocument.Open(ms, true, new OpenSettings { AutoSave = true }))
            {
                var workbookPart = doc.WorkbookPart;
                if (workbookPart == null)
                    return BadRequest(new { message = "לא ניתן להכין את תבנית הקובץ." });

                foreach (var wsPart in workbookPart.WorksheetParts)
                {
                    var worksheet = wsPart.Worksheet;
                    if (worksheet == null)
                        continue;

                    var views = worksheet.GetFirstChild<SheetViews>();
                    if (views == null)
                    {
                        views = new SheetViews();
                        worksheet.InsertAt(views, 0);
                    }

                    var view = views.GetFirstChild<SheetView>();
                    if (view == null)
                    {
                        view = new SheetView { WorkbookViewId = 0 };
                        views.AppendChild(view);
                    }

                    view.RightToLeft = true;
                    worksheet.Save();
                }
            }

            return File(ms.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                filename);
        }
    }
}
