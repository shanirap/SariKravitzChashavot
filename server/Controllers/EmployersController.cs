using AccountingProject.Contracts;
using AccountingProject.Infrastructure;
using AccountingProject.Models;
using AccountingProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccountingProject.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class EmployersController : ControllerBase
    {
        private readonly IEmployerService _employerService;
        private readonly IEmployeeService _employeeService;
        private readonly IComparisonReportService _comparisonReportService;
        private readonly ILogger<EmployersController> _logger;

        public EmployersController(
            IEmployerService employerService,
            IEmployeeService employeeService,
            IComparisonReportService comparisonReportService,
            ILogger<EmployersController> logger)
        {
            _employerService = employerService;
            _employeeService = employeeService;
            _comparisonReportService = comparisonReportService;
            _logger = logger;
        }

        /// <summary>Upload monthly payroll Excel; returns comparison workbook (V per match; yellow empty cell per mismatch).</summary>
        [HttpPost("{id}/comparison/monthly-payroll")]
        [RequestSizeLimit(ExcelUploadRules.ComparisonMonthlyPayrollMaxBytes)]
        public async Task<IActionResult> CompareMonthlyPayroll(int id, IFormFile file, CancellationToken cancellationToken)
        {
            var employer = await _employerService.GetByIdAsync(id);
            if (employer == null) return NotFound();

            if (!ExcelUploadRules.TryValidateStrictXlsx(file, ExcelUploadRules.ComparisonMonthlyPayrollMaxBytes, out var err))
                return BadRequest(new { message = err });

            try
            {
                await using var stream = file!.OpenReadStream();
                var bytes = await _comparisonReportService.GenerateMonthlyPayrollComparisonExcelAsync(id, stream, cancellationToken);
                var fn = $"comparison_{id}_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";
                return File(bytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileDownloadName: fn);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Monthly payroll comparison rejected for employer {EmployerId}.", id);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Monthly payroll comparison failed for employer {EmployerId}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "אירעה שגיאה פנימית בעת השוואת קובץ השכר." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            var result = await _employerService.GetPagedAsync(search, page, pageSize);
            return Ok(new
            {
                items = result.Items.Select(MapEmployer),
                result.TotalCount,
                result.Page,
                result.PageSize
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var employer = await _employerService.GetByIdAsync(id);
            if (employer == null) return NotFound();
            return Ok(MapEmployer(employer));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] EmployerDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest(new { message = "שם המעסיק הוא שדה חובה." });

            try
            {
                var employer = await _employerService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = employer.Id }, MapEmployer(employer));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] EmployerDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest(new { message = "שם המעסיק הוא שדה חובה." });

            try
            {
                var updated = await _employerService.UpdateAsync(id, dto);
                if (!updated) return NotFound();
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _employerService.DeleteAsync(id);
            if (!result.Success && result.Message == null) return NotFound();
            if (!result.Success) return Conflict(new { message = result.Message });
            return NoContent();
        }

        [HttpGet("{id}/export/excel")]
        public async Task<IActionResult> ExportFullExcel(int id)
        {
            var employer = await _employerService.GetByIdAsync(id);
            if (employer == null) return NotFound();

            var bytes = await _employerService.BuildFullEmployerExportExcelAsync(id);
            if (bytes == null || bytes.Length == 0) return NotFound();

            var safeBase = string.Concat(employer.Name.Split(Path.GetInvalidFileNameChars()));
            if (string.IsNullOrWhiteSpace(safeBase)) safeBase = $"employer_{id}";
            safeBase = safeBase.Trim();
            if (safeBase.Length > 80) safeBase = safeBase[..80];
            var fileName = $"{safeBase}_{id}_{DateTime.UtcNow:yyyyMMdd}.xlsx";

            return File(
                bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileDownloadName: fileName);
        }

        [HttpGet("{id}/employees")]
        public async Task<IActionResult> GetEmployees(int id,
            [FromQuery] string? search = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            var employer = await _employerService.GetByIdAsync(id);
            if (employer == null) return NotFound();

            var result = await _employerService.GetEmployeesAsync(id, search, page, pageSize);
            var employeeIds = result.Items.Select(e => e.Id).ToList();
            var withEmploymentData = await _employerService.GetEmployeeIdsWithEmploymentDataAsync(id, employeeIds);

            return Ok(new
            {
                items = result.Items.Select(e => MapEmployee(e, withEmploymentData.Contains(e.Id))),
                result.TotalCount,
                result.Page,
                result.PageSize
            });
        }

        /// <summary>Resolve one non-deleted employee by employer and national id.</summary>
        [HttpGet("{id}/employees/by-id-number/{idNumber}")]
        public async Task<IActionResult> GetEmployeeByIdNumber(int id, string idNumber)
        {
            var employer = await _employerService.GetByIdAsync(id);
            if (employer == null) return NotFound();

            var emp = await _employeeService.GetByEmployerAndIdNumberAsync(id, idNumber);
            if (emp == null) return NotFound();

            var withEmploymentData = await _employerService.GetEmployeeIdsWithEmploymentDataAsync(id, new List<int> { emp.Id });
            return Ok(MapEmployee(emp, withEmploymentData.Contains(emp.Id)));
        }

        [HttpGet("{id}/institution-symbols")]
        public async Task<IActionResult> GetInstitutionSymbols(int id)
        {
            var employer = await _employerService.GetByIdAsync(id);
            if (employer == null) return NotFound();

            var symbols = await _employerService.GetInstitutionSymbolsAsync(id);
            return Ok(symbols.Select(s => new
            {
                s.Id,
                s.EmployerId,
                s.InstitutionSymbol,
                s.InstitutionSymbolName
            }));
        }

        [HttpPost("{id}/institution-symbols")]
        public async Task<IActionResult> CreateInstitutionSymbol(int id, [FromBody] EmployerInstitutionSymbolDto dto)
        {
            var (symbol, message) = await _employerService.CreateInstitutionSymbolAsync(id, dto);
            if (symbol == null && message == null) return NotFound();
            if (symbol == null) return BadRequest(new { message });

            return Ok(new
            {
                symbol.Id,
                symbol.EmployerId,
                symbol.InstitutionSymbol,
                symbol.InstitutionSymbolName
            });
        }

        [HttpDelete("{id}/institution-symbols/{symbolId}")]
        public async Task<IActionResult> DeleteInstitutionSymbol(int id, int symbolId)
        {
            var result = await _employerService.DeleteInstitutionSymbolAsync(id, symbolId);
            if (!result.Success && result.Message == null) return NotFound();
            if (!result.Success) return Conflict(new { message = result.Message });
            return NoContent();
        }

        private static object MapEmployer(Employer employer) => new
        {
            employer.Id,
            employer.Name,
            employer.BusinessNumber,
            employer.BeneficiarySymbol,
            employer.EketzNumber
        };

        private static object MapEmployee(Employee employee, bool hasEmploymentData) => new
        {
            employee.Id,
            employee.EmployerId,
            employee.IdNumber,
            employee.FirstName,
            employee.LastName,
            employee.EmployeeNumber,
            BirthDate = employee.BirthDate.HasValue ? employee.BirthDate.Value.ToString("yyyy-MM-dd") : null,
            employee.Gender,
            FullName = employee.FullName,
            hasEmploymentData,
            isActive = employee.ManualActiveStatus ?? hasEmploymentData,
            employee.ManualActiveStatus
        };
    }
}
