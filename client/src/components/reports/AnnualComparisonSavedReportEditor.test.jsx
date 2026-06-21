// @vitest-environment jsdom
import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { act } from 'react';
import { createRoot } from 'react-dom/client';
import AnnualComparisonSavedReportEditor from './AnnualComparisonSavedReportEditor.jsx';

const mockPreview = vi.fn();
const mockSaveOverrides = vi.fn();
const mockClearOverrides = vi.fn();
const mockAnnualComparisonSaved = vi.fn();

vi.mock('bootstrap', () => ({
  Modal: class {
    show() {}
    hide() {}
    dispose() {}
  },
}));

vi.mock('../../api', () => ({
  reportsApi: {
    annualComparisonSavedPreview: (...args) => mockPreview(...args),
    saveAnnualComparisonOverrides: (...args) => mockSaveOverrides(...args),
    clearAnnualComparisonOverrides: (...args) => mockClearOverrides(...args),
    annualComparisonSaved: (...args) => mockAnnualComparisonSaved(...args),
  },
}));

const samplePreview = {
  academicYear: 'תשפ"ו',
  monthHeaders: ['9.2025', '10.2025'],
  rows: [
    {
      slotId: 42,
      gradeBand: 1,
      institutionSymbol: { computed: 'SYM-1', display: 'SYM-1', isOverridden: false },
      fullName: { computed: 'Worker Import', display: 'Worker Import', isOverridden: false },
      role: { computed: 'גננת', display: 'גננת', isOverridden: false },
      sugMisraFromPayroll: { computed: '', display: '', isOverridden: false },
      grade: { computed: 'ב', display: 'ב', isOverridden: false },
      seniority: { computed: '5', display: '5', isOverridden: false },
      weeklyHours: { computed: '30', display: '30', isOverridden: false },
      jobBase: { computed: '28', display: '28', isOverridden: false },
      jobPercent: { computed: '100', display: '100', isOverridden: false },
      doubleGeneral: { computed: '0', display: '0', isOverridden: false },
      monthCells: {
        '9.2025': { computed: 'V', display: 'V', isOverridden: false },
        '10.2025': { computed: 'לא נקלט', display: 'לא נקלט', isOverridden: false },
      },
      isManualEdited: false,
    },
  ],
};

function renderEditor(onSaved = vi.fn()) {
  const host = document.createElement('div');
  document.body.appendChild(host);
  const root = createRoot(host);
  const onClose = vi.fn();
  act(() => {
    root.render(
      <AnnualComparisonSavedReportEditor
        employerId={5}
        academicYear='תשפ"ו'
        onClose={onClose}
        onSaved={onSaved}
      />,
    );
  });
  return { host, root, onClose, onSaved };
}

async function flush() {
  await act(async () => {
    await Promise.resolve();
  });
}

describe('AnnualComparisonSavedReportEditor', () => {
  let root;

  beforeEach(() => {
    vi.clearAllMocks();
    mockPreview.mockResolvedValue({ data: samplePreview });
    mockSaveOverrides.mockResolvedValue({ data: { message: 'השינויים נשמרו.' } });
    mockClearOverrides.mockResolvedValue({ data: { message: 'אופס.' } });
    mockAnnualComparisonSaved.mockResolvedValue({
      data: new Blob(['xlsx'], {
        type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
      }),
      headers: {},
    });
    global.URL.createObjectURL = vi.fn(() => 'blob:test');
    global.URL.revokeObjectURL = vi.fn();
    window.confirm = vi.fn(() => true);
  });

  afterEach(() => {
    act(() => {
      root?.unmount();
    });
    document.body.innerHTML = '';
  });

  it('loads preview and displays row data', async () => {
    ({ root } = renderEditor());
    await flush();

    await vi.waitFor(() => {
      expect(mockPreview).toHaveBeenCalledWith(5, 'תשפ"ו');
      expect(document.body.textContent).toContain('9.2025');
      const inputs = document.querySelectorAll('tbody textarea');
      expect(inputs.length).toBeGreaterThan(0);
      expect(inputs[1]?.value).toBe('Worker Import');
    });
  });

  it('save sends only changed rows', async () => {
    ({ root } = renderEditor());
    await flush();

    let nameInput;
    await vi.waitFor(() => {
      nameInput = document.querySelectorAll('tbody textarea')[1];
      expect(nameInput?.value).toBe('Worker Import');
    });

    await act(async () => {
      const nativeInputValueSetter = Object.getOwnPropertyDescriptor(
        window.HTMLTextAreaElement.prototype,
        'value',
      ).set;
      nativeInputValueSetter.call(nameInput, 'שם מעודכן');
      nameInput.dispatchEvent(new Event('input', { bubbles: true }));
      nameInput.dispatchEvent(new Event('change', { bubbles: true }));
    });

    const saveBtn = [...document.querySelectorAll('button')].find((b) =>
      b.textContent.includes('שמור'),
    );
    await act(async () => {
      saveBtn.click();
    });
    await flush();

    await vi.waitFor(() => {
      expect(mockSaveOverrides).toHaveBeenCalled();
      const [, , rows] = mockSaveOverrides.mock.calls[0];
      expect(rows).toHaveLength(1);
      expect(rows[0].slotId).toBe(42);
      expect(rows[0].fullName).toBe('שם מעודכן');
    });
  });

  it('export triggers annualComparisonSaved download', async () => {
    ({ root } = renderEditor());
    await flush();

    await vi.waitFor(() => {
      expect(document.querySelectorAll('tbody textarea').length).toBeGreaterThan(0);
    });

    const exportBtn = [...document.querySelectorAll('button')].find((b) =>
      b.textContent.includes('ייצוא לאקסל'),
    );
    await act(async () => {
      exportBtn.click();
    });
    await flush();

    await vi.waitFor(() => {
      expect(mockAnnualComparisonSaved).toHaveBeenCalledWith(5, 'תשפ"ו');
      expect(global.URL.createObjectURL).toHaveBeenCalled();
    });
  });

  it('shows error when preview fails', async () => {
    mockPreview.mockRejectedValueOnce({
      response: { data: { message: 'תצוגה נכשלה' } },
    });
    ({ root } = renderEditor());
    await flush();

    await vi.waitFor(() => {
      expect(document.body.textContent).toContain('תצוגה נכשלה');
    });
  });

  it('clear all overrides calls clearAnnualComparisonOverrides without slotId', async () => {
    ({ root } = renderEditor());
    await flush();

    await vi.waitFor(() => {
      expect(document.querySelectorAll('tbody textarea').length).toBeGreaterThan(0);
    });

    const clearAllBtn = [...document.querySelectorAll('button')].find((b) =>
      b.textContent.includes('אפס דוח'),
    );
    expect(clearAllBtn).toBeTruthy();

    await act(async () => {
      clearAllBtn.click();
    });
    await flush();

    await vi.waitFor(() => {
      expect(mockClearOverrides).toHaveBeenCalledWith(5, 'תשפ"ו');
    });
  });
});
