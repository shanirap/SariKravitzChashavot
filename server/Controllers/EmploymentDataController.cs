using AccountingProject.Contracts;
using AccountingProject.Models;
using AccountingProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccountingProject.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/employment-data")]
    public class EmploymentDataController : ControllerBase
    {
        private readonly IEmploymentDataService _employmentDataService;
        public EmploymentDataController(IEmploymentDataService employmentDataService) => _employmentDataService = employmentDataService;

        [HttpGet("employee/{employeeId}/employer/{employerId}")]
        public async Task<IActionResult> GetByEmployeeAndEmployer(int employeeId, int employerId)
        {
            var records = await _employmentDataService.GetByEmployeeAndEmployerAsync(employeeId, employerId);
            return Ok(records.Select(Map).ToList());
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] EmploymentDataDto dto)
        {
            var (record, message) = await _employmentDataService.CreateAsync(dto);
            if (record == null) return BadRequest(new { message });
            return CreatedAtAction(nameof(GetByEmployeeAndEmployer),
                new { employeeId = record.EmployeeId, employerId = record.EmployerId }, Map(record));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] EmploymentDataDto dto)
        {
            var (record, message) = await _employmentDataService.UpdateAsync(id, dto);
            if (record == null && message == null) return NotFound();
            if (record == null) return BadRequest(new { message });
            return Ok(Map(record));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _employmentDataService.DeleteAsync(id);
            if (!result.Success && result.Message == null) return NotFound();
            return NoContent();
        }

        private static object Map(EmploymentData r) => new
        {
            r.Id,
            r.EmployeeId,
            r.EmployerId,
            r.AcademicYear,
            r.PeriodDisplay,
            grade1Total = r.Grade1Total,
            grade1JobPercent = r.Grade1JobPercent,
            grade1TrainingFundPercent = r.Grade1TrainingFundPercent,
            grade1AgeHours = r.Grade1AgeHours,
            grade1MotherBenefitPercent = r.Grade1MotherBenefitPercent,
            grade1TrainingBenefits = r.Grade1TrainingBenefits,
            grade1DoubleDegree = r.Grade1DoubleDegree,
            grade2Total = r.Grade2Total,
            grade2JobPercent = r.Grade2JobPercent,
            grade2TrainingFundPercent = r.Grade2TrainingFundPercent,
            grade2AgeHours = r.Grade2AgeHours,
            grade2MotherBenefitPercent = r.Grade2MotherBenefitPercent,
            grade2TrainingBenefits = r.Grade2TrainingBenefits,
            grade2DoubleDegree = r.Grade2DoubleDegree,
            grade1GradeName = r.Grade1GradeName,
            grade1Grade = r.Grade1Grade,
            grade1Role = r.Grade1Role,
            grade1Seniority = r.Grade1Seniority,
            grade2GradeName = r.Grade2GradeName,
            grade2Grade = r.Grade2Grade,
            grade2Role = r.Grade2Role,
            grade2Seniority = r.Grade2Seniority,
            slots = r.Slots
                .OrderBy(s => s.GradeBand)
                .ThenBy(s => s.SlotIndex)
                .Select(s => new
                {
                    s.Id,
                    s.EmploymentDataId,
                    gradeBand = s.GradeBand,
                    slotIndex = s.SlotIndex,
                    s.InstitutionSymbol,
                    s.WeeklyHours,
                    s.JobBase,
                    supplementaryParentSlotIndex = s.SupplementaryParentSlotIndex
                })
        };
    }
}
