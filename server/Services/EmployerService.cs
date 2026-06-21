using AccountingProject.Contracts;
using AccountingProject.Data;
using AccountingProject.Domain;
using AccountingProject.Models;
using Microsoft.EntityFrameworkCore;

namespace AccountingProject.Services
{
    public class EmployerService : IEmployerService
    {
        private readonly PayrollDbContext _db;

        public EmployerService(PayrollDbContext db)
        {
            _db = db;
        }

        public async Task<PagedResult<Employer>> GetPagedAsync(string? search, int page, int pageSize)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 200);

            var query = _db.Employers.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                var pattern = $"%{term}%";
                query = query.Where(e =>
                    EF.Functions.Like(e.Name, pattern) ||
                    (e.BusinessNumber != null && EF.Functions.Like(e.BusinessNumber, pattern)) ||
                    (e.BeneficiarySymbol != null && EF.Functions.Like(e.BeneficiarySymbol, pattern)) ||
                    (e.EketzNumber != null && EF.Functions.Like(e.EketzNumber, pattern)));
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderBy(e => e.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Employer>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public Task<Employer?> GetByIdAsync(int id) =>
            _db.Employers.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);

        public async Task<Employer> CreateAsync(EmployerDto dto)
        {
            var bn = Normalize(dto.BusinessNumber);

            if (bn != null)
            {
                // Active duplicate: same as before (global query filter excludes soft-deleted rows).
                var activeExists = await _db.Employers.AnyAsync(e => e.BusinessNumber == bn);
                if (activeExists)
                    throw new InvalidOperationException($"מעסיק עם ח.פ. {bn} כבר קיים במערכת.");

                // Restore soft-deleted row by ח.פ. only (never by name).
                var deleted = await _db.Employers
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(e => e.BusinessNumber == bn && e.IsDeleted);
                if (deleted != null)
                {
                    deleted.Name = dto.Name.Trim();
                    deleted.BusinessNumber = bn;
                    deleted.BeneficiarySymbol = Normalize(dto.BeneficiarySymbol);
                    deleted.EketzNumber = Normalize(dto.EketzNumber);
                    deleted.IsDeleted = false;
                    deleted.DeletedAtUtc = null;
                    await _db.SaveChangesAsync();
                    return deleted;
                }
            }

            var employer = new Employer
            {
                Name = dto.Name.Trim(),
                BusinessNumber = bn,
                BeneficiarySymbol = Normalize(dto.BeneficiarySymbol),
                EketzNumber = Normalize(dto.EketzNumber)
            };

            _db.Employers.Add(employer);
            await _db.SaveChangesAsync();
            return employer;
        }

        public async Task<bool> UpdateAsync(int id, EmployerDto dto)
        {
            var employer = await _db.Employers.FirstOrDefaultAsync(e => e.Id == id);
            if (employer == null) return false;

            var bn = Normalize(dto.BusinessNumber);
            if (bn != null)
            {
                var duplicateExists = await _db.Employers.AnyAsync(e => e.Id != id && e.BusinessNumber == bn);
                if (duplicateExists)
                    throw new InvalidOperationException($"מעסיק עם ח.פ. {bn} כבר קיים במערכת.");
            }

            employer.Name = dto.Name.Trim();
            employer.BusinessNumber = bn;
            employer.BeneficiarySymbol = Normalize(dto.BeneficiarySymbol);
            employer.EketzNumber = Normalize(dto.EketzNumber);

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<(bool Success, string? Message)> DeleteAsync(int id)
        {
            var employer = await _db.Employers.FirstOrDefaultAsync(e => e.Id == id);
            if (employer == null) return (false, null);

            var hasData = await _db.EmploymentData.AnyAsync(ed => ed.EmployerId == id);
            if (hasData)
            {
                return (false, $"לא ניתן למחוק את \"{employer.Name}\" — קיימים נתוני העסקה מקושרים. מחק תחילה את כל נתוני ההעסקה של עובדי מעסיק זה.");
            }

            var hasEmployees = await _db.Employees.AnyAsync(e => e.EmployerId == id);
            if (hasEmployees)
            {
                return (false, $"לא ניתן למחוק את \"{employer.Name}\" — קיימים עובדים מקושרים למעסיק זה.");
            }

            _db.Employers.Remove(employer);
            await _db.SaveChangesAsync();
            return (true, null);
        }

        public async Task<PagedResult<Employee>> GetEmployeesAsync(
            int employerId,
            string? search,
            int page,
            int pageSize,
            bool? isActive = null,
            string? institutionSymbol = null)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 200);

            var query = _db.Employees
                .AsNoTracking()
                .Where(e => e.EmployerId == employerId || e.EmploymentData.Any(ed => ed.EmployerId == employerId));

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                var pattern = $"%{term}%";
                query = query.Where(e =>
                    (e.FirstName != null && EF.Functions.Like(e.FirstName, pattern)) ||
                    (e.LastName != null && EF.Functions.Like(e.LastName, pattern)) ||
                    EF.Functions.Like(e.IdNumber, pattern) ||
                    (e.EmployeeNumber != null && EF.Functions.Like(e.EmployeeNumber.ToString()!, pattern)));
            }

            if (isActive.HasValue)
            {
                if (isActive.Value)
                {
                    query = query.Where(e =>
                        e.ManualActiveStatus == true
                        || (e.ManualActiveStatus == null
                            && e.EmploymentData.Any(ed => ed.EmployerId == employerId)));
                }
                else
                {
                    query = query.Where(e =>
                        e.ManualActiveStatus == false
                        || (e.ManualActiveStatus == null
                            && !e.EmploymentData.Any(ed => ed.EmployerId == employerId)));
                }
            }

            if (!string.IsNullOrWhiteSpace(institutionSymbol))
            {
                var sym = institutionSymbol.Trim();
                query = query.Where(e =>
                    e.EmploymentData.Any(ed =>
                        ed.EmployerId == employerId
                        && ed.Slots.Any(s => s.InstitutionSymbol == sym)));
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Employee>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<HashSet<int>> GetEmployeeIdsWithEmploymentDataAsync(int employerId, IReadOnlyList<int> employeeIds)
        {
            if (employeeIds == null || employeeIds.Count == 0)
                return new HashSet<int>();

            var ids = await _db.EmploymentData
                .AsNoTracking()
                .Where(ed => ed.EmployerId == employerId && employeeIds.Contains(ed.EmployeeId))
                .Select(ed => ed.EmployeeId)
                .Distinct()
                .ToListAsync();

            return ids.ToHashSet();
        }

        public async Task<IReadOnlyList<EmployerInstitutionSymbol>> GetInstitutionSymbolsAsync(int employerId)
        {
            return await _db.EmployerInstitutionSymbols
                .AsNoTracking()
                .Where(s => s.EmployerId == employerId)
                .OrderBy(s => s.InstitutionSymbol)
                .ToListAsync();
        }

        public async Task<(EmployerInstitutionSymbol? Symbol, string? Message)> CreateInstitutionSymbolAsync(int employerId, EmployerInstitutionSymbolDto dto)
        {
            var employer = await _db.Employers.FirstOrDefaultAsync(e => e.Id == employerId);
            if (employer == null) return (null, null);

            var institutionSymbol = Normalize(dto.InstitutionSymbol);
            if (institutionSymbol == null)
                return (null, "סמל מוסד הוא שדה חובה.");

            var exists = await _db.EmployerInstitutionSymbols.AnyAsync(s =>
                s.EmployerId == employerId
                && s.InstitutionSymbol == institutionSymbol);
            if (exists)
                return (null, "סמל מוסד זה כבר קיים למעסיק.");

            var (institutionType, typeError) = InstitutionTypes.Resolve(dto.InstitutionType);
            if (typeError != null)
                return (null, typeError);

            var symbol = new EmployerInstitutionSymbol
            {
                EmployerId = employerId,
                InstitutionSymbol = institutionSymbol,
                InstitutionSymbolName = Normalize(dto.InstitutionSymbolName),
                InstitutionType = institutionType,
            };

            _db.EmployerInstitutionSymbols.Add(symbol);
            await _db.SaveChangesAsync();
            return (symbol, null);
        }

        public async Task<(EmployerInstitutionSymbol? Symbol, string? Message)> UpdateInstitutionSymbolAsync(
            int employerId, int symbolId, EmployerInstitutionSymbolUpdateDto dto)
        {
            var symbol = await _db.EmployerInstitutionSymbols.FirstOrDefaultAsync(s =>
                s.Id == symbolId && s.EmployerId == employerId);
            if (symbol == null)
                return (null, null);

            if (dto.InstitutionSymbolName != null)
                symbol.InstitutionSymbolName = Normalize(dto.InstitutionSymbolName);

            if (dto.InstitutionType != null)
            {
                var (institutionType, typeError) = InstitutionTypes.Resolve(dto.InstitutionType);
                if (typeError != null)
                    return (null, typeError);
                symbol.InstitutionType = institutionType;
            }

            await _db.SaveChangesAsync();
            return (symbol, null);
        }

        public async Task<byte[]?> BuildFullEmployerExportExcelAsync(int employerId)
        {
            var employer = await GetByIdAsync(employerId);
            if (employer == null) return null;

            var employees = await _db.Employees
                .AsNoTracking()
                .Where(e => e.EmployerId == employerId || e.EmploymentData.Any(ed => ed.EmployerId == employerId))
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .ToListAsync();

            var symbols = await GetInstitutionSymbolsAsync(employerId);

            var employmentList = await _db.EmploymentData
                .AsNoTracking()
                .Include(ed => ed.Employee)
                .Include(ed => ed.Slots)
                .Where(ed => ed.EmployerId == employerId)
                .OrderBy(ed => ed.Employee!.LastName)
                .ThenBy(ed => ed.Employee!.FirstName)
                .ThenBy(ed => ed.AcademicYear)
                .ToListAsync();

            var empIdsWithEd = employmentList.Select(ed => ed.EmployeeId).ToHashSet();

            return EmployerFullExcelExport.Build(employer, employees, symbols, employmentList, empIdsWithEd);
        }

        public async Task<(bool Success, string? Message)> DeleteInstitutionSymbolAsync(int employerId, int symbolId)
        {
            var symbol = await _db.EmployerInstitutionSymbols.FirstOrDefaultAsync(s =>
                s.Id == symbolId
                && s.EmployerId == employerId);
            if (symbol == null) return (false, null);

            var inUse = await _db.EmploymentDataSlots.AnyAsync(s =>
                s.InstitutionSymbol == symbol.InstitutionSymbol
                && s.EmploymentData!.EmployerId == employerId);
            if (inUse)
                return (false, "לא ניתן למחוק סמל מוסד שנמצא בשימוש בנתוני העסקה.");

            _db.EmployerInstitutionSymbols.Remove(symbol);
            await _db.SaveChangesAsync();
            return (true, null);
        }

        private static string? Normalize(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}

