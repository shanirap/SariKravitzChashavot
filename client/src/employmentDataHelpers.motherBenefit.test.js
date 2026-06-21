import { describe, it, expect } from 'vitest';
import {
  computeMotherBenefitPercentString,
  computeTrainingFundPercentString,
  emptyEmploymentSlot,
  GRADE_NAMES,
  getJobBaseValue,
  normalizeGradeName,
  UNIFIED_EDUCATION_SUPPORT_GRADE_NAME,
} from './employmentDataHelpers.jsx';

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

describe('computeMotherBenefitPercentString (aligned with server)', () => {
  it('grade names expose the new unified education support label', () => {
    expect(GRADE_NAMES).toContain(UNIFIED_EDUCATION_SUPPORT_GRADE_NAME);
    expect(GRADE_NAMES).not.toContain('אחיד');
  });

  it('female + אחיד/תומכות חינוך + eligible child + base job above 79% → 0', () => {
    const form = baseForm({ grade1GradeName: UNIFIED_EDUCATION_SUPPORT_GRADE_NAME });
    expect(computeMotherBenefitPercentString(form, 1)).toBe('0');
  });

  it('legacy אחיד is treated like אחיד/תומכות חינוך', () => {
    const form = baseForm({
      grade1GradeName: 'אחיד',
      grade1Role: 'סייעת ראשית',
      grade1Seniority: '2',
      grade1JobPercent: '75',
    });

    expect(normalizeGradeName('אחיד')).toBe(UNIFIED_EDUCATION_SUPPORT_GRADE_NAME);
    expect(getJobBaseValue('אחיד', 'סייעת ראשית')).toBe('40');
    expect(computeMotherBenefitPercentString(form, 1)).toBe('0');
    expect(computeTrainingFundPercentString(form, 1)).toBe('7.5');
  });

  it('male employee → 0', () => {
    const form = baseForm({ employeeGender: 'זכר' });
    expect(computeMotherBenefitPercentString(form, 1)).toBe('0');
  });

  it('no eligible child → 0', () => {
    const form = baseForm({ childBirthDate1: '' });
    expect(computeMotherBenefitPercentString(form, 1)).toBe('0');
  });

  it('child over 14 at academic year start → 0', () => {
    const form = baseForm({ childBirthDate1: '2010-08-31' });
    expect(computeMotherBenefitPercentString(form, 1)).toBe('0');
  });

  it('base job at or below 79% → 0', () => {
    const form = baseForm({
      grade1Total: '20',
      slots: makeSlotsBand1(20, 30),
    });
    expect(computeMotherBenefitPercentString(form, 1)).toBe('0');
  });

  it('no usable base job percent (empty slots) → 0', () => {
    const form = baseForm({
      grade1Total: '',
      slots: Array.from({ length: 12 }, (_, i) =>
        emptyEmploymentSlot(i < 6 ? 1 : 2, (i % 6) + 1),
      ),
    });
    expect(computeMotherBenefitPercentString(form, 1)).toBe('0');
  });

  it('unknown grade name → empty string', () => {
    const form = baseForm({ grade1GradeName: 'לא קיים' });
    expect(computeMotherBenefitPercentString(form, 1)).toBe('');
  });

  it('יסודי וגנים above threshold with eligible child → 10', () => {
    const form = baseForm({ grade1GradeName: 'יסודי וגנים' });
    expect(computeMotherBenefitPercentString(form, 1)).toBe('10');
  });

  it('עוז לתמורה above threshold with eligible child → 7', () => {
    const form = baseForm({ grade1GradeName: 'עוז לתמורה' });
    expect(computeMotherBenefitPercentString(form, 1)).toBe('7');
  });

  it('אופק חדש above threshold with eligible child → 10', () => {
    const form = baseForm({ grade1GradeName: 'אופק חדש' });
    expect(computeMotherBenefitPercentString(form, 1)).toBe('10');
  });

  it('child exactly 14 at academic year start → eligible rate', () => {
    const form = baseForm({ childBirthDate1: '2011-09-01' });
    expect(computeMotherBenefitPercentString(form, 1)).toBe('10');
  });

  it('missing academicYear uses current year ref for child age', () => {
    const form = baseForm({ academicYear: '', childBirthDate1: '2012-03-15' });
    expect(computeMotherBenefitPercentString(form, 1)).toBe('10');
  });
});
