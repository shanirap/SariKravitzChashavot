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

const { employmentDataApi } = await import('./api.js');

describe('employmentDataApi', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('getByEmployeeAndEmployer calls nested path', async () => {
    await employmentDataApi.getByEmployeeAndEmployer(10, 20);
    expect(mockGet).toHaveBeenCalledWith('/employment-data/employee/10/employer/20');
  });

  it('update PUTs to employment-data id', async () => {
    const data = { academicYear: 'תשפ"ו' };
    await employmentDataApi.update(55, data);
    expect(mockPut).toHaveBeenCalledWith('/employment-data/55', data);
  });
});
