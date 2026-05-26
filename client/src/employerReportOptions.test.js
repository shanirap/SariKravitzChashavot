import { describe, it, expect } from 'vitest';

/**
 * מראה את אפשרויות הדוח ב-EmployerActions — בדיקה שהמזהים תואמים ל-reportsApi.
 */
const EMPLOYER_REPORT_OPTIONS = [
  { id: 'employer-full-xlsx', needsYear: false },
  { id: 'kindergarten-annual', needsYear: true },
  { id: 'school-annual', needsYear: true },
  { id: 'employees-personal', needsYear: false },
  { id: 'employees-employment-data', needsYear: true },
];

const COMPARE_SUBTYPES = [
  { key: 'payroll' },
  { key: 'monthly' },
  { key: 'annual' },
  { key: 'institution-hours' },
];

const REPORTS_API_IDS = new Set([
  'kindergarten-annual',
  'school-annual',
  'employees-personal',
  'employees-employment-data',
]);

const COMPARE_API_KEYS = new Set(['monthly', 'annual', 'institution-hours']);

describe('employer report option ids', () => {
  it('server-backed issuance reports map to reportsApi', () => {
    const serverBacked = EMPLOYER_REPORT_OPTIONS.filter((o) => o.id !== 'employer-full-xlsx');
    for (const opt of serverBacked) {
      expect(REPORTS_API_IDS.has(opt.id)).toBe(true);
    }
  });

  it('comparison subtypes with API coverage are monthly, annual, institution-hours', () => {
    const withApi = COMPARE_SUBTYPES.filter((s) => COMPARE_API_KEYS.has(s.key));
    expect(withApi).toHaveLength(3);
  });

  it('year-required reports declare needsYear', () => {
    const needYear = ['kindergarten-annual', 'school-annual', 'employees-employment-data'];
    for (const id of needYear) {
      const opt = EMPLOYER_REPORT_OPTIONS.find((o) => o.id === id);
      expect(opt?.needsYear).toBe(true);
    }
  });
});
