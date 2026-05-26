using AccountingProject.Contracts;
using AccountingProject.Data;
using AccountingProject.Domain;
using AccountingProject.Infrastructure;
using AccountingProject.Models;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;

namespace AccountingProject.Services
{
    public class PayrollMonthlyInputService : IPayrollMonthlyInputService
    {
        private const string StatusCaptured = "נקלט";
        private const string StatusMissing = "חסר";
        private const string RowNotFoundMessage = "שורת קלט עוקץ חודשי לא נמצאה.";

        private static readonly Dictionary<char, int> HebrewLetterValues = new()
        {
            ['ת'] = 400, ['ש'] = 300, ['ר'] = 200, ['ק'] = 100,
            ['צ'] = 90,  ['פ'] = 80,  ['ע'] = 70,  ['ס'] = 60,
            ['נ'] = 50,  ['מ'] = 40,  ['ל'] = 30,  ['כ'] = 20,
            ['י'] = 10,  ['ט'] = 9,   ['ח'] = 8,   ['ז'] = 7,
            ['ו'] = 6,   ['ה'] = 5,   ['ד'] = 4,   ['ג'] = 3,
            ['ב'] = 2,   ['א'] = 1
        };

        private readonly PayrollDbContext _db;
        private readonly ICurrentUserService _currentUserService;

        public PayrollMonthlyInputService(PayrollDbContext db, ICurrentUserService currentUserService)
        {
            _db = db;
            _currentUserService = currentUserService;
        }

        public async Task<PayrollImportResultDto> ImportMonthAsync(
            int employerId,
            string academicYear,
            int month,
            Stream file,
            string originalFileName)
        {
            if (employerId <= 0)
                throw new InvalidOperationException("מזהה מעסיק לא תקין.");
            if (string.IsNullOrWhiteSpace(academicYear))
                throw new InvalidOperationException("שנת לימודים נדרשת.");
            if (month is < 1 or > 12)
                throw new InvalidOperationException("חודש חייב להיות בין 1 ל-12.");
            if (file is not { CanRead: true })
                throw new InvalidOperationException("קובץ העלאה אינו קריא.");

            if (!await _db.Employers.AnyAsync(e => e.Id == employerId))
                throw new InvalidOperationException("המעסיק לא נמצא במערכת.");

            var canonYear = CanonAcademicYear(academicYear);
            var gregorianYear = PayrollComparisonUploadSupport.ResolveExpectedGregorianYear(
                canonYear,
                month,
                ParseSeptemberGregorianYear);

            List<PayrollComparisonInputRow> parsedRows;
            try
            {
                if (file.CanSeek)
                    file.Position = 0;

                using var workbook = new XLWorkbook(file);
                var sheet = workbook.Worksheets.FirstOrDefault()
                            ?? throw new InvalidOperationException("בחוברת Excel אין גיליונות נתונים.");
                var layout = PayrollComparisonUploadSupport.ParseLayout(sheet);
                parsedRows = PayrollComparisonUploadSupport.ParseInputRowsForMonth(
                    sheet,
                    layout,
                    month,
                    gregorianYear,
                    canonYear,
                    includeRawCellsJson: true);
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "קובץ ה-Excel פגום או לא תקין. יש להעלות קובץ .xlsx תקין.",
                    ex);
            }

            if (parsedRows.Count == 0)
                throw new InvalidOperationException($"לא נמצאו שורות נתונים בקובץ לחודש {month}.");

            var now = DateTime.UtcNow;
            var uploadedBy = NormalizeUploadedBy(_currentUserService.GetAuditActor());
            var storedFileName = string.IsNullOrWhiteSpace(originalFileName)
                ? "upload.xlsx"
                : originalFileName.Trim();

            await using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var activeBatches = await _db.PayrollMonthlyInputBatches
                    .Where(b =>
                        b.EmployerId == employerId
                        && b.Month == month
                        && b.GregorianYear == gregorianYear
                        && b.IsActive
                        && !b.IsDeleted)
                    .ToListAsync();

                foreach (var existing in activeBatches.Where(b => CanonAcademicYear(b.AcademicYear) == canonYear))
                {
                    existing.IsActive = false;
                    existing.UpdatedAtUtc = now;
                }

                var batch = new PayrollMonthlyInputBatch
                {
                    EmployerId = employerId,
                    AcademicYear = canonYear,
                    Month = month,
                    GregorianYear = gregorianYear,
                    OriginalFileName = storedFileName,
                    UploadedAtUtc = now,
                    UploadedBy = uploadedBy,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAtUtc = now,
                };
                _db.PayrollMonthlyInputBatches.Add(batch);
                await _db.SaveChangesAsync();

                var entities = parsedRows
                    .Select(row => MapParsedRowToEntity(row, batch.Id, employerId, canonYear, now))
                    .ToList();
                _db.PayrollMonthlyInputRows.AddRange(entities);
                batch.RowsCount = entities.Count;
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                return new PayrollImportResultDto
                {
                    BatchId = batch.Id,
                    EmployerId = employerId,
                    AcademicYear = canonYear,
                    Month = month,
                    GregorianYear = gregorianYear,
                    RowsCount = entities.Count,
                    OriginalFileName = storedFileName,
                    Message = $"נקלטו {entities.Count} שורות בהצלחה.",
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<IReadOnlyList<PayrollMonthStatusDto>> GetYearStatusAsync(
            int employerId,
            string academicYear)
        {
            var canonYear = CanonAcademicYear(academicYear);
            var sepYear = ParseSeptemberGregorianYear(canonYear);
            var monthSequence = SchoolYearGregorian.GetSchoolYearMonthSequence(sepYear);

            var activeBatches = await _db.PayrollMonthlyInputBatches
                .AsNoTracking()
                .Where(b => b.EmployerId == employerId && b.IsActive && !b.IsDeleted)
                .ToListAsync();

            var batchesByMonth = activeBatches
                .Where(b => CanonAcademicYear(b.AcademicYear) == canonYear)
                .ToDictionary(b => (b.Month, b.GregorianYear));

            return monthSequence
                .Select(m =>
                {
                    batchesByMonth.TryGetValue((m.Month, m.GregorianYear), out var batch);
                    return new PayrollMonthStatusDto
                    {
                        Month = m.Month,
                        GregorianYear = m.GregorianYear,
                        DisplayName = $"{m.Month}.{m.GregorianYear}",
                        Status = batch != null ? StatusCaptured : StatusMissing,
                        BatchId = batch?.Id,
                        RowsCount = batch?.RowsCount ?? 0,
                        UploadedAtUtc = batch?.UploadedAtUtc,
                        OriginalFileName = batch?.OriginalFileName
                    };
                })
                .ToList();
        }

        public async Task<IReadOnlyList<PayrollMonthlyInputRowDto>> GetRowsAsync(
            int employerId,
            string academicYear,
            int month)
        {
            var batch = await FindActiveBatchAsync(employerId, academicYear, month);
            if (batch == null)
                return [];

            var rows = await _db.PayrollMonthlyInputRows
                .AsNoTracking()
                .Where(r => r.BatchId == batch.Id && !r.IsDeleted)
                .OrderBy(r => r.SourceExcelRowNumber ?? int.MaxValue)
                .ThenBy(r => r.Id)
                .ToListAsync();

            return rows.Select(ToDto).ToList();
        }

        public async Task<PayrollMonthlyInputRowDto> UpdateRowAsync(
            int rowId,
            PayrollMonthlyInputRowEditDto dto)
        {
            var row = await _db.PayrollMonthlyInputRows
                .FirstOrDefaultAsync(r => r.Id == rowId && !r.IsDeleted);
            if (row == null)
                throw new InvalidOperationException(RowNotFoundMessage);

            ApplyEdit(row, dto);
            row.IsManualEdited = true;
            row.UpdatedAtUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return ToDto(row);
        }

        public async Task DeleteRowAsync(int rowId)
        {
            var row = await _db.PayrollMonthlyInputRows
                .FirstOrDefaultAsync(r => r.Id == rowId && !r.IsDeleted);
            if (row == null)
                throw new InvalidOperationException(RowNotFoundMessage);

            row.IsDeleted = true;
            row.DeletedAtUtc = DateTime.UtcNow;
            row.UpdatedAtUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        private async Task<PayrollMonthlyInputBatch?> FindActiveBatchAsync(
            int employerId,
            string academicYear,
            int month)
        {
            var canonYear = CanonAcademicYear(academicYear);
            var gregorianYear = PayrollComparisonUploadSupport.ResolveExpectedGregorianYear(
                canonYear,
                month,
                ParseSeptemberGregorianYear);

            var batches = await _db.PayrollMonthlyInputBatches
                .AsNoTracking()
                .Where(b =>
                    b.EmployerId == employerId
                    && b.Month == month
                    && b.GregorianYear == gregorianYear
                    && b.IsActive
                    && !b.IsDeleted)
                .ToListAsync();

            return batches.FirstOrDefault(b => CanonAcademicYear(b.AcademicYear) == canonYear);
        }

        private static void ApplyEdit(PayrollMonthlyInputRow row, PayrollMonthlyInputRowEditDto dto)
        {
            row.InstitutionSymbol = dto.InstitutionSymbol;
            row.OketzEmployeeNumber = dto.OketzEmployeeNumber;
            row.IdNumber = dto.IdNumber;
            row.FullName = dto.FullName;
            row.Role = dto.Role;
            row.Grade = dto.Grade;
            row.Seniority = dto.Seniority;
            row.WeeklyHours = dto.WeeklyHours;
            row.JobBase = dto.JobBase;
            row.JobPercent = dto.JobPercent;
            row.AgeHours = dto.AgeHours;
            row.TrainingBenefits = dto.TrainingBenefits;
            row.DoubleDegree = dto.DoubleDegree;
            row.TrainingFund = dto.TrainingFund;
            row.GeneralMultiplier = dto.GeneralMultiplier;
            row.ManualEditNote = dto.ManualEditNote;
        }

        private static PayrollMonthlyInputRowDto ToDto(PayrollMonthlyInputRow row) => new()
        {
            Id = row.Id,
            BatchId = row.BatchId,
            InstitutionSymbol = row.InstitutionSymbol,
            OketzEmployeeNumber = row.OketzEmployeeNumber,
            IdNumber = row.IdNumber,
            FullName = row.FullName,
            Role = row.Role,
            Grade = row.Grade,
            Seniority = row.Seniority,
            WeeklyHours = row.WeeklyHours,
            JobBase = row.JobBase,
            JobPercent = row.JobPercent,
            AgeHours = row.AgeHours,
            TrainingBenefits = row.TrainingBenefits,
            DoubleDegree = row.DoubleDegree,
            TrainingFund = row.TrainingFund,
            GeneralMultiplier = row.GeneralMultiplier,
            IsManualEdited = row.IsManualEdited
        };

        private static PayrollMonthlyInputRow MapParsedRowToEntity(
            PayrollComparisonInputRow parsed,
            int batchId,
            int employerId,
            string canonYear,
            DateTime now) => new()
        {
            BatchId = batchId,
            EmployerId = employerId,
            AcademicYear = canonYear,
            Month = parsed.Month,
            GregorianYear = parsed.GregorianYear,
            SourceExcelRowNumber = parsed.SourceExcelRowNumber,
            InstitutionSymbol = parsed.InstitutionSymbol,
            OketzEmployeeNumber = parsed.OketzEmployeeNumber,
            IdNumber = parsed.IdNumber,
            FullName = parsed.FullName,
            Role = parsed.Role,
            Grade = parsed.Grade,
            Seniority = parsed.Seniority,
            WeeklyHours = parsed.WeeklyHours,
            JobBase = parsed.JobBase,
            JobPercent = parsed.JobPercent,
            AgeHours = parsed.AgeHours,
            TrainingBenefits = parsed.TrainingBenefits,
            DoubleDegree = parsed.DoubleDegree,
            TrainingFund = parsed.TrainingFund,
            GeneralMultiplier = parsed.GeneralMultiplier,
            RawCellsJson = parsed.RawCellsJson,
            IsManualEdited = false,
            IsDeleted = false,
            CreatedAtUtc = now,
        };

        private static string? NormalizeUploadedBy(string actor)
        {
            if (string.IsNullOrWhiteSpace(actor))
                return null;
            var trimmed = actor.Trim();
            return trimmed.Length <= 200 ? trimmed : trimmed[..200];
        }

        private static string CanonAcademicYear(string? stored) =>
            PayrollComparisonUploadSupport.CanonAcademicYear(stored);

        private static int ParseSeptemberGregorianYear(string hebrewYear)
        {
            var sum = hebrewYear
                .Where(c => HebrewLetterValues.ContainsKey(c))
                .Sum(c => HebrewLetterValues[c]);
            if (sum == 0)
                throw new InvalidOperationException("שנת לימודים לא תקינה.");
            var hebrewYearFull = 5000 + sum;
            return hebrewYearFull - 3761;
        }
    }
}
