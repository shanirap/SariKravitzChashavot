// @vitest-environment jsdom
import { describe, it, expect, vi, beforeEach } from 'vitest';

const mockGet = vi.fn();
const mockPost = vi.fn();
const requestHandlers = [];
const responseHandlers = [];
const mockClearStoredAuth = vi.fn();
const mockGetToken = vi.fn(() => 'jwt-token');

vi.mock('axios', () => ({
  default: {
    create: (defaults) => {
      const instance = {
        defaults: { headers: defaults?.headers ?? {} },
        get: mockGet,
        post: mockPost,
        interceptors: {
          request: {
            use: (fn) => {
              requestHandlers.push(fn);
              return 0;
            },
          },
          response: {
            use: (_ok, onRejected) => {
              responseHandlers.push(onRejected);
              return 0;
            },
          },
        },
      };
      return instance;
    },
  },
}));

vi.mock('./auth/authStorage', () => ({
  getToken: () => mockGetToken(),
  clearStoredAuth: (...args) => mockClearStoredAuth(...args),
}));

await import('./api.js');

function runRequestInterceptor(config) {
  expect(requestHandlers.length).toBeGreaterThan(0);
  return requestHandlers[0]({ headers: {}, ...config });
}

async function runResponseError(status, url = '/employers') {
  const err = { response: { status }, config: { url, headers: {} } };
  await expect(responseHandlers[0](err)).rejects.toBe(err);
}

describe('api interceptors', () => {
  beforeEach(() => {
    mockClearStoredAuth.mockClear();
    mockGetToken.mockReturnValue('jwt-token');
    vi.stubGlobal('alert', vi.fn());
    Object.defineProperty(window, 'location', {
      value: { pathname: '/employers', replace: vi.fn() },
      writable: true,
      configurable: true,
    });
  });

  it('adds Bearer token for non-login requests', () => {
    const cfg = runRequestInterceptor({ url: '/employers' });
    expect(cfg.headers.Authorization).toBe('Bearer jwt-token');
  });

  it('does not add Bearer token on login request', () => {
    const cfg = runRequestInterceptor({ url: '/auth/login' });
    expect(cfg.headers.Authorization).toBeUndefined();
  });

  it('removes Content-Type for FormData uploads', () => {
    const form = new FormData();
    form.append('file', new File(['x'], 'a.xlsx'));
    const cfg = runRequestInterceptor({
      url: '/payroll-monthly-inputs/import',
      data: form,
      headers: { 'Content-Type': 'application/json' },
    });
    expect(cfg.headers['Content-Type']).toBeUndefined();
    expect(cfg.headers['content-type']).toBeUndefined();
  });

  it('401 clears auth and redirects away from login page', async () => {
    await runResponseError(401, '/reports/kindergarten-annual');
    expect(mockClearStoredAuth).toHaveBeenCalledTimes(1);
    expect(window.location.replace).toHaveBeenCalledWith('/login');
  });

  it('403 shows permission alert', async () => {
    await runResponseError(403, '/employers');
    expect(window.alert).toHaveBeenCalledWith(
      expect.stringContaining('אין לך הרשאה'),
    );
  });
});
