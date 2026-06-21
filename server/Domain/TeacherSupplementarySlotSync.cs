using AccountingProject.Contracts;

namespace AccountingProject.Domain
{
    /// <summary>
    /// סנכרון שורות 3 ש"ש למחנך/גננת — תואם ל-syncTeacherSupplementarySlots ב-frontend.
    /// </summary>
    public static class TeacherSupplementarySlotSync
    {
        public const decimal TeacherExtraHoursWeekly = 3m;

        public static void Sync(EmploymentDataDto dto)
        {
            dto.Slots ??= [];
            EnsureSlotGrid(dto.Slots);

            for (var band = 1; band <= 2; band++)
            {
                var gradeName = band == 1 ? dto.Grade1GradeName : dto.Grade2GradeName;
                var role = band == 1 ? dto.Grade1Role : dto.Grade2Role;
                if (!TeacherSupplementarySlotRules.Qualifies(gradeName, role))
                {
                    ClearSupplementarySlots(dto.Slots, band);
                    continue;
                }

                for (var parentIndex = 1; parentIndex <= 5; parentIndex++)
                {
                    var parent = GetSlot(dto.Slots, band, parentIndex);
                    var child = GetSlot(dto.Slots, band, parentIndex + 1);

                    if (!SlotParentHasSymbolAndHours(parent) || IsSupplementarySlot(parent))
                    {
                        if (child.SupplementaryParentSlotIndex == parentIndex)
                            ClearSlot(child);
                        continue;
                    }

                    child.GradeBand = band;
                    child.SlotIndex = parentIndex + 1;
                    child.InstitutionSymbol = parent.InstitutionSymbol?.Trim();
                    child.WeeklyHours = TeacherExtraHoursWeekly;
                    child.JobBase = parent.JobBase;
                    child.SupplementaryParentSlotIndex = parentIndex;
                }
            }
        }

        private static void EnsureSlotGrid(List<EmploymentDataSlotDto> slots)
        {
            for (var band = 1; band <= 2; band++)
            for (var slotIndex = 1; slotIndex <= 6; slotIndex++)
            {
                if (slots.All(s => s.GradeBand != band || s.SlotIndex != slotIndex))
                {
                    slots.Add(new EmploymentDataSlotDto
                    {
                        GradeBand = band,
                        SlotIndex = slotIndex,
                    });
                }
            }
        }

        private static EmploymentDataSlotDto GetSlot(List<EmploymentDataSlotDto> slots, int band, int slotIndex) =>
            slots.First(s => s.GradeBand == band && s.SlotIndex == slotIndex);

        private static bool IsSupplementarySlot(EmploymentDataSlotDto slot) =>
            slot.SupplementaryParentSlotIndex is >= 1 and <= 5;

        private static bool SlotParentHasSymbolAndHours(EmploymentDataSlotDto slot)
        {
            if (string.IsNullOrWhiteSpace(slot.InstitutionSymbol))
                return false;
            return slot.WeeklyHours is > 0;
        }

        private static void ClearSupplementarySlots(List<EmploymentDataSlotDto> slots, int band)
        {
            foreach (var slot in slots.Where(s => s.GradeBand == band && IsSupplementarySlot(s)))
                ClearSlot(slot);
        }

        private static void ClearSlot(EmploymentDataSlotDto slot)
        {
            slot.InstitutionSymbol = null;
            slot.WeeklyHours = null;
            slot.JobBase = null;
            slot.SupplementaryParentSlotIndex = null;
        }
    }
}
