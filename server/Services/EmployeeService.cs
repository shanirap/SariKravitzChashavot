using AccountingProject.Contracts;
using AccountingProject.Data;
using AccountingProject.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace AccountingProject.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly PayrollDbContext _db;

        public EmployeeService(PayrollDbContext db)
        {
            _db = db;
        }

        public Task<Employee?> GetByIdAsync(int id) =>
            _db.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);

        public Task<Employee?> GetByEmployerAndIdNumberAsync(int employerId, string idNumber)
        {
            if (employerId <= 0 || string.IsNullOrWhiteSpace(idNumber))
                return Task.FromResult<Employee?>(null);

            var normalized = idNumber.Trim();
            return _db.Employees.AsNoTracking()
                .FirstOrDefaultAsync(e => e.EmployerId == employerId && e.IdNumber == normalized);
        }

        public async Task<EmployeePrecreateHint> GetPrecreateHintAsync(int employerId, string? idNumberRaw, CancellationToken cancellationToken = default)
        {
            if (employerId <= 0 ||
                !await _db.Employers.AnyAsync(e => e.Id == employerId, cancellationToken))
                return new EmployeePrecreateHint(true, false, false);

            var norm = string.IsNullOrWhiteSpace(idNumberRaw) ? string.Empty : idNumberRaw.Trim();
            if (norm.Length == 0)
                return new EmployeePrecreateHint(false, false, false);

            var hasActive = await _db.Employees
                .AnyAsync(e => e.EmployerId == employerId && e.IdNumber == norm, cancellationToken);

            if (hasActive)
                return new EmployeePrecreateHint(false, true, false);

            var hasDeleted = await _db.Employees
                .IgnoreQueryFilters()
                .AnyAsync(e => e.EmployerId == employerId && e.IdNumber == norm && e.IsDeleted, cancellationToken);

            return new EmployeePrecreateHint(false, false, hasDeleted);
        }

        public async Task<EmployeeCreateOrGetResult> CreateOrGetAsync(EmployeeDto dto)
        {
            var normalizedId = dto.IdNumber.Trim();

            if (!await _db.Employers.AnyAsync(e => e.Id == dto.EmployerId))
                throw new InvalidOperationException("המעסיק לא נמצא במערכת.");

            // One query including soft-deleted: avoids missing rows (filters / split-query edge cases).
            var existingRow = await _db.Employees
                .IgnoreQueryFilters()
                .Where(e => e.EmployerId == dto.EmployerId && e.IdNumber == normalizedId)
                .OrderByDescending(e => !e.IsDeleted)
                .ThenBy(e => e.Id)
                .FirstOrDefaultAsync();

            if (existingRow != null)
            {
                if (!existingRow.IsDeleted)
                    return new EmployeeCreateOrGetResult(existingRow, false, false);

                Apply(existingRow, dto);
                existingRow.IsDeleted = false;
                existingRow.DeletedAtUtc = null;
                await SaveChangesOrThrowDuplicateEmployeeAsync();
                return new EmployeeCreateOrGetResult(existingRow, false, true);
            }

            var employee = Apply(new Employee(), dto);
            _db.Employees.Add(employee);
            await SaveChangesOrThrowDuplicateEmployeeAsync();
            return new EmployeeCreateOrGetResult(employee, true, false);
        }

        public async Task<bool> UpdateAsync(int id, EmployeeDto dto)
        {
            var employee = await _db.Employees.FirstOrDefaultAsync(e => e.Id == id);
            if (employee == null) return false;

            if (!await _db.Employers.AnyAsync(e => e.Id == dto.EmployerId))
                throw new InvalidOperationException("המעסיק לא נמצא במערכת.");

            Apply(employee, dto);
            await SaveChangesOrThrowDuplicateEmployeeAsync();
            return true;
        }

        public async Task<bool> SetManualActiveStatusAsync(int id, bool isActive)
        {
            var employee = await _db.Employees.FirstOrDefaultAsync(e => e.Id == id);
            if (employee == null) return false;

            if (isActive)
            {
                var hasEmploymentData = await _db.EmploymentData.AnyAsync(ed => ed.EmployeeId == id);
                if (!hasEmploymentData)
                    throw new InvalidOperationException("לא ניתן להגדיר עובד כפעיל ללא נתוני העסקה.");
            }

            employee.ManualActiveStatus = isActive;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<(bool Success, string? Message)> DeleteAsync(int id)
        {
            var employee = await _db.Employees.FirstOrDefaultAsync(e => e.Id == id);
            if (employee == null) return (false, null);

            var hasData = await _db.EmploymentData.AnyAsync(ed => ed.EmployeeId == id);
            if (hasData)
            {
                return (false, $"לא ניתן למחוק את העובד \"{employee.FullName}\" — קיימים נתוני העסקה מקושרים. מחק תחילה את כל נתוני ההעסקה שלו.");
            }

            _db.Employees.Remove(employee);
            await _db.SaveChangesAsync();
            return (true, null);
        }

        private async Task SaveChangesOrThrowDuplicateEmployeeAsync()
        {
            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                if (ShouldMapToDuplicateEmployeeEmployerTzMessage(ex))
                    throw new InvalidOperationException(
                        "קיים עובד עם תעודת זהות זהה עבור מעסיק זה.", ex);
                throw;
            }
        }

        /// <summary>Expose duplicate-key detection for API controllers (fallback if <see cref="InvalidOperationException"/> was not mapped).</summary>
        public static bool IsDuplicateEmployeeEmployerTzConstraint(Exception ex) =>
            ShouldMapToDuplicateEmployeeEmployerTzMessage(ex);

        /// <summary>
        /// True when SQL reports duplicate insert/update violating the EmployerId+Tz unique index on Employees.
        /// Walks InnerException chains (AggregateException-safe) — EF does not guarantee SqlException depth.
        /// </summary>
        private static bool ShouldMapToDuplicateEmployeeEmployerTzMessage(Exception ex)
        {
            foreach (var e in EnumerateExceptions(ex))
            {
                if (LooksLikeEmployeesEmployerTzUniqueViolation(e.Message))
                    return true;
                if (LooksLikeEmployeesEmployerTzUniqueViolation(e.ToString()))
                    return true;
            }

            // Legacy: typed SqlClient exception (either Microsoft.Data or System.Data).
            foreach (var e in EnumerateExceptions(ex).Where(x => x.GetType().Name == nameof(SqlException)))
            {
                var numProp = e.GetType().GetProperty(nameof(SqlException.Number));
                if (numProp?.GetValue(e) is int n && n is 2601 or 2627
                    && LooksLikeEmployeesEmployerTzUniqueViolation(e.Message))
                    return true;
            }

            return false;
        }

        /// <summary>Does not rely on exception type — works across SqlClient assemblies and wrapping.</summary>
        private static bool LooksLikeEmployeesEmployerTzUniqueViolation(string? text)
        {
            if (string.IsNullOrEmpty(text)) return false;

            var hasEmployeesObject =
                text.Contains("dbo.עובדים", StringComparison.Ordinal)
                || text.Contains("'עובדים'", StringComparison.Ordinal);

            var hasIndex =
                text.Contains("IX_עובדים_מזהה_מעסיק_תז", StringComparison.Ordinal);

            var looksDup =
                text.Contains("duplicate key", StringComparison.OrdinalIgnoreCase)
                || text.Contains("Cannot insert duplicate", StringComparison.OrdinalIgnoreCase)
                || text.Contains("UNIQUE constraint", StringComparison.OrdinalIgnoreCase);

            return (hasIndex && looksDup) || (hasEmployeesObject && hasIndex);
        }

        /// <summary>Depth-first traversal of inner and aggregate exceptions.</summary>
        private static IEnumerable<Exception> EnumerateExceptions(Exception ex)
        {
            if (ex is AggregateException ae)
            {
                foreach (var inner in ae.Flatten().InnerExceptions)
                foreach (var e in EnumerateExceptions(inner))
                        yield return e;
                yield break;
            }

            yield return ex;

            if (ex.InnerException != null)
            {
                foreach (var nested in EnumerateExceptions(ex.InnerException))
                    yield return nested;
            }
        }

        private static Employee Apply(Employee emp, EmployeeDto dto)
        {
            emp.EmployerId = dto.EmployerId;
            emp.IdNumber = dto.IdNumber.Trim();
            emp.FirstName = Normalize(dto.FirstName);
            emp.LastName = Normalize(dto.LastName);
            emp.EmployeeNumber = dto.EmployeeNumber;
            emp.Gender = Normalize(dto.Gender);
            emp.BirthDate = ParseDate(dto.BirthDate);
            emp.Phone = Normalize(dto.Phone);
            emp.ChildBirthDate1 = ParseDate(dto.ChildBirthDate1);
            emp.ChildBirthDate2 = ParseDate(dto.ChildBirthDate2);
            emp.ChildBirthDate3 = ParseDate(dto.ChildBirthDate3);
            emp.ChildBirthDate4 = ParseDate(dto.ChildBirthDate4);
            emp.ChildBirthDate5 = ParseDate(dto.ChildBirthDate5);
            emp.ChildBirthDate6 = ParseDate(dto.ChildBirthDate6);
            emp.ChildBirthDate7 = ParseDate(dto.ChildBirthDate7);
            emp.ChildBirthDate8 = ParseDate(dto.ChildBirthDate8);
            emp.ChildBirthDate9 = ParseDate(dto.ChildBirthDate9);
            emp.ChildBirthDate10 = ParseDate(dto.ChildBirthDate10);
            // Avoid clearing persisted manual status when old clients send update payload without this field.
            if (dto.ManualActiveStatus.HasValue || emp.Id == 0)
                emp.ManualActiveStatus = dto.ManualActiveStatus;
            return emp;
        }

        private static string? Normalize(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static DateOnly? ParseDate(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            return DateOnly.TryParseExact(value.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
                ? date
                : null;
        }
    }
}
