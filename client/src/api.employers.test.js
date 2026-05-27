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

const { employersApi } = await import('./api.js');

describe('employersApi', () => {
  beforeEach(() => {
    mockGet.mockReset();
    mockPost.mockReset();
    mockPut.mockReset();
    mockDelete.mockReset();
  });

  it('getAll calls GET /employers with params', async () => {
    await employersApi.getAll({ search: 'גן' });
    expect(mockGet).toHaveBeenCalledWith('/employers', { params: { search: 'גן' } });
  });

  it('getEmployeeByIdNumber encodes special characters in path', async () => {
    await employersApi.getEmployeeByIdNumber(5, '12/34 56');
    expect(mockGet).toHaveBeenCalledWith(
      `/employers/5/employees/by-id-number/${encodeURIComponent('12/34 56')}`,
    );
  });

  it('compareMonthlyPayroll POSTs FormData with blob response', async () => {
    const file = new File(['x'], 'payroll.xlsx');
    await employersApi.compareMonthlyPayroll(9, file);
    const [url, form, config] = mockPost.mock.calls[0];
    expect(url).toBe('/employers/9/comparison/monthly-payroll');
    expect(form).toBeInstanceOf(FormData);
    expect(form.get('file')).toBe(file);
    expect(config).toEqual({ responseType: 'blob' });
  });

  it('exportFullExcel requests blob from export endpoint', async () => {
    await employersApi.exportFullExcel(3);
    expect(mockGet).toHaveBeenCalledWith('/employers/3/export/excel', {
      responseType: 'blob',
    });
  });

  it('deleteInstitutionSymbol calls DELETE with symbol id', async () => {
    await employersApi.deleteInstitutionSymbol(2, 77);
    expect(mockDelete).toHaveBeenCalledWith('/employers/2/institution-symbols/77');
  });
});
