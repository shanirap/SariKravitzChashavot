// @vitest-environment jsdom
import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { act } from 'react';
import { createRoot } from 'react-dom/client';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import EmployerDetails from './EmployerDetails.jsx';

const mockGetById = vi.fn();
const mockGetEmployees = vi.fn();
const mockGetInstitutionSymbols = vi.fn();

vi.mock('../api', () => ({
  employersApi: {
    getById: (...args) => mockGetById(...args),
    getEmployees: (...args) => mockGetEmployees(...args),
    getInstitutionSymbols: (...args) => mockGetInstitutionSymbols(...args),
  },
  employeesApi: {
    update: vi.fn(),
    delete: vi.fn(),
    updateActiveStatus: vi.fn(),
  },
}));

function renderPage() {
  const host = document.createElement('div');
  document.body.appendChild(host);
  const root = createRoot(host);
  act(() => {
    root.render(
      <MemoryRouter initialEntries={['/employers/5']}>
        <Routes>
          <Route path="/employers/:id" element={<EmployerDetails />} />
        </Routes>
      </MemoryRouter>,
    );
  });
  return { host, root };
}

async function flush() {
  await act(async () => {
    await Promise.resolve();
  });
}

describe('EmployerDetails employee filters', () => {
  let root;

  beforeEach(() => {
    vi.clearAllMocks();
    mockGetById.mockResolvedValue({ data: { id: 5, name: 'Test Employer' } });
    mockGetInstitutionSymbols.mockResolvedValue({
      data: [
        { id: 1, institutionSymbol: 'SYM-A', institutionSymbolName: 'Garden A' },
        { id: 2, institutionSymbol: 'SYM-B', institutionSymbolName: 'Garden B' },
      ],
    });
    mockGetEmployees.mockResolvedValue({
      data: { items: [], totalCount: 0, page: 1, pageSize: 50 },
    });
  });

  afterEach(() => {
    act(() => {
      root?.unmount();
    });
    document.body.innerHTML = '';
  });

  it('loads employees with default filters', async () => {
    ({ root } = renderPage());
    await flush();

    await vi.waitFor(() => {
      expect(mockGetEmployees).toHaveBeenCalledWith('5', {
        page: 1,
        pageSize: 50,
        search: undefined,
      });
    });
  });

  it('passes isActive when active status filter changes', async () => {
    ({ root } = renderPage());
    await flush();

    await vi.waitFor(() => {
      expect(document.querySelectorAll('select').length).toBeGreaterThan(0);
    });

    const statusSelect = [...document.querySelectorAll('select')].find((s) =>
      [...s.options].some((o) => o.value === 'active'),
    );
    expect(statusSelect).toBeTruthy();

    mockGetEmployees.mockClear();
    await act(async () => {
      statusSelect.value = 'active';
      statusSelect.dispatchEvent(new Event('change', { bubbles: true }));
    });
    await flush();

    await vi.waitFor(() => {
      expect(mockGetEmployees).toHaveBeenCalledWith('5', {
        page: 1,
        pageSize: 50,
        search: undefined,
        isActive: true,
      });
    });
  });

  it('passes institutionSymbol when symbol filter changes', async () => {
    ({ root } = renderPage());
    await flush();

    await vi.waitFor(() => {
      expect(document.querySelectorAll('select').length).toBeGreaterThan(1);
    });

    const symbolSelect = [...document.querySelectorAll('select')].find((s) =>
      [...s.options].some((o) => o.value === 'SYM-A'),
    );
    expect(symbolSelect).toBeTruthy();

    mockGetEmployees.mockClear();
    await act(async () => {
      symbolSelect.value = 'SYM-A';
      symbolSelect.dispatchEvent(new Event('change', { bubbles: true }));
    });
    await flush();

    await vi.waitFor(() => {
      expect(mockGetEmployees).toHaveBeenCalledWith('5', {
        page: 1,
        pageSize: 50,
        search: undefined,
        institutionSymbol: 'SYM-A',
      });
    });
  });

  it('clear filters resets API params', async () => {
    ({ root } = renderPage());
    await flush();

    await vi.waitFor(() => {
      expect(document.querySelectorAll('select').length).toBeGreaterThan(1);
    });

    const statusSelect = [...document.querySelectorAll('select')].find((s) =>
      [...s.options].some((o) => o.value === 'inactive'),
    );
    await act(async () => {
      statusSelect.value = 'inactive';
      statusSelect.dispatchEvent(new Event('change', { bubbles: true }));
    });
    await flush();

    mockGetEmployees.mockClear();
    const clearBtn = [...document.querySelectorAll('button')].find((b) =>
      b.textContent.includes('נקה סינון'),
    );
    expect(clearBtn).toBeTruthy();

    await act(async () => {
      clearBtn.click();
    });
    await flush();

    await vi.waitFor(() => {
      expect(mockGetEmployees).toHaveBeenCalledWith('5', {
        page: 1,
        pageSize: 50,
        search: undefined,
      });
    });
  });
});
