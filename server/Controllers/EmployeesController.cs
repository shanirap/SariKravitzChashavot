using AccountingProject.Contracts;
using AccountingProject.Infrastructure;
using AccountingProject.Models;
using AccountingProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace AccountingProject.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public EmployeesController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        /// <summary>Preview: active vs soft-deleted row for same EmployerId + trimmed IdNumber (matches create/restore logic).</summary>
        [HttpGet("precreate-hint")]
        public async Task<IActionResult> GetPrecreateHint([FromQuery] int employerId, [FromQuery] string? idNumber, CancellationToken cancellationToken)
        {
            var hint = await _employeeService.GetPrecreateHintAsync(employerId, idNumber ?? string.Empty, cancellationToken);
            return Ok(hint);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var emp = await _employeeService.GetByIdAsync(id);
            if (emp == null) return NotFound();
            return Ok(MapEmployee(emp, false));
        }

        // Historical route: lookup by national id only is ambiguous when the same IdNumber exists under multiple employers.
        [HttpGet("by-id-number/{idNumber}")]
        public IActionResult GetByIdNumberDeprecated(string idNumber)
        {
            return BadRequest(new
            {
                message =
                    "לא ניתן לחפש עובד לפי מספר תעודת זהות בלבד. יש לציין את מזהה המעסיק: השתמשו בבקשת GET לכתובת המעסיק עם נתיב employees/by-id-number."
            });
        }

        [HttpPost]
        [AdminWrite]
        public async Task<IActionResult> Create([FromBody] EmployeeDto dto)
        {
            var validationMessage = ValidateRequiredEmployeeFields(dto);
            if (validationMessage != null)
                return BadRequest(new { message = validationMessage });

            if (dto.EmployerId <= 0)
                return BadRequest(new { message = "מזהה מעסיק הוא שדה חובה." });

            try
            {
                var result = await _employeeService.CreateOrGetAsync(dto);
                var payload = MapEmployee(result.Employee, result.RestoredFromSoftDelete);
                if (result.CreatedNew)
                    return CreatedAtAction(nameof(GetById), new { id = result.Employee.Id }, payload);

                return Ok(payload);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                if (EmployeeService.IsDuplicateEmployeeEmployerTzConstraint(ex))
                    return BadRequest(new { message = "קיים עובד עם תעודת זהות זהה עבור מעסיק זה." });
                throw;
            }
        }

        [HttpDelete("{id}")]
        [AdminWrite]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _employeeService.DeleteAsync(id);
            if (!result.Success && result.Message == null) return NotFound();
            if (!result.Success) return Conflict(new { message = result.Message });
            return NoContent();
        }

        [HttpPut("{id}")]
        [AdminWrite]
        public async Task<IActionResult> Update(int id, [FromBody] EmployeeDto dto)
        {
            var validationMessage = ValidateRequiredEmployeeFields(dto);
            if (validationMessage != null)
                return BadRequest(new { message = validationMessage });

            if (dto.EmployerId <= 0)
                return BadRequest(new { message = "מזהה מעסיק הוא שדה חובה." });

            try
            {
                var updated = await _employeeService.UpdateAsync(id, dto);
                if (!updated) return NotFound();
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                if (EmployeeService.IsDuplicateEmployeeEmployerTzConstraint(ex))
                    return BadRequest(new { message = "קיים עובד עם תעודת זהות זהה עבור מעסיק זה." });
                throw;
            }
        }

        public class UpdateEmployeeStatusDto
        {
            public bool IsActive { get; set; }
        }

        [HttpPatch("{id}/active-status")]
        [AdminWrite]
        public async Task<IActionResult> UpdateActiveStatus(int id, [FromBody] UpdateEmployeeStatusDto dto)
        {
            try
            {
                var updated = await _employeeService.SetManualActiveStatusAsync(id, dto.IsActive);
                if (!updated) return NotFound();
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        private static object MapEmployee(Employee emp, bool restoredFromSoftDelete = false) => new
        {
            emp.Id, emp.EmployerId, emp.IdNumber, emp.FirstName, emp.LastName,
            emp.EmployeeNumber, emp.Gender, emp.Phone,
            BirthDate        = Fmt(emp.BirthDate),
            ChildBirthDate1  = Fmt(emp.ChildBirthDate1),
            ChildBirthDate2  = Fmt(emp.ChildBirthDate2),
            ChildBirthDate3  = Fmt(emp.ChildBirthDate3),
            ChildBirthDate4  = Fmt(emp.ChildBirthDate4),
            ChildBirthDate5  = Fmt(emp.ChildBirthDate5),
            ChildBirthDate6  = Fmt(emp.ChildBirthDate6),
            ChildBirthDate7  = Fmt(emp.ChildBirthDate7),
            ChildBirthDate8  = Fmt(emp.ChildBirthDate8),
            ChildBirthDate9  = Fmt(emp.ChildBirthDate9),
            ChildBirthDate10 = Fmt(emp.ChildBirthDate10),
            FullName = emp.FullName,
            emp.ManualActiveStatus,
            restoredFromSoftDelete
        };

        private static string? ValidateRequiredEmployeeFields(EmployeeDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.LastName))
                return "משפחה היא שדה חובה.";
            if (string.IsNullOrWhiteSpace(dto.FirstName))
                return "פרטי הוא שדה חובה.";
            if (string.IsNullOrWhiteSpace(dto.IdNumber))
                return "תעודת זהות היא שדה חובה.";
            if (string.IsNullOrWhiteSpace(dto.Gender))
                return "מין הוא שדה חובה.";
            if (string.IsNullOrWhiteSpace(dto.BirthDate))
                return "תאריך לידה הוא שדה חובה.";
            if (!DateOnly.TryParseExact(dto.BirthDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                return "תאריך לידה אינו תקין.";

            return null;
        }

        private static string? Fmt(DateOnly? d) => d.HasValue ? d.Value.ToString("yyyy-MM-dd") : null;
    }
}
