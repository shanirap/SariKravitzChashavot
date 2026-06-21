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

const { payrollMonthlyInputsApi } = await import('./api.js');

describe('payrollMonthlyInputsApi', () => {
  beforeEach(() => {
    mockGet.mockReset();
    mockPost.mockReset();
    mockPut.mockReset();
    mockDelete.mockReset();
  });

  it('getYearStatus calls GET with params', async () => {
    await payrollMonthlyInputsApi.getYearStatus(5, 'תשפ"ו');
    expect(mockGet).toHaveBeenCalledWith('/payroll-monthly-inputs/status', {
      params: { employerId: 5, academicYear: 'תשפ"ו' },
    });
  });

  it('importMonth POSTs FormData with query params', async () => {
    const file = new File(['x'], 'okets.xlsx');
    await payrollMonthlyInputsApi.importMonth(7, 'תשפ"ו', 9, file);
    expect(mockPost).toHaveBeenCalledTimes(1);
    const [url, form, config] = mockPost.mock.calls[0];
    expect(url).toBe('/payroll-monthly-inputs/import');
    expect(config).toEqual({
      params: { employerId: 7, academicYear: 'תשפ"ו', month: 9 },
    });
    expect(form).toBeInstanceOf(FormData);
    expect(form.get('file')).toBe(file);
  });

  it('getRows calls GET with employer year and month', async () => {
    await payrollMonthlyInputsApi.getRows(3, 'תשפ"ה', 10);
    expect(mockGet).toHaveBeenCalledWith('/payroll-monthly-inputs/rows', {
      params: { employerId: 3, academicYear: 'תשפ"ה', month: 10 },
    });
  });

  it('updateRow PUTs payload to row path with employerId', async () => {
    const payload = { role: 'גננת', weeklyHours: 30 };
    await payrollMonthlyInputsApi.updateRow(5, 42, payload);
    expect(mockPut).toHaveBeenCalledWith('/payroll-monthly-inputs/rows/42', payload, {
      params: { employerId: 5 },
    });
  });

  it('deleteRow DELETEs row path with employerId', async () => {
    await payrollMonthlyInputsApi.deleteRow(5, 99);
    expect(mockDelete).toHaveBeenCalledWith('/payroll-monthly-inputs/rows/99', {
      params: { employerId: 5 },
    });
  });
});
