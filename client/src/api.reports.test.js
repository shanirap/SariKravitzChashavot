import { describe, it, expect, vi, beforeEach } from 'vitest';

const mockGet = vi.fn();
const mockPost = vi.fn();
const mockPut = vi.fn();
const mockDelete = vi.fn();

vi.mock('axios', () => ({
  default: {
    create: () => ({
      get: mockGet,
      post: mockPost,
      put: mockPut,
      delete: mockDelete,
      interceptors: {
        request: { use: vi.fn() },
        response: { use: vi.fn() },
      },
    }),
  },
}));

vi.mock('./auth/authStorage', () => ({
  getToken: () => 'test-token',
  clearStoredAuth: vi.fn(),
}));

const { reportsApi } = await import('./api.js');

describe('reportsApi', () => {
  beforeEach(() => {
    mockGet.mockReset();
    mockPost.mockReset();
    mockPut.mockReset();
    mockDelete.mockReset();
  });

  it('kindergartenAnnual calls GET with params', async () => {
    await reportsApi.kindergartenAnnual(5, 'תשפ"ו');
    expect(mockGet).toHaveBeenCalledWith('/reports/kindergarten-annual', {
      params: { employerId: 5, academicYear: 'תשפ"ו' },
      responseType: 'blob',
    });
  });

  it('schoolAnnual calls GET with params', async () => {
    await reportsApi.schoolAnnual(3, 'תשפ"ה');
    expect(mockGet).toHaveBeenCalledWith('/reports/school-annual', {
      params: { employerId: 3, academicYear: 'תשפ"ה' },
      responseType: 'blob',
    });
  });

  it('employeesPersonal calls GET with employerId only', async () => {
    await reportsApi.employeesPersonal(10);
    expect(mockGet).toHaveBeenCalledWith('/reports/employees-personal', {
      params: { employerId: 10 },
      responseType: 'blob',
    });
  });

  it('institutionHours encodes symbol in params', async () => {
    await reportsApi.institutionHours(1, 'תשפ"ו', 'G-1/A');
    expect(mockGet).toHaveBeenCalledWith('/reports/institution-hours', {
      params: { employerId: 1, academicYear: 'תשפ"ו', institutionSymbol: 'G-1/A' },
      responseType: 'blob',
    });
  });

  it('institutionHours supports all-symbols sentinel', async () => {
    await reportsApi.institutionHours(1, 'תשפ"ו', '*');
    expect(mockGet).toHaveBeenCalledWith('/reports/institution-hours', {
      params: { employerId: 1, academicYear: 'תשפ"ו', institutionSymbol: '*' },
      responseType: 'blob',
    });
  });

  it('monthlyComparison POSTs FormData with encoded query', async () => {
    const file = new File(['x'], 'payroll.xlsx');
    await reportsApi.monthlyComparison(7, 'תשפ"ו', 9, file);
    expect(mockPost).toHaveBeenCalledTimes(1);
    const [url, form, config] = mockPost.mock.calls[0];
    expect(url).toBe(
      '/reports/monthly-comparison?employerId=7&academicYear=%D7%AA%D7%A9%D7%A4%22%D7%95&month=9',
    );
    expect(form).toBeInstanceOf(FormData);
    expect(form.get('file')).toBe(file);
    expect(config).toEqual({ responseType: 'blob' });
  });

  it('annualComparison POSTs FormData without month param', async () => {
    const file = new File(['x'], 'annual.xlsx');
    await reportsApi.annualComparison(10, 'תשפ"ו', file);
    const [url, form] = mockPost.mock.calls[0];
    expect(url).toContain('employerId=10');
    expect(url).toContain('academicYear=');
    expect(url).not.toContain('month=');
    expect(form.get('file')).toBe(file);
  });

  it('employeesEmploymentData calls GET with year', async () => {
    await reportsApi.employeesEmploymentData(2, 'תשפ"ו');
    expect(mockGet).toHaveBeenCalledWith('/reports/employees-employment-data', {
      params: { employerId: 2, academicYear: 'תשפ"ו' },
      responseType: 'blob',
    });
  });

  it('annualComparisonSaved calls GET with blob response', async () => {
    await reportsApi.annualComparisonSaved(8, 'תשפ"ו');
    expect(mockGet).toHaveBeenCalledWith('/reports/annual-comparison-saved', {
      params: { employerId: 8, academicYear: 'תשפ"ו' },
      responseType: 'blob',
    });
  });

  it('annualComparisonSavedPreview calls GET preview endpoint', async () => {
    await reportsApi.annualComparisonSavedPreview(8, 'תשפ"ו');
    expect(mockGet).toHaveBeenCalledWith('/reports/annual-comparison-saved/preview', {
      params: { employerId: 8, academicYear: 'תשפ"ו' },
    });
  });

  it('saveAnnualComparisonOverrides PUTs rows payload', async () => {
    const rows = [{ slotId: 1, fullName: 'Test' }];
    await reportsApi.saveAnnualComparisonOverrides(8, 'תשפ"ו', rows);
    expect(mockPut).toHaveBeenCalledWith('/reports/annual-comparison-saved/overrides', {
      employerId: 8,
      academicYear: 'תשפ"ו',
      rows,
    });
  });

  it('clearAnnualComparisonOverrides DELETEs with optional slotId', async () => {
    await reportsApi.clearAnnualComparisonOverrides(8, 'תשפ"ו', 42);
    expect(mockDelete).toHaveBeenCalledWith('/reports/annual-comparison-saved/overrides', {
      params: { employerId: 8, academicYear: 'תשפ"ו', slotId: 42 },
    });
  });
});
