using System.Globalization;
using AccountingProject.Contracts;
using AccountingProject.Domain;

namespace AccountingProject.Services
{
    public class EmploymentCalculationService : IEmploymentCalculationService
    {
        private const decimal OneThirdJobPercent = 100m / 3m;

        public void PrepareForSave(
            EmploymentDataDto dto,
            DateOnly? employeeBirthDate,
            bool isFemaleEmployee,
            IReadOnlyList<DateOnly?> childBirthDates)
        {
            TeacherSupplementarySlotSync.Sync(dto);
            ApplyDefaultJobBases(dto);
            ApplyDefaultAgeHours(dto, employeeBirthDate);
            RecalculateDerivedValues(dto, isFemaleEmployee, childBirthDates);
        }

        public void ApplyDefaultJobBases(EmploymentDataDto dto)
        {
            ApplyDefaultJobBasesForBand(dto, 1, dto.Grade1GradeName, dto.Grade1Role);
            ApplyDefaultJobBasesForBand(dto, 2, dto.Grade2GradeName, dto.Grade2Role);
        }

        public void RecalculateDerivedValues(
            EmploymentDataDto dto,
            bool isFemaleEmployee,
            IReadOnlyList<DateOnly?> childBirthDates)
        {
            var slots = dto.Slots ?? [];
            dto.Grade1Total = SumWeeklyHours(slots.Where(s => s.GradeBand == 1));
            dto.Grade2Total = SumWeeklyHours(slots.Where(s => s.GradeBand == 2));

            var refDate = HebrewAcademicYear.GetSchoolYearStartDate(dto.AcademicYear!);
            dto.Grade1MotherBenefitPercent = ComputeMotherBenefitPercent(dto, 1, isFemaleEmployee, childBirthDates, refDate);
            dto.Grade2MotherBenefitPercent = ComputeMotherBenefitPercent(dto, 2, isFemaleEmployee, childBirthDates, refDate);

            dto.Grade1JobPercent = ComputeJobPercent(dto, 1);
            dto.Grade2JobPercent = ComputeJobPercent(dto, 2);

            dto.Grade1TrainingFundPercent = ComputeTrainingFundPercent(dto, 1);
            dto.Grade2TrainingFundPercent = ComputeTrainingFundPercent(dto, 2);
        }

        private static void ApplyDefaultAgeHours(EmploymentDataDto dto, DateOnly? employeeBirthDate)
        {
            if (string.IsNullOrWhiteSpace(dto.AcademicYear))
                return;

            var computed = EmploymentAgeHoursDefaults.Compute(
                employeeBirthDate,
                HebrewAcademicYear.GetSchoolYearStartDate(dto.AcademicYear));
            if (!computed.HasValue)
                return;

            if (!dto.Grade1AgeHours.HasValue)
                dto.Grade1AgeHours = computed;
            if (!dto.Grade2AgeHours.HasValue)
                dto.Grade2AgeHours = computed;
        }

        private static void ApplyDefaultJobBasesForBand(
            EmploymentDataDto dto,
            int band,
            string? gradeName,
            string? role)
        {
            var defaultBase = EmploymentJobBaseDefaults.GetJobBaseValue(gradeName, role);
            if (!defaultBase.HasValue)
                return;

            dto.Slots ??= [];
            foreach (var slot in dto.Slots.Where(s => s.GradeBand == band))
            {
                if (!slot.JobBase.HasValue)
                    slot.JobBase = defaultBase;
            }
        }

        private static decimal? SumWeeklyHours(IEnumerable<EmploymentDataSlotDto> rows)
        {
            var vals = rows
                .Select(r => r.WeeklyHours)
                .Where(v => v.HasValue)
                .Select(v => v!.Value)
                .ToList();
            if (vals.Count == 0) return null;
            return Math.Round(vals.Sum(), 2);
        }

        private static decimal? ComputeEquivalentJobBase(IEnumerable<EmploymentDataSlotDto> rows, decimal? ageHours)
        {
            decimal sumW = 0m;
            decimal sumWOverBase = 0m;
            foreach (var row in rows)
            {
                var w = row.WeeklyHours;
                var b = EmploymentJobBaseAdjustments.NetJobBaseAfterAgeHours(row.JobBase, ageHours);
                if (!w.HasValue || !b.HasValue || w.Value <= 0 || b.Value <= 0) continue;
                sumW += w.Value;
                sumWOverBase += w.Value / b.Value;
            }
            if (sumW <= 0 || sumWOverBase <= 0) return null;
            return sumW / sumWOverBase;
        }

        private static decimal? ComputeBaseJobPercent(EmploymentDataDto dto, int band)
        {
            var total = band == 1 ? dto.Grade1Total : dto.Grade2Total;
            var ageHours = band == 1 ? dto.Grade1AgeHours : dto.Grade2AgeHours;
            var rows = (dto.Slots ?? []).Where(s => s.GradeBand == band);
            var equiv = ComputeEquivalentJobBase(rows, ageHours);
            if (!total.HasValue || !equiv.HasValue || equiv.Value <= 0) return null;
            return Math.Round((total.Value / equiv.Value) * 100m, 2);
        }

        private static decimal? ComputeMotherBenefitPercent(
            EmploymentDataDto dto,
            int band,
            bool isFemaleEmployee,
            IReadOnlyList<DateOnly?> childBirthDates,
            DateOnly refDate)
        {
            var gradeName = band == 1 ? dto.Grade1GradeName : dto.Grade2GradeName;
            var basePct = ComputeBaseJobPercent(dto, band);
            return MotherBenefitRules.ComputePercent(
                gradeName,
                isFemaleEmployee,
                childBirthDates,
                refDate,
                basePct);
        }

        private static decimal? ComputeJobPercent(EmploymentDataDto dto, int band)
        {
            var total = band == 1 ? dto.Grade1Total : dto.Grade2Total;
            var mother = band == 1 ? dto.Grade1MotherBenefitPercent : dto.Grade2MotherBenefitPercent;
            var ageHours = band == 1 ? dto.Grade1AgeHours : dto.Grade2AgeHours;
            var rows = (dto.Slots ?? []).Where(s => s.GradeBand == band);
            var equiv = ComputeEquivalentJobBase(rows, ageHours);
            if (!total.HasValue || !equiv.HasValue || equiv.Value <= 0) return null;
            var mom = mother ?? 0m;
            return Math.Round((total.Value / equiv.Value) * 100m + mom, 2);
        }

        private static decimal? ComputeTrainingFundPercent(EmploymentDataDto dto, int band)
        {
            var gradeName = (band == 1 ? dto.Grade1GradeName : dto.Grade2GradeName)?.Trim();
            if (string.IsNullOrWhiteSpace(gradeName)) return null;
            var jobPct = band == 1 ? dto.Grade1JobPercent : dto.Grade2JobPercent;
            if (!jobPct.HasValue) return null;
            if (jobPct.Value < OneThirdJobPercent) return 0m;

            if (GradeOptions.NormalizeGradeName(gradeName) == GradeOptions.UnifiedEducationSupportGradeName)
            {
                var seniority = (band == 1 ? dto.Grade1Seniority : dto.Grade2Seniority)?.Trim();
                if (decimal.TryParse(seniority, NumberStyles.Any, CultureInfo.InvariantCulture, out var years) && years >= 2m)
                    return 7.5m;
                return 0m;
            }

            return gradeName switch
            {
                "יסודי וגנים" or "עוז לתמורה" or "אופק חדש" or "אופק גנים" => 8.4m,
                _ => null
            };
        }
    }
}
