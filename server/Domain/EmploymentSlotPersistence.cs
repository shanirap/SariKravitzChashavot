using AccountingProject.Contracts;
using AccountingProject.Models;

namespace AccountingProject.Domain
{
    /// <summary>
    /// כלל אחיד: מתי לשמור מקטע ב-DB (תואם לסינון בדוחות).
    /// SlotIndex נשמר כמיקום 1–6 — ללא דחיסה מחדש.
    /// </summary>
    public static class EmploymentSlotPersistence
    {
        public static bool ShouldPersistSlot(EmploymentDataSlotDto slot) =>
            slot.SupplementaryParentSlotIndex is >= 1 and <= 5
            || !string.IsNullOrWhiteSpace(slot.InstitutionSymbol)
            || slot.WeeklyHours is > 0;

        public static bool ShouldPersistSlot(EmploymentDataSlot slot) =>
            slot.SupplementaryParentSlotIndex is >= 1 and <= 5
            || !string.IsNullOrWhiteSpace(slot.InstitutionSymbol)
            || slot.WeeklyHours is > 0;
    }
}
