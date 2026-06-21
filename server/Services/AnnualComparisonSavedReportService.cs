using AccountingProject.Contracts;
using AccountingProject.Data;
using AccountingProject.Domain;
using AccountingProject.Models;
using Microsoft.EntityFrameworkCore;

namespace AccountingProject.Services;

public interface IAnnualComparisonSavedReportService
{
    Task<AnnualComparisonPreviewDto> GetPreviewAsync(int employerId, string academicYear);
    Task SaveOverridesAsync(int employerId, string academicYear, IReadOnlyList<AnnualComparisonOverrideRowSaveDto> rows);
    Task ClearOverridesAsync(int employerId, string academicYear, int? slotId);
    Task<Dictionary<int, AnnualComparisonReportRowOverride>> LoadOverridesBySlotAsync(int employerId, string academicYear);
}

public sealed class AnnualComparisonSavedReportService : IAnnualComparisonSavedReportService
{
    private readonly PayrollDbContext _db;

    public AnnualComparisonSavedReportService(PayrollDbContext db) => _db = db;

    public async Task<AnnualComparisonPreviewDto> GetPreviewAsync(int employerId, string academicYear)
    {
        await EnsureEmployerExistsAsync(employerId);
        var canonYear = PayrollComparisonUploadSupport.CanonAcademicYear(academicYear);
        var (records, monthSequence, monthSource) = await LoadReportContextAsync(employerId, canonYear);
        var overrides = await LoadOverridesBySlotAsync(employerId, canonYear);

        var monthHeaders = monthSequence.Select(m => $"{m.Month}.{m.GregorianYear}").ToList();
        var rows = new List<AnnualComparisonPreviewRowDto>();

        foreach (var ed in records.OrderBy(e => e.Employee?.LastName).ThenBy(e => e.Employee?.FirstName))
        {
            foreach (var slot in ed.Slots
                         .Where(s => !PayrollComparisonUploadSupport.SlotIsEmpty(s))
                         .OrderBy(s => s.GradeBand)
                         .ThenBy(s => s.SlotIndex))
            {
                var computed = AnnualComparisonRowComputer.Compute(ed, slot, monthSequence, monthSource);
                overrides.TryGetValue(slot.Id, out var ovr);
                var display = AnnualComparisonRowComputer.ToDisplay(computed, ovr);
                rows.Add(ToPreviewRow(computed, display, ovr));
            }
        }

        if (rows.Count == 0)
            throw new InvalidOperationException("לא נמצאו מקטעי העסקה להשוואה.");

        return new AnnualComparisonPreviewDto
        {
            AcademicYear = canonYear,
            MonthHeaders = monthHeaders,
            Rows = rows,
        };
    }

    public async Task SaveOverridesAsync(
        int employerId,
        string academicYear,
        IReadOnlyList<AnnualComparisonOverrideRowSaveDto> rows)
    {
        if (rows.Count == 0)
            return;

        await EnsureEmployerExistsAsync(employerId);
        var canonYear = PayrollComparisonUploadSupport.CanonAcademicYear(academicYear);
        var (records, monthSequence, monthSource) = await LoadReportContextAsync(employerId, canonYear);
        var computedBySlot = BuildComputedBySlot(records, monthSequence, monthSource);

        var slotIds = rows.Select(r => r.SlotId).Distinct().ToList();
        var existing = await _db.AnnualComparisonReportRowOverrides
            .Where(o => o.EmployerId == employerId && o.AcademicYear == canonYear && slotIds.Contains(o.SlotId))
            .ToDictionaryAsync(o => o.SlotId);

        foreach (var save in rows)
        {
            if (!computedBySlot.TryGetValue(save.SlotId, out var computed))
                throw new InvalidOperationException($"מקטע {save.SlotId} לא נמצא בדוח.");

            existing.TryGetValue(save.SlotId, out var current);
            var entity = AnnualComparisonRowComputer.BuildOverrideEntity(
                employerId, canonYear, computed, save, current);

            if (!entity.IsManualEdited)
            {
                if (current != null)
                    _db.AnnualComparisonReportRowOverrides.Remove(current);
                continue;
            }

            if (current == null)
                _db.AnnualComparisonReportRowOverrides.Add(entity);
        }

        await _db.SaveChangesAsync();
    }

    public async Task ClearOverridesAsync(int employerId, string academicYear, int? slotId)
    {
        await EnsureEmployerExistsAsync(employerId);
        var canonYear = PayrollComparisonUploadSupport.CanonAcademicYear(academicYear);

        var query = _db.AnnualComparisonReportRowOverrides
            .Where(o => o.EmployerId == employerId && o.AcademicYear == canonYear);

        if (slotId.HasValue)
            query = query.Where(o => o.SlotId == slotId.Value);

        var toRemove = await query.ToListAsync();
        if (toRemove.Count == 0)
            return;

        _db.AnnualComparisonReportRowOverrides.RemoveRange(toRemove);
        await _db.SaveChangesAsync();
    }

    public async Task<Dictionary<int, AnnualComparisonReportRowOverride>> LoadOverridesBySlotAsync(
        int employerId,
        string academicYear)
    {
        var canonYear = PayrollComparisonUploadSupport.CanonAcademicYear(academicYear);
        return await _db.AnnualComparisonReportRowOverrides
            .AsNoTracking()
            .Where(o => o.EmployerId == employerId && o.AcademicYear == canonYear)
            .ToDictionaryAsync(o => o.SlotId);
    }

    private static AnnualComparisonPreviewRowDto ToPreviewRow(
        AnnualComparisonRowComputer.ComputedRow computed,
        AnnualComparisonRowComputer.DisplayRow display,
        AnnualComparisonReportRowOverride? ovr)
    {
        var monthOverrides = AnnualComparisonRowComputer.ParseMonthCellsJson(ovr?.MonthCellsJson);
        var monthCells = new Dictionary<string, AnnualComparisonFieldDto>();
        foreach (var (key, computedValue) in computed.MonthCells)
        {
            var displayValue = display.MonthCells.GetValueOrDefault(key) ?? computedValue;
            monthCells[key] = Field(computedValue, displayValue);
        }

        return new AnnualComparisonPreviewRowDto
        {
            SlotId = computed.SlotId,
            GradeBand = computed.GradeBand,
            InstitutionSymbol = Field(computed.InstitutionSymbol, display.InstitutionSymbol),
            FullName = Field(computed.FullName, display.FullName),
            Role = Field(computed.Role, display.Role),
            SugMisraFromPayroll = Field(computed.SugMisraFromPayroll, display.SugMisraFromPayroll),
            Grade = Field(computed.Grade, display.Grade),
            Seniority = Field(computed.Seniority, display.Seniority),
            WeeklyHours = Field(
                AnnualComparisonRowComputer.FormatDecimal(computed.WeeklyHours),
                AnnualComparisonRowComputer.FormatDecimal(display.WeeklyHours)),
            JobBase = Field(
                AnnualComparisonRowComputer.FormatDecimal(computed.JobBase),
                AnnualComparisonRowComputer.FormatDecimal(display.JobBase)),
            JobPercent = Field(
                AnnualComparisonRowComputer.FormatDecimal(computed.JobPercent),
                AnnualComparisonRowComputer.FormatDecimal(display.JobPercent)),
            DoubleGeneral = Field(
                AnnualComparisonRowComputer.FormatDecimal(computed.DoubleGeneral),
                AnnualComparisonRowComputer.FormatDecimal(display.DoubleGeneral)),
            MonthCells = monthCells,
            IsManualEdited = ovr?.IsManualEdited ?? false,
            ManualEditNote = ovr?.ManualEditNote,
        };
    }

    private static AnnualComparisonFieldDto Field(string computed, string display) =>
        new()
        {
            Computed = computed,
            Display = display,
            IsOverridden = !PayrollComparisonUploadSupport.TextEqual(computed, display),
        };

    private static Dictionary<int, AnnualComparisonRowComputer.ComputedRow> BuildComputedBySlot(
        IReadOnlyList<EmploymentData> records,
        IReadOnlyList<(int Month, int GregorianYear)> monthSequence,
        IAnnualComparisonMonthSource monthSource)
    {
        var result = new Dictionary<int, AnnualComparisonRowComputer.ComputedRow>();
        foreach (var ed in records)
        {
            foreach (var slot in ed.Slots.Where(s => !PayrollComparisonUploadSupport.SlotIsEmpty(s)))
                result[slot.Id] = AnnualComparisonRowComputer.Compute(ed, slot, monthSequence, monthSource);
        }

        return result;
    }

    private async Task<(List<EmploymentData> Records,
        IReadOnlyList<(int Month, int GregorianYear)> MonthSequence,
        IAnnualComparisonMonthSource MonthSource)>
        LoadReportContextAsync(int employerId, string canonYear)
    {
        var records = await _db.EmploymentData
            .Include(e => e.Employee)
            .Include(e => e.Slots)
            .Where(e => e.EmployerId == employerId
                        && e.AcademicYear == canonYear
                        && !e.IsDeleted)
            .ToListAsync();

        int sepYear;
        if (!HebrewAcademicYear.TryParseSeptemberGregorianYear(canonYear, out sepYear))
            throw new InvalidOperationException(HebrewAcademicYear.InvalidMessage);

        var monthSequence = SchoolYearGregorian.GetSchoolYearMonthSequence(sepYear);
        var (activeBatches, comparisonRows) = await LoadSavedAnnualComparisonInputAsync(employerId, canonYear);
        var monthSource = AnnualComparisonSavedMonthSource.FromSavedRows(activeBatches, comparisonRows);

        return (records, monthSequence, monthSource);
    }

    private async Task<(List<PayrollMonthlyInputBatch> ActiveBatches, List<PayrollComparisonInputRow> ComparisonRows)>
        LoadSavedAnnualComparisonInputAsync(int employerId, string canonYear)
    {
        var activeBatches = await _db.PayrollMonthlyInputBatches
            .AsNoTracking()
            .Where(b => b.EmployerId == employerId && b.IsActive && !b.IsDeleted)
            .ToListAsync();

        activeBatches = activeBatches
            .Where(b => PayrollComparisonUploadSupport.CanonAcademicYear(b.AcademicYear) == canonYear)
            .ToList();

        if (activeBatches.Count == 0)
            return ([], []);

        var batchIds = activeBatches.Select(b => b.Id).ToList();
        var entityRows = await _db.PayrollMonthlyInputRows
            .AsNoTracking()
            .Where(r => batchIds.Contains(r.BatchId) && r.EmployerId == employerId && !r.IsDeleted)
            .ToListAsync();

        return (activeBatches, entityRows.Select(AnnualComparisonSavedRowMapper.ToComparisonInput).ToList());
    }

    private async Task EnsureEmployerExistsAsync(int employerId)
    {
        if (!await _db.Employers.AnyAsync(e => e.Id == employerId))
            throw new InvalidOperationException("המעסיק לא נמצא במערכת.");
    }

    private static readonly Dictionary<char, int> HebrewLetterValues = new()
    {
        ['ת'] = 400, ['ש'] = 300, ['ר'] = 200, ['ק'] = 100,
        ['צ'] = 90,  ['פ'] = 80,  ['ע'] = 70,  ['ס'] = 60,
        ['נ'] = 50,  ['מ'] = 40,  ['ל'] = 30,  ['כ'] = 20,
        ['י'] = 10,  ['ט'] = 9,   ['ח'] = 8,   ['ז'] = 7,
        ['ו'] = 6,   ['ה'] = 5,   ['ד'] = 4,   ['ג'] = 3,
        ['ב'] = 2,   ['א'] = 1,
    };

    private static int ParseSeptemberGregorianYear(string hebrewYear)
    {
        var sum = hebrewYear.Where(c => HebrewLetterValues.ContainsKey(c)).Sum(c => HebrewLetterValues[c]);
        return 5000 + sum - 3761;
    }
}
