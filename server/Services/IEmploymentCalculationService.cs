using AccountingProject.Contracts;

namespace AccountingProject.Services
{
    public interface IEmploymentCalculationService
    {
        void PrepareForSave(
            EmploymentDataDto dto,
            DateOnly? employeeBirthDate,
            bool isFemaleEmployee,
            IReadOnlyList<DateOnly?> childBirthDates);

        void ApplyDefaultJobBases(EmploymentDataDto dto);

        void RecalculateDerivedValues(
            EmploymentDataDto dto,
            bool isFemaleEmployee,
            IReadOnlyList<DateOnly?> childBirthDates);
    }
}
