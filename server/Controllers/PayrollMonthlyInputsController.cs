using AccountingProject.Contracts;
using AccountingProject.Infrastructure;
using AccountingProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccountingProject.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/payroll-monthly-inputs")]
    public class PayrollMonthlyInputsController : ControllerBase
    {
        private readonly IPayrollMonthlyInputService _payrollMonthlyInputService;

        public PayrollMonthlyInputsController(IPayrollMonthlyInputService payrollMonthlyInputService) =>
            _payrollMonthlyInputService = payrollMonthlyInputService;

        // POST /api/payroll-monthly-inputs/import?employerId=&academicYear=&month=
        // Body: multipart/form-data with "file" (.xlsx)
        [HttpPost("import")]
        [RequestSizeLimit(ExcelUploadRules.ComparisonMonthlyPayrollMaxBytes)]
        public async Task<IActionResult> ImportMonth(
            [FromQuery] int employerId,
            [FromQuery] string academicYear,
            [FromQuery] int month,
            IFormFile? file)
        {
            if (employerId <= 0 || string.IsNullOrWhiteSpace(academicYear) || month is < 1 or > 12)
                return BadRequest(new { message = "employerId, academicYear וחודש תקין (1–12) נדרשים." });
            if (!ExcelUploadRules.TryValidateStrictXlsx(file, ExcelUploadRules.ComparisonMonthlyPayrollMaxBytes, out var uploadError))
                return BadRequest(new { message = uploadError });

            try
            {
                await using var stream = file!.OpenReadStream();
                var result = await _payrollMonthlyInputService.ImportMonthAsync(
                    employerId,
                    academicYear.Trim(),
                    month,
                    stream,
                    file.FileName);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetStatus(
            [FromQuery] int employerId,
            [FromQuery] string academicYear)
        {
            if (employerId <= 0 || string.IsNullOrWhiteSpace(academicYear))
                return BadRequest(new { message = "employerId ו-academicYear נדרשים." });

            try
            {
                var status = await _payrollMonthlyInputService.GetYearStatusAsync(
                    employerId,
                    academicYear.Trim());
                return Ok(status);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("rows")]
        public async Task<IActionResult> GetRows(
            [FromQuery] int employerId,
            [FromQuery] string academicYear,
            [FromQuery] int month)
        {
            if (employerId <= 0 || string.IsNullOrWhiteSpace(academicYear) || month is < 1 or > 12)
                return BadRequest(new { message = "employerId, academicYear וחודש תקין (1–12) נדרשים." });

            try
            {
                var rows = await _payrollMonthlyInputService.GetRowsAsync(
                    employerId,
                    academicYear.Trim(),
                    month);
                return Ok(rows);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("rows/{id}")]
        public async Task<IActionResult> UpdateRow(int id, [FromBody] PayrollMonthlyInputRowEditDto dto)
        {
            try
            {
                var row = await _payrollMonthlyInputService.UpdateRowAsync(id, dto);
                return Ok(row);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpDelete("rows/{id}")]
        public async Task<IActionResult> DeleteRow(int id)
        {
            try
            {
                await _payrollMonthlyInputService.DeleteRowAsync(id);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
