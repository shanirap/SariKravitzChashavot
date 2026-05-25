namespace AccountingProject.Contracts
{
    /// <summary>
    /// Shown before POST /employees: indicates whether the same employer+IdNumber already exists (active or soft-deleted).
    /// Same person for restore = same <see cref="EmployeeDto.EmployerId"/> + same trimmed <see cref="EmployeeDto.IdNumber"/>.
    /// </summary>
    public sealed record EmployeePrecreateHint(
        bool EmployerMissing,
        bool HasActiveEmployeeWithSameTz,
        bool WillRestoreSoftDeletedEmployee);
}
