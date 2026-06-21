using AccountingProject.Contracts;
using AccountingProject.Data;
using AccountingProject.Domain;
using AccountingProject.Models;
using Microsoft.EntityFrameworkCore;

namespace AccountingProject.Services
{
    public class EmploymentDataService : IEmploymentDataService
    {
        private readonly PayrollDbContext _db;
        private readonly IEmploymentCalculationService _calculations;

        public EmploymentDataService(PayrollDbContext db, IEmploymentCalculationService calculations)
        {
            _db = db;
            _calculations = calculations;
        }

        public async Task<IReadOnlyList<EmploymentData>> GetByEmployeeAndEmployerAsync(int employeeId, int employerId)
        {
            return await _db.EmploymentData
                .AsNoTracking()
                .Include(e => e.Slots)
                .Where(ed => ed.EmployeeId == employeeId && ed.EmployerId == employerId)
                .OrderByDescending(ed => ed.AcademicYear)
                .ToListAsync();
        }

        public async Task<(EmploymentData? Record, string? Message)> CreateAsync(EmploymentDataDto dto)
        {
            var validationError = await ValidateAsync(dto, null);
            if (validationError != null) return (null, validationError);

            var record = new EmploymentData();
            ApplyHeader(record, dto);
            ApplySlots(record, dto.Slots ?? []);
            _db.EmploymentData.Add(record);
            await _db.SaveChangesAsync();
            await _db.Entry(record).Collection(r => r.Slots).LoadAsync();
            return (record, null);
        }

        public async Task<(EmploymentData? Record, string? Message)> UpdateAsync(int id, EmploymentDataDto dto)
        {
            var record = await _db.EmploymentData
                .Include(r => r.Slots)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (record == null) return (null, null);

            var validationError = await ValidateAsync(dto, record.Id);
            if (validationError != null) return (null, validationError);

            ApplyHeader(record, dto);
            _db.EmploymentDataSlots.RemoveRange(record.Slots);
            record.Slots.Clear();
            ApplySlots(record, dto.Slots ?? []);
            await _db.SaveChangesAsync();
            await _db.Entry(record).Collection(r => r.Slots).LoadAsync();
            return (record, null);
        }

        public async Task<(bool Success, string? Message)> DeleteAsync(int id)
        {
            var record = await _db.EmploymentData.FirstOrDefaultAsync(r => r.Id == id);
            if (record == null) return (false, null);

            _db.EmploymentData.Remove(record);
            await _db.SaveChangesAsync();
            return (true, null);
        }

        private async Task<string?> ValidateAsync(EmploymentDataDto dto, int? excludeId)
        {
            if (dto.Slots == null)
                return "מקטעי העסקה חסרים.";

            if (!HebrewAcademicYear.TryValidateAndCanonicalize(dto.AcademicYear, out var academicYear))
                return HebrewAcademicYear.InvalidMessage;
            dto.AcademicYear = academicYear;
            dto.Grade1GradeName = GradeOptions.NormalizeGradeName(dto.Grade1GradeName);
            dto.Grade2GradeName = GradeOptions.NormalizeGradeName(dto.Grade2GradeName);

            var employeeRow = await _db.Employees
                .AsNoTracking()
                .Where(e => e.Id == dto.EmployeeId)
                .Select(e => new
                {
                    e.EmployerId,
                    e.Gender,
                    e.BirthDate,
                    e.ChildBirthDate1,
                    e.ChildBirthDate2,
                    e.ChildBirthDate3,
                    e.ChildBirthDate4,
                    e.ChildBirthDate5,
                    e.ChildBirthDate6,
                    e.ChildBirthDate7,
                    e.ChildBirthDate8,
                    e.ChildBirthDate9,
                    e.ChildBirthDate10,
                })
                .FirstOrDefaultAsync();
            if (employeeRow == null)
                return "העובד לא נמצא במערכת.";
            // Employment must use the Employee row for the same employer (same person can have multiple Employee rows).
            if (employeeRow.EmployerId != dto.EmployerId)
                return "מזהה העובד שייך למעסיק אחר — יש להשתמש ברשומת העובד הרשומה תחת אותו מעסיק כמו בנתוני ההעסקה.";

            var employeeGender = employeeRow.Gender;
            if (employeeGender == null)
                return "העובד לא נמצא במערכת.";

            if (!await _db.Employers.AnyAsync(e => e.Id == dto.EmployerId))
                return "המעסיק לא נמצא במערכת.";

            var dup = await _db.EmploymentData.AnyAsync(ed =>
                ed.EmployeeId == dto.EmployeeId
                && ed.EmployerId == dto.EmployerId
                && ed.AcademicYear == dto.AcademicYear
                && (!excludeId.HasValue || ed.Id != excludeId.Value));
            if (dup)
                return "כבר קיימת רשומה לעובד זה, מעסיק זה ושנת הלימודים.";

            var allowedInstitutionSymbols = (await _db.EmployerInstitutionSymbols
                    .AsNoTracking()
                    .Where(s => s.EmployerId == dto.EmployerId)
                    .Select(s => s.InstitutionSymbol)
                    .ToListAsync())
                .ToHashSet(StringComparer.Ordinal);

            var band1Err = ValidateGradeBand(1, dto.Grade1GradeName, dto.Grade1Grade, dto.Grade1Role, dto.Grade1Seniority);
            if (band1Err != null) return band1Err;
            var band2Err = ValidateGradeBand(2, dto.Grade2GradeName, dto.Grade2Grade, dto.Grade2Role, dto.Grade2Seniority);
            if (band2Err != null) return band2Err;

            foreach (var s in dto.Slots)
            {
                if (s.GradeBand is < 1 or > 2)
                    return "רמת דרגה חייבת להיות 1 או 2.";
                if (s.SlotIndex is < 1 or > 6)
                    return "מספר מקטע חייב להיות בין 1 ל-6.";
                if (!string.IsNullOrWhiteSpace(s.InstitutionSymbol)
                    && !allowedInstitutionSymbols.Contains(s.InstitutionSymbol.Trim()))
                    return $"סמל המוסד במקטע דרגה{s.GradeBand}_{s.SlotIndex} אינו שייך למעסיק.";
            }

            var keys = dto.Slots.Select(s => (s.GradeBand, s.SlotIndex)).ToList();
            if (keys.Count != keys.Distinct().Count())
                return "כפילות במקטעים (דרגה+אינדקס).";

            var slotLookup = dto.Slots.ToDictionary(x => (x.GradeBand, x.SlotIndex));
            foreach (var s in dto.Slots)
            {
                if (s.SupplementaryParentSlotIndex == null)
                    continue;

                var gradeName = s.GradeBand == 1 ? dto.Grade1GradeName : dto.Grade2GradeName;
                var role = s.GradeBand == 1 ? dto.Grade1Role : dto.Grade2Role;
                if (!TeacherSupplementarySlotRules.Qualifies(gradeName, role))
                    return $"שורת שעות נוספות במקטע דרגה{s.GradeBand}_{s.SlotIndex} אינה מותרת לשילוב שם הדירוג/תפקיד.";

                var spi = s.SupplementaryParentSlotIndex.Value;
                if (spi is < 1 or > 5)
                    return "מקטע הורה לשעות נוספות חייב להיות בין 1 ל-5.";
                if (s.SlotIndex != spi + 1)
                    return $"מקטע דרגה{s.GradeBand}_{s.SlotIndex} אינו ממוקם מתחת למקטע ההורה.";

                if (s.WeeklyHours is null || Math.Abs((decimal)s.WeeklyHours.Value - 3m) > 0.01m)
                    return $"שורת שעות נוספות במקטע דרגה{s.GradeBand}_{s.SlotIndex} חייבת להכיל 3 שעות שבועיות.";

                if (!slotLookup.TryGetValue((s.GradeBand, spi), out var parent))
                    return $"חסר מקטע הורה {spi} לשעות נוספות במקטע דרגה{s.GradeBand}_{s.SlotIndex}.";

                var symChild = s.InstitutionSymbol?.Trim() ?? string.Empty;
                var symParent = parent.InstitutionSymbol?.Trim() ?? string.Empty;
                if (symChild.Length == 0 || !string.Equals(symChild, symParent, StringComparison.Ordinal))
                    return $"סמל המוסד בשורת השעות הנוספות דרגה{s.GradeBand}_{s.SlotIndex} חייב להתאים למקטע ההורה.";
            }

            var childBirthDates = new DateOnly?[]
            {
                employeeRow.ChildBirthDate1,
                employeeRow.ChildBirthDate2,
                employeeRow.ChildBirthDate3,
                employeeRow.ChildBirthDate4,
                employeeRow.ChildBirthDate5,
                employeeRow.ChildBirthDate6,
                employeeRow.ChildBirthDate7,
                employeeRow.ChildBirthDate8,
                employeeRow.ChildBirthDate9,
                employeeRow.ChildBirthDate10,
            };
            _calculations.PrepareForSave(
                dto,
                employeeRow.BirthDate,
                IsFemaleGender(employeeGender),
                childBirthDates);
            return null;
        }

        private static string? ValidateGradeBand(int band, string? gradeName, string? grade, string? role, string? seniority) =>
            GradeOptions.GetGradeBandValidationError(band, gradeName, grade, role, seniority);

        private static void ApplyHeader(EmploymentData r, EmploymentDataDto d)
        {
            r.EmployeeId = d.EmployeeId;
            r.EmployerId = d.EmployerId;
            r.AcademicYear = d.AcademicYear ?? string.Empty;
            r.Grade1Total = d.Grade1Total;
            r.Grade1JobPercent = d.Grade1JobPercent;
            r.Grade1TrainingFundPercent = d.Grade1TrainingFundPercent;
            r.Grade1AgeHours = d.Grade1AgeHours;
            r.Grade1MotherBenefitPercent = d.Grade1MotherBenefitPercent;
            r.Grade1TrainingBenefits = d.Grade1TrainingBenefits;
            r.Grade1DoubleDegree = d.Grade1DoubleDegree;
            r.Grade2Total = d.Grade2Total;
            r.Grade2JobPercent = d.Grade2JobPercent;
            r.Grade2TrainingFundPercent = d.Grade2TrainingFundPercent;
            r.Grade2AgeHours = d.Grade2AgeHours;
            r.Grade2MotherBenefitPercent = d.Grade2MotherBenefitPercent;
            r.Grade2TrainingBenefits = d.Grade2TrainingBenefits;
            r.Grade2DoubleDegree = d.Grade2DoubleDegree;
            r.Grade1GradeName = string.IsNullOrWhiteSpace(d.Grade1GradeName) ? null : d.Grade1GradeName.Trim();
            r.Grade1Grade = string.IsNullOrWhiteSpace(d.Grade1Grade) ? null : d.Grade1Grade.Trim();
            r.Grade1Role = string.IsNullOrWhiteSpace(d.Grade1Role) ? null : d.Grade1Role.Trim();
            r.Grade1Seniority = string.IsNullOrWhiteSpace(d.Grade1Seniority) ? null : d.Grade1Seniority.Trim();
            r.Grade2GradeName = string.IsNullOrWhiteSpace(d.Grade2GradeName) ? null : d.Grade2GradeName.Trim();
            r.Grade2Grade = string.IsNullOrWhiteSpace(d.Grade2Grade) ? null : d.Grade2Grade.Trim();
            r.Grade2Role = string.IsNullOrWhiteSpace(d.Grade2Role) ? null : d.Grade2Role.Trim();
            r.Grade2Seniority = string.IsNullOrWhiteSpace(d.Grade2Seniority) ? null : d.Grade2Seniority.Trim();
        }

        private static void ApplySlots(EmploymentData r, List<EmploymentDataSlotDto> items)
        {
            foreach (var s in items.Where(EmploymentSlotPersistence.ShouldPersistSlot))
            {
                r.Slots.Add(new EmploymentDataSlot
                {
                    GradeBand = (byte)s.GradeBand,
                    SlotIndex = (byte)s.SlotIndex,
                    InstitutionSymbol = string.IsNullOrWhiteSpace(s.InstitutionSymbol) ? null : s.InstitutionSymbol.Trim(),
                    WeeklyHours = s.WeeklyHours,
                    JobBase = s.JobBase,
                    SupplementaryParentSlotIndex = s.SupplementaryParentSlotIndex is >= 1 and <= 5
                        ? (byte?)s.SupplementaryParentSlotIndex.Value
                        : null
                });
            }
        }

        private static bool IsFemaleGender(string? gender) =>
            string.Equals(gender?.Trim(), "נקבה", StringComparison.Ordinal)
            || string.Equals(gender?.Trim(), "female", StringComparison.OrdinalIgnoreCase);
    }
}
