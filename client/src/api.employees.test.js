import { describe, it, expect, vi, beforeEach } from 'vitest';

const mockGet = vi.fn();
const mockPost = vi.fn();
const mockPut = vi.fn();
const mockPatch = vi.fn();
const mockDelete = vi.fn();

vi.mock('axios', () => ({
  default: {
    create: () => ({
      get: mockGet,
      post: mockPost,
      put: mockPut,
      patch: mockPatch,
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

const { employeesApi } = await import('./api.js');

describe('employeesApi', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('precreateHint calls GET with employerId and idNumber params', async () => {
    await employeesApi.precreateHint(4, '123456789');
    expect(mockGet).toHaveBeenCalledWith('/employees/precreate-hint', {
      params: { employerId: 4, idNumber: '123456789' },
    });
  });

  it('updateActiveStatus PATCHes active flag', async () => {
    await employeesApi.updateActiveStatus(12, false);
    expect(mockPatch).toHaveBeenCalledWith('/employees/12/active-status', {
      isActive: false,
    });
  });

  it('create POSTs employee payload', async () => {
    const payload = { employerId: 1, idNumber: '999' };
    await employeesApi.create(payload);
    expect(mockPost).toHaveBeenCalledWith('/employees', payload);
  });
});
