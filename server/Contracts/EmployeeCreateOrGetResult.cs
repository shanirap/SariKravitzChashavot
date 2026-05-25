using AccountingProject.Models;

namespace AccountingProject.Contracts
{
    /// <summary>Outcome of create-or-get: new row, restored soft-deleted row, or existing active row.</summary>
    public sealed record EmployeeCreateOrGetResult(
        Employee Employee,
        bool CreatedNew,
        bool RestoredFromSoftDelete);
}
