// @vitest-environment jsdom
import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { act } from 'react';
import { createRoot } from 'react-dom/client';
import AnnualComparisonSavedPanel from './AnnualComparisonSavedPanel.jsx';

const mockGetYearStatus = vi.fn();
const mockImportMonth = vi.fn();
const mockAnnualComparisonSaved = vi.fn();

vi.mock('bootstrap', () => ({
  Modal: class {
    static getInstance() {
      return null;
    }
    show() {}
    hide() {}
    dispose() {}
  },
}));

vi.mock('./PayrollMonthlyRowsEditor.jsx', () => ({
  default: () => null,
}));

vi.mock('../../employmentDataHelpers', () => ({
  REPORT_ACADEMIC_YEAR_OPTIONS: ['תשפ"ו'],
}));

vi.mock('../../api', () => ({
  payrollMonthlyInputsApi: {
    getYearStatus: (...args) => mockGetYearStatus(...args),
    importMonth: (...args) => mockImportMonth(...args),
  },
  reportsApi: {
    annualComparisonSaved: (...args) => mockAnnualComparisonSaved(...args),
  },
}));

const MONTHS_ORDER = [9, 10, 11, 12, 1, 2, 3, 4, 5, 6, 7, 8];
const MONTH_NAMES = {
  9: 'ספטמבר',
  10: 'אוקטובר',
  11: 'נובמבר',
  12: 'דצמבר',
  1: 'ינואר',
  2: 'פברואר',
  3: 'מרץ',
  4: 'אפריל',
  5: 'מאי',
  6: 'יוני',
  7: 'יולי',
  8: 'אוגוסט',
};

function makeYearStatus() {
  return MONTHS_ORDER.map((month, index) => ({
    month,
    gregorianYear: 2025,
    displayName: MONTH_NAMES[month],
    status: index === 0 ? 'נקלט' : 'חסר',
    rowsCount: index === 0 ? 12 : 0,
    originalFileName: index === 0 ? 'sept.xlsx' : null,
    uploadedAtUtc: index === 0 ? '2025-09-15T10:00:00Z' : null,
  }));
}

function renderPanel(employerId = 7) {
  const host = document.createElement('div');
  document.body.appendChild(host);
  const root = createRoot(host);
  act(() => {
    root.render(<AnnualComparisonSavedPanel employerId={employerId} />);
  });
  return { host, root };
}

async function flushPromises() {
  await act(async () => {
    await Promise.resolve();
  });
}

async function waitForTableRows() {
  await vi.waitFor(() => {
    const rows = document.querySelectorAll('tbody tr');
    expect(rows.length).toBe(12);
  });
}

function getDataRows() {
  return [...document.querySelectorAll('tbody tr')].filter(
    (tr) => !tr.textContent.includes('טוען'),
  );
}

describe('AnnualComparisonSavedPanel', () => {
  let root;

  beforeEach(() => {
    vi.clearAllMocks();
    mockGetYearStatus.mockResolvedValue({ data: makeYearStatus() });
    mockImportMonth.mockResolvedValue({ data: { message: 'הקובץ נקלט בהצלחה.' } });
    mockAnnualComparisonSaved.mockResolvedValue({
      data: new Blob(['xlsx'], {
        type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
      }),
      headers: {},
    });
    global.URL.createObjectURL = vi.fn(() => 'blob:test');
    global.URL.revokeObjectURL = vi.fn();
  });

  afterEach(() => {
    act(() => {
      root?.unmount();
    });
    document.body.innerHTML = '';
  });

  it('loads and displays 12 monthly status rows', async () => {
    ({ root } = renderPanel());
    await flushPromises();
    await waitForTableRows();

    expect(mockGetYearStatus).toHaveBeenCalledWith(7, 'תשפ"ו');
    expect(getDataRows()).toHaveLength(12);
    expect(document.body.textContent).toContain('ספטמבר');
    expect(document.body.textContent).toContain('אוגוסט');
  });

  it('shows upload action for missing month', async () => {
    ({ root } = renderPanel());
    await flushPromises();
    await waitForTableRows();

    const octoberRow = getDataRows().find((tr) => tr.textContent.includes('אוקטובר'));
    expect(octoberRow).toBeTruthy();
    expect(octoberRow.textContent).toContain('חסר');
    expect(octoberRow.querySelector('button')?.textContent).toContain('העלאה');
  });

  it('shows view/edit and replace actions for imported month', async () => {
    ({ root } = renderPanel());
    await flushPromises();
    await waitForTableRows();

    const septemberRow = getDataRows().find((tr) => tr.textContent.includes('ספטמבר'));
    expect(septemberRow).toBeTruthy();
    expect(septemberRow.textContent).toContain('נקלט');
    const labels = [...septemberRow.querySelectorAll('button')].map((b) => b.textContent);
    expect(labels.some((t) => t.includes('צפייה/עריכה'))).toBe(true);
    expect(labels.some((t) => t.includes('החלפת קובץ'))).toBe(true);
  });

  it('calls reportsApi.annualComparisonSaved when generating report', async () => {
    ({ root } = renderPanel(3));
    await flushPromises();
    await waitForTableRows();

    const generateBtn = [...document.querySelectorAll('button')].find((b) =>
      b.textContent.includes('הפק דוח השוואה שנתי'),
    );
    expect(generateBtn).toBeTruthy();

    await act(async () => {
      generateBtn.click();
    });
    await flushPromises();

    await vi.waitFor(() => {
      expect(mockAnnualComparisonSaved).toHaveBeenCalledWith(3, 'תשפ"ו');
    });
  });

  it('refreshes year status after successful upload', async () => {
    ({ root } = renderPanel());
    await flushPromises();
    await waitForTableRows();

    mockGetYearStatus.mockClear();

    const octoberRow = getDataRows().find((tr) => tr.textContent.includes('אוקטובר'));
    const uploadBtn = [...octoberRow.querySelectorAll('button')].find((b) =>
      b.textContent.includes('העלאה'),
    );
    const fileInput = octoberRow.querySelector('input[type="file"]');
    const file = new File(['x'], 'october.xlsx', {
      type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
    });

    await act(async () => {
      uploadBtn.click();
    });

    Object.defineProperty(fileInput, 'files', { value: [file], configurable: true });
    await act(async () => {
      fileInput.dispatchEvent(new Event('change', { bubbles: true }));
    });
    await flushPromises();

    await vi.waitFor(() => {
      expect(mockImportMonth).toHaveBeenCalledWith(7, 'תשפ"ו', 10, file);
      expect(mockGetYearStatus.mock.calls.length).toBeGreaterThanOrEqual(1);
    });
  });
});
