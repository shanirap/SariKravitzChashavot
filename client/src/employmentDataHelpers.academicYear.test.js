import { describe, it, expect } from 'vitest';
import {
  parseSeptemberGregorianYear,
  academicYearStartRefDate,
  employmentFormHasChildUpToAgeInclusive,
} from './employmentDataHelpers.jsx';

describe('parseSeptemberGregorianYear', () => {
  it('parses תשפ"ו to 2025', () => {
    expect(parseSeptemberGregorianYear('תשפ"ו')).toBe(2025);
  });

  it('returns null for empty or invalid input', () => {
    expect(parseSeptemberGregorianYear('')).toBeNull();
    expect(parseSeptemberGregorianYear(null)).toBeNull();
    expect(parseSeptemberGregorianYear('xyz123')).toBeNull();
  });
});

describe('academicYearStartRefDate', () => {
  it('returns September 1 of parsed school year', () => {
    const d = academicYearStartRefDate('תשפ"ו');
    expect(d.getFullYear()).toBe(2025);
    expect(d.getMonth()).toBe(8);
    expect(d.getDate()).toBe(1);
  });

  it('falls back to current academic year when missing', () => {
    const d = academicYearStartRefDate('');
    expect(d.getMonth()).toBe(8);
    expect(d.getDate()).toBe(1);
  });
});

describe('employmentFormHasChildUpToAgeInclusive', () => {
  const refDate = new Date(2025, 8, 1, 12, 0, 0);

  it('returns true when at least one child is age 14 at ref date', () => {
    const form = { childBirthDate1: '2011-09-01' };
    expect(employmentFormHasChildUpToAgeInclusive(form, 14, refDate)).toBe(true);
  });

  it('returns false when all children are older than 14', () => {
    const form = { childBirthDate1: '2010-08-31', childBirthDate2: '2005-01-01' };
    expect(employmentFormHasChildUpToAgeInclusive(form, 14, refDate)).toBe(false);
  });

  it('returns false when no child birth dates provided', () => {
    expect(employmentFormHasChildUpToAgeInclusive({}, 14, refDate)).toBe(false);
  });
});
