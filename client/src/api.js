import axios from 'axios';
import { clearStoredAuth, getToken } from './auth/authStorage';

/**
 * API base URL resolution:
 * - Set VITE_API_BASE_URL when you need an explicit absolute URL (should end with `/api` or your API prefix).
 * - Production builds default to relative `/api` (same-origin hosting).
 * - Development defaults to `http://localhost:5036/api`; set VITE_API_USE_HTTPS=true for `https://localhost:7068/api`.
 */

function resolveApiBaseUrl() {
  const explicit = import.meta.env.VITE_API_BASE_URL;
  if (typeof explicit === 'string' && explicit.trim()) {
    return explicit.trim().replace(/\/+$/, '');
  }
  if (import.meta.env.PROD) {
    return '/api';
  }
  const useHttps = import.meta.env.VITE_API_USE_HTTPS === 'true';
  return useHttps
    ? 'https://localhost:7068/api'
    : 'http://localhost:5036/api';
}

export const api = axios.create({
  baseURL: resolveApiBaseUrl(),
  headers: {
    'Content-Type': 'application/json',
  },
});

function isAuthLoginRequest(config) {
  const path = config.url ?? '';
  return path.includes('/auth/login') || path.endsWith('auth/login');
}

api.interceptors.request.use((config) => {
  if (!isAuthLoginRequest(config)) {
    const token = getToken();
    if (token) {
      config.headers = config.headers ?? {};
      config.headers.Authorization = `Bearer ${token}`;
    }
  }
  return config;
});

api.interceptors.response.use(
  (res) => res,
  (error) => {
    const status = error?.response?.status;
    const cfg = error?.config;

    if (status === 401 && cfg && !isAuthLoginRequest(cfg)) {
      clearStoredAuth();
      if (typeof window !== 'undefined' && window.location.pathname !== '/login') {
        window.location.replace('/login');
      }
    }

    if (status === 403 && cfg && !isAuthLoginRequest(cfg)) {
      if (typeof window !== 'undefined' && typeof window.alert === 'function') {
        window.alert(
          'אין לך הרשאה לבצע פעולה זו. אם הדבר נדרש — פנה למנהל המערכת.',
        );
      }
    }

    return Promise.reject(error);
  },
);

function triggerBrowserDownload(blob, filename) {
  const url = URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = filename;
  document.body.appendChild(a);
  a.click();
  a.remove();
  URL.revokeObjectURL(url);
}

/** Authenticated Excel template downloads (avoid raw anchor links to the API). */
export const bulkImportApi = {
  /** @param {{ employerId?: number }} [opts] */
  async downloadEmployeesTemplate(opts = {}) {
    const { employerId } = opts;
    const params =
      employerId != null
        ? { includeEmployerName: false, employerId }
        : {};
    const res = await api.get('/bulk-import/template/employees', {
      params,
      responseType: 'blob',
    });
    triggerBrowserDownload(res.data, 'תבנית_ייבוא_עובדים.xlsx');
  },

  async downloadEmployersTemplate() {
    const res = await api.get('/bulk-import/template/employers', {
      responseType: 'blob',
    });
    triggerBrowserDownload(res.data, 'תבנית_ייבוא_מעסיקים.xlsx');
  },
};

export const employersApi = {
  getAll: (params) => api.get('/employers', { params }),
  getById: (id) => api.get(`/employers/${id}`),
  getEmployees: (id, params) => api.get(`/employers/${id}/employees`, { params }),
  getInstitutionSymbols: (id) => api.get(`/employers/${id}/institution-symbols`),
  createInstitutionSymbol: (id, data) =>
    api.post(`/employers/${id}/institution-symbols`, data),
  deleteInstitutionSymbol: (id, symbolId) =>
    api.delete(`/employers/${id}/institution-symbols/${symbolId}`),
  create: (data) => api.post('/employers', data),
  update: (id, data) => api.put(`/employers/${id}`, data),
  delete: (id) => api.delete(`/employers/${id}`),
  exportFullExcel: (id) =>
    api.get(`/employers/${id}/export/excel`, { responseType: 'blob' }),
  compareMonthlyPayroll: (employerId, file) => {
    const fd = new FormData();
    fd.append('file', file);
    return api.post(
      `/employers/${employerId}/comparison/monthly-payroll`,
      fd,
      { responseType: 'blob' },
    );
  },
  getEmployeeByIdNumber: (employerId, idNumber) =>
    api.get(
      `/employers/${employerId}/employees/by-id-number/${encodeURIComponent(idNumber)}`,
    ),
};

export const employeesApi = {
  getById: (id) => api.get(`/employees/${id}`),
  create: (data) => api.post('/employees', data),
  update: (id, data) => api.put(`/employees/${id}`, data),
  updateActiveStatus: (id, isActive) =>
    api.patch(`/employees/${id}/active-status`, { isActive }),
  delete: (id) => api.delete(`/employees/${id}`),
  precreateHint: (employerId, idNumber) =>
    api.get('/employees/precreate-hint', { params: { employerId, idNumber } }),
};

export const reportsApi = {
  /** Returns a blob (responseType: 'blob') for all 7 report endpoints. */
  kindergartenAnnual: (employerId, academicYear) =>
    api.get('/reports/kindergarten-annual', {
      params: { employerId, academicYear },
      responseType: 'blob',
    }),
  schoolAnnual: (employerId, academicYear) =>
    api.get('/reports/school-annual', {
      params: { employerId, academicYear },
      responseType: 'blob',
    }),
  monthlyComparison: (employerId, academicYear, month, file) => {
    const form = new FormData();
    form.append('file', file);
    return api.post(
      `/reports/monthly-comparison?employerId=${encodeURIComponent(employerId)}&academicYear=${encodeURIComponent(academicYear)}&month=${encodeURIComponent(month)}`,
      form,
      { responseType: 'blob' },
    );
  },
  annualComparison: (employerId, academicYear, file) => {
    const form = new FormData();
    form.append('file', file);
    return api.post(
      `/reports/annual-comparison?employerId=${encodeURIComponent(employerId)}&academicYear=${encodeURIComponent(academicYear)}`,
      form,
      { responseType: 'blob' },
    );
  },
  institutionHours: (employerId, academicYear, institutionSymbol) =>
    api.get('/reports/institution-hours', {
      params: { employerId, academicYear, institutionSymbol },
      responseType: 'blob',
    }),
  employeesPersonal: (employerId) =>
    api.get('/reports/employees-personal', {
      params: { employerId },
      responseType: 'blob',
    }),
  employeesEmploymentData: (employerId, academicYear) =>
    api.get('/reports/employees-employment-data', {
      params: { employerId, academicYear },
      responseType: 'blob',
    }),
};

export const employmentDataApi = {
  getByEmployeeAndEmployer: (employeeId, employerId) =>
    api.get(
      `/employment-data/employee/${employeeId}/employer/${employerId}`,
    ),
  create: (data) => api.post('/employment-data', data),
  update: (id, data) => api.put(`/employment-data/${id}`, data),
  delete: (id) => api.delete(`/employment-data/${id}`),
};
