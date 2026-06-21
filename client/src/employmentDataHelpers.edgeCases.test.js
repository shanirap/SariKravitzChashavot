import { describe, it, expect } from 'vitest';
import {
  computeMotherBenefitPercentString,
  computeGradeBaseJobPercentNumber,
  employmentFormHasChildUpToAgeInclusive,
  ageInFullYearsAtDate,
  emptyEmploymentSlot,
  academicYearStartRefDate,
  computeGradeJobPercentString,
  patchEmploymentSlotJobBases,
} from './employmentDataHelpers.jsx';

const refDate = new Date(2025, 8, 1, 12, 0, 0);

function makeSlotsBand1(weeklyHours, jobBase) {
  const slots = [];
  for (let b = 1; b <= 2; b++) {
    for (let i = 1; i <= 6; i++) {
      slots.push(emptyEmploymentSlot(b, i));
    }
  }
  slots[0] = {
    ...slots[0],
    institutionSymbol: 'G-1',
    weeklyHours: String(weeklyHours),
    jobBase: String(jobBase),
  };
  return slots;
}

function baseForm(overrides = {}) {
  return {
    employeeGender: 'נקבה',
    academicYear: 'תשפ"ו',
    childBirthDate1: '2012-03-15',
    grade1GradeName: 'יסודי וגנים',
    grade1Total: '30',
    grade2GradeName: '',
    slots: makeSlotsBand1(30, 30),
    ...overrides,
  };
}

describe('אופק גנים job base by role', () => {
  it.each([
    ['גננת ראשית', '30.4'],
    ['גננת עמיתה', '33.8'],
    ['פרא רפואי', '33.8'],
  ])('patchEmploymentSlotJobBases sets %s → %s', (role, expectedJobBase) => {
    const form = patchEmploymentSlotJobBases({
      grade1GradeName: 'אופק גנים',
      grade1Role: role,
      slots: makeSlotsBand1(30, ''),
    });
    expect(form.slots[0].jobBase).toBe(expectedJobBase);
  });
});

describe('mother benefit edge cases', () => {
  it('patchEmploymentSlotJobBases stores gross job base without age hours deduction', () => {
    const form = patchEmploymentSlotJobBases({
      grade1GradeName: 'יסודי וגנים',
      grade1Role: 'גננת ראשית',
      grade1AgeHours: '2',
      slots: makeSlotsBand1(30, ''),
    });
    expect(form.slots[0].jobBase).toBe('30');
  });

  it('job percent uses gross job base minus age hours', () => {
    const form = {
      grade1GradeName: 'יסודי וגנים',
      grade1Role: 'גננת ראשית',
      grade1AgeHours: '2',
      grade1MotherBenefitPercent: '0',
      grade1Total: '28',
      slots: makeSlotsBand1(28, 30),
    };
    expect(form.slots[0].jobBase).toBe('30');
    expect(computeGradeJobPercentString(form, 1)).toBe('100');
  });

  it('base job just above 79% threshold → eligible rate', () => {
    const form = baseForm({
      grade1Total: '23.71',
      slots: makeSlotsBand1(23.71, 30),
    });
    expect(computeGradeBaseJobPercentNumber(form, 1)).toBeGreaterThan(79);
    expect(computeMotherBenefitPercentString(form, 1)).toBe('10');
  });

  it('base job exactly at 79% threshold → 0', () => {
    const form = baseForm({
      grade1Total: '23.7',
      slots: makeSlotsBand1(23.7, 30),
    });
    expect(computeGradeBaseJobPercentNumber(form, 1)).toBe(79);
    expect(computeMotherBenefitPercentString(form, 1)).toBe('0');
  });

  it('eligible child only in slot 10 → rate applies', () => {
    const form = baseForm({
      childBirthDate1: '',
      childBirthDate10: '2012-03-15',
    });
    expect(computeMotherBenefitPercentString(form, 1)).toBe('10');
  });

  it('first child too old but second eligible → rate applies', () => {
    const form = baseForm({
      childBirthDate1: '2010-08-31',
      childBirthDate2: '2012-06-01',
      grade1GradeName: 'עוז לתמורה',
    });
    expect(computeMotherBenefitPercentString(form, 1)).toBe('7');
  });

  it('child born day after ref date birthday still age 14 → eligible', () => {
    expect(ageInFullYearsAtDate('2010-09-02', refDate)).toBe(14);
    const form = baseForm({ childBirthDate1: '2010-09-02' });
    expect(computeMotherBenefitPercentString(form, 1)).toBe('10');
  });

  it('child turning 15 on ref date → 0', () => {
    expect(ageInFullYearsAtDate('2010-09-01', refDate)).toBe(15);
    const form = baseForm({ childBirthDate1: '2010-09-01' });
    expect(computeMotherBenefitPercentString(form, 1)).toBe('0');
  });

  it('english gender "female" → eligible when other conditions met', () => {
    const form = baseForm({ employeeGender: 'female' });
    expect(computeMotherBenefitPercentString(form, 1)).toBe('10');
  });

  it('band 2 mother benefit with eligible child → rate', () => {
    const slots = makeSlotsBand1(30, 30);
    slots[6] = {
      ...slots[6],
      institutionSymbol: 'G-2',
      weeklyHours: '20',
      jobBase: '20',
    };
    const form = baseForm({
      grade2GradeName: 'אופק חדש',
      grade2Total: '20',
      grade2Role: 'גננת',
      grade2Grade: 'ב',
      grade2Seniority: '1',
      slots,
    });
    expect(computeMotherBenefitPercentString(form, 2)).toBe('10');
  });

  it('harmonic base job from multiple slots affects threshold', () => {
    const slots = makeSlotsBand1(15, 30);
    slots[1] = {
      ...slots[1],
      institutionSymbol: 'G-1b',
      weeklyHours: '15',
      jobBase: '30',
    };
    const form = baseForm({
      grade1Total: '30',
      slots,
    });
    expect(computeGradeBaseJobPercentNumber(form, 1)).toBe(100);
    expect(computeMotherBenefitPercentString(form, 1)).toBe('10');
  });

  it('no child birth dates at all → 0', () => {
    const form = baseForm({
      childBirthDate1: '',
    });
    expect(employmentFormHasChildUpToAgeInclusive(form, 14, refDate)).toBe(false);
    expect(computeMotherBenefitPercentString(form, 1)).toBe('0');
  });

  it('missing academic year still uses ref date for age boundary', () => {
    const form = baseForm({
      academicYear: '',
      childBirthDate1: '2010-09-01',
    });
    const fallbackRef = academicYearStartRefDate('');
    expect(ageInFullYearsAtDate('2010-09-01', fallbackRef)).toBeGreaterThan(14);
    expect(computeMotherBenefitPercentString(form, 1)).toBe('0');
  });
});
