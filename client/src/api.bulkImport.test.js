// @vitest-environment jsdom
import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';

const mockGet = vi.fn();

vi.mock('axios', () => ({
  default: {
    create: () => ({
      get: mockGet,
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

const { bulkImportApi } = await import('./api.js');

describe('bulkImportApi', () => {
  let appendChildSpy;
  let clickSpy;

  beforeEach(() => {
    mockGet.mockReset();
    mockGet.mockResolvedValue({ data: new Blob(['xlsx']) });
    appendChildSpy = vi.spyOn(document.body, 'appendChild').mockImplementation(() => {});
    clickSpy = vi.spyOn(HTMLAnchorElement.prototype, 'click').mockImplementation(() => {});
    URL.createObjectURL = vi.fn(() => 'blob:test');
    URL.revokeObjectURL = vi.fn();
  });

  afterEach(() => {
    appendChildSpy.mockRestore();
    clickSpy.mockRestore();
  });

  it('downloadEmployersTemplate triggers Hebrew filename download', async () => {
    await bulkImportApi.downloadEmployersTemplate();
    expect(mockGet).toHaveBeenCalledWith('/bulk-import/template/employers', {
      responseType: 'blob',
    });
    expect(clickSpy).toHaveBeenCalled();
  });

  it('downloadEmployeesTemplate passes employerId when provided', async () => {
    await bulkImportApi.downloadEmployeesTemplate({ employerId: 8 });
    expect(mockGet).toHaveBeenCalledWith('/bulk-import/template/employees', {
      params: { includeEmployerName: false, employerId: 8 },
      responseType: 'blob',
    });
  });
});
