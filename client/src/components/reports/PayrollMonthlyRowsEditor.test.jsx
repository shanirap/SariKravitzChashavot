// @vitest-environment jsdom
import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { act } from 'react';
import { createRoot } from 'react-dom/client';
import PayrollMonthlyRowsEditor from './PayrollMonthlyRowsEditor.jsx';

const mockGetRows = vi.fn();
const mockUpdateRow = vi.fn();

vi.mock('bootstrap', () => ({
  Modal: class {
    show() {}
    hide() {}
    dispose() {}
  },
}));

vi.mock('../../api', () => ({
  payrollMonthlyInputsApi: {
    getRows: (...args) => mockGetRows(...args),
    updateRow: (...args) => mockUpdateRow(...args),
  },
}));

const sampleRows = [
  {
    id: 1,
    batchId: 10,
    idNumber: '111222333',
    fullName: 'Worker One',
    role: 'גננת',
    isManualEdited: false,
  },
];

function renderEditor(onSaved = vi.fn()) {
  const host = document.createElement('div');
  document.body.appendChild(host);
  const root = createRoot(host);
  const onClose = vi.fn();
  act(() => {
    root.render(
      <PayrollMonthlyRowsEditor
        employerId={5}
        academicYear='תשפ"ו'
        month={9}
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

describe('PayrollMonthlyRowsEditor', () => {
  let root;

  beforeEach(() => {
    vi.clearAllMocks();
    mockGetRows.mockResolvedValue({ data: sampleRows });
    mockUpdateRow.mockResolvedValue({
      data: { ...sampleRows[0], fullName: 'Updated', isManualEdited: true },
    });
  });

  afterEach(() => {
    act(() => {
      root?.unmount();
    });
    document.body.innerHTML = '';
  });

  it('loads and displays rows from getRows', async () => {
    ({ root } = renderEditor());
    await flush();

    await vi.waitFor(() => {
      expect(mockGetRows).toHaveBeenCalledWith(5, 'תשפ"ו', 9);
      expect(document.body.textContent).toContain('Worker One');
      expect(document.body.textContent).toContain('111222333');
    });
  });

  it('shows empty state when no rows returned', async () => {
    mockGetRows.mockResolvedValueOnce({ data: [] });
    ({ root } = renderEditor());
    await flush();

    await vi.waitFor(() => {
      expect(document.body.textContent).toContain('אין שורות לחודש זה');
    });
  });

  it('shows manual-edit badge for edited rows', async () => {
    mockGetRows.mockResolvedValueOnce({
      data: [{ ...sampleRows[0], isManualEdited: true }],
    });
    ({ root } = renderEditor());
    await flush();

    await vi.waitFor(() => {
      expect(document.body.textContent).toContain('נערך');
    });
  });

  it('load failure shows error and retry triggers reload', async () => {
    mockGetRows
      .mockRejectedValueOnce({ response: { data: { message: 'טעינה נכשלה' } } })
      .mockResolvedValueOnce({ data: sampleRows });
    ({ root } = renderEditor());
    await flush();

    await vi.waitFor(() => {
      expect(document.body.textContent).toContain('טעינה נכשלה');
    });

    const retryBtn = [...document.querySelectorAll('button')].find((b) =>
      b.textContent.includes('נסה שוב'),
    );
    await act(async () => {
      retryBtn.click();
    });
    await flush();

    await vi.waitFor(() => {
      expect(mockGetRows.mock.calls.length).toBeGreaterThanOrEqual(2);
      expect(document.body.textContent).toContain('Worker One');
    });
  });

  it('save failure shows error alert without calling onSaved', async () => {
    mockUpdateRow.mockRejectedValueOnce({
      response: { data: { message: 'שמירה נכשלה' } },
    });
    const onSaved = vi.fn();
    ({ root } = renderEditor(onSaved));
    await flush();

    await vi.waitFor(() => {
      expect(document.body.textContent).toContain('עריכה');
    });

    const editBtn = [...document.querySelectorAll('button')].find((b) =>
      b.textContent.includes('עריכה'),
    );
    await act(async () => {
      editBtn.click();
    });
    await flush();

    const saveBtn = [...document.querySelectorAll('button')].find((b) =>
      b.textContent.includes('שמור'),
    );
    await act(async () => {
      saveBtn.click();
    });
    await flush();

    await vi.waitFor(() => {
      expect(document.body.textContent).toContain('שמירה נכשלה');
      expect(onSaved).not.toHaveBeenCalled();
    });
  });

  it('opening edit and submitting calls updateRow and onSaved', async () => {
    const onSaved = vi.fn();
    ({ root } = renderEditor(onSaved));
    await flush();

    await vi.waitFor(() => {
      expect(document.body.textContent).toContain('עריכה');
    });

    const editBtn = [...document.querySelectorAll('button')].find((b) =>
      b.textContent.includes('עריכה'),
    );
    await act(async () => {
      editBtn.click();
    });
    await flush();

    await vi.waitFor(() => {
      expect(document.body.textContent).toContain('שמור');
    });

    const saveBtn = [...document.querySelectorAll('button')].find((b) =>
      b.textContent.includes('שמור'),
    );
    await act(async () => {
      saveBtn.click();
    });
    await flush();

    await vi.waitFor(() => {
      expect(mockUpdateRow).toHaveBeenCalledWith(1, expect.any(Object));
      expect(onSaved).toHaveBeenCalled();
      expect(mockGetRows.mock.calls.length).toBeGreaterThanOrEqual(2);
    });
  });
});
