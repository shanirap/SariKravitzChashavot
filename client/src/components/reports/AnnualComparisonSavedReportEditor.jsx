import { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import { Modal as BSModal } from 'bootstrap';
import { reportsApi } from '../../api';
import { parseApiErrorMessage } from '../../utils/apiErrorMessage';
import { formatDateDDMMYYYYForFilename } from '../../utils/dateFormat';
import './AnnualComparisonSavedReportEditor.css';

const ACTIONS_COL_KEY = '__actions__';
const MIN_COL_WIDTH = 56;
const DEFAULT_STATIC_WIDTHS = {
  institutionSymbol: 100,
  fullName: 160,
  role: 130,
  sugMisraFromPayroll: 150,
  grade: 72,
  seniority: 72,
  weeklyHours: 72,
  jobBase: 96,
  jobPercent: 96,
  doubleGeneral: 96,
};

const STATIC_FIELDS = [
  { key: 'institutionSymbol', label: 'סמל מוסד' },
  { key: 'fullName', label: 'שם' },
  { key: 'role', label: 'תפקיד' },
  { key: 'sugMisraFromPayroll', label: 'סוג משרה (מעוקץ)' },
  { key: 'grade', label: 'דרגה' },
  { key: 'seniority', label: 'ותק' },
  { key: 'weeklyHours', label: 'ש"ש' },
  { key: 'jobBase', label: 'בסיס משרה' },
  { key: 'jobPercent', label: 'אחוז משרה' },
  { key: 'doubleGeneral', label: 'הכפלה כללית' },
];

function buildDefaultColumnWidths(monthHeaders) {
  const widths = { [ACTIONS_COL_KEY]: 100 };
  STATIC_FIELDS.forEach(({ key }) => {
    widths[key] = DEFAULT_STATIC_WIDTHS[key] ?? 100;
  });
  monthHeaders.forEach((h) => {
    widths[h] = 240;
  });
  return widths;
}

function startColumnResize(columnKey, getWidth, setWidth, event) {
  event.preventDefault();
  event.stopPropagation();

  const startX = event.clientX;
  const startWidth = getWidth(columnKey);
  const resizer = event.currentTarget;
  resizer.classList.add('is-active');

  const onMove = (moveEvent) => {
    const delta = startX - moveEvent.clientX;
    setWidth(columnKey, Math.max(MIN_COL_WIDTH, startWidth + delta));
  };

  const onUp = () => {
    resizer.classList.remove('is-active');
    document.removeEventListener('mousemove', onMove);
    document.removeEventListener('mouseup', onUp);
  };

  document.addEventListener('mousemove', onMove);
  document.addEventListener('mouseup', onUp);
}

function ReportCellTextarea({ value, title, disabled, onChange }) {
  return (
    <textarea
      className="annual-report-editor-cell-input"
      rows={1}
      value={value}
      title={title ?? value}
      disabled={disabled}
      onChange={onChange}
    />
  );
}

function ResizableHeader({ columnKey, label, width, onResize }) {
  return (
    <th scope="col" style={{ width, minWidth: width, maxWidth: width }}>
      <span className="d-block text-truncate" title={label}>{label}</span>
      <span
        className="annual-report-editor-col-resizer"
        role="separator"
        aria-orientation="vertical"
        aria-label={`שינוי רוחב עמודה ${label}`}
        onMouseDown={(e) => onResize(columnKey, e)}
      />
    </th>
  );
}

function downloadBlob(filename, blob) {
  const url = URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = filename;
  document.body.appendChild(a);
  a.click();
  a.remove();
  URL.revokeObjectURL(url);
}

function filenameFromContentDisposition(cd, fallback) {
  if (!cd || typeof cd !== 'string') return fallback;
  const star = /filename\*=UTF-8''([^;\s]+)/i.exec(cd);
  const quoted = /filename="([^"]+)"/i.exec(cd);
  const raw = star?.[1] ?? quoted?.[1];
  if (!raw) return fallback;
  try {
    return decodeURIComponent(raw.replace(/\+/g, '%20'));
  } catch {
    return raw;
  }
}

function fieldDisplay(field) {
  if (!field) return '';
  return field.display ?? field.computed ?? '';
}

function rowToEditable(row) {
  const staticValues = {};
  STATIC_FIELDS.forEach(({ key }) => {
    staticValues[key] = fieldDisplay(row[key]);
  });
  const monthCells = {};
  if (row.monthCells) {
    Object.entries(row.monthCells).forEach(([monthKey, field]) => {
      monthCells[monthKey] = fieldDisplay(field);
    });
  }
  return {
    slotId: row.slotId,
    gradeBand: row.gradeBand,
    staticValues,
    monthCells,
    isManualEdited: row.isManualEdited,
    manualEditNote: row.manualEditNote ?? '',
  };
}

function rowHasChanges(original, edited) {
  for (const { key } of STATIC_FIELDS) {
    if ((edited.staticValues[key] ?? '') !== fieldDisplay(original[key])) return true;
  }
  const monthKeys = new Set([
    ...Object.keys(original.monthCells ?? {}),
    ...Object.keys(edited.monthCells ?? {}),
  ]);
  for (const monthKey of monthKeys) {
    const orig = fieldDisplay(original.monthCells?.[monthKey]);
    const edit = edited.monthCells?.[monthKey] ?? '';
    if (edit !== orig) return true;
  }
  return false;
}

function editedToSavePayload(edited) {
  return {
    slotId: edited.slotId,
    institutionSymbol: edited.staticValues.institutionSymbol || null,
    fullName: edited.staticValues.fullName || null,
    role: edited.staticValues.role || null,
    sugMisraFromPayroll: edited.staticValues.sugMisraFromPayroll || null,
    grade: edited.staticValues.grade || null,
    seniority: edited.staticValues.seniority || null,
    weeklyHours: parseOptionalDecimal(edited.staticValues.weeklyHours),
    jobBase: parseOptionalDecimal(edited.staticValues.jobBase),
    jobPercent: parseOptionalDecimal(edited.staticValues.jobPercent),
    doubleGeneral: parseOptionalDecimal(edited.staticValues.doubleGeneral),
    monthCells: edited.monthCells,
    manualEditNote: null,
  };
}

function parseOptionalDecimal(value) {
  const trimmed = String(value ?? '').trim();
  if (!trimmed) return null;
  const n = Number(trimmed.replace(',', '.'));
  return Number.isFinite(n) ? n : null;
}

function cellClassName(isOverridden) {
  return isOverridden ? 'table-warning' : '';
}

/**
 * עורך דוח השוואה שנתי — עריכת שורות מלאות לפני ייצוא.
 */
export default function AnnualComparisonSavedReportEditor({
  employerId,
  academicYear,
  onClose,
  onSaved,
}) {
  const modalRef = useRef(null);
  const bsModal = useRef(null);

  const [preview, setPreview] = useState(null);
  const [editedRows, setEditedRows] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [alert, setAlert] = useState(null);
  const [saving, setSaving] = useState(false);
  const [exporting, setExporting] = useState(false);
  const [clearing, setClearing] = useState(false);

  const showAlert = useCallback((type, msg) => {
    setAlert({ type, msg });
    setTimeout(() => setAlert(null), 4500);
  }, []);

  const loadPreview = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const res = await reportsApi.annualComparisonSavedPreview(employerId, academicYear.trim());
      const data = res.data ?? {};
      setPreview(data);
      setEditedRows((Array.isArray(data.rows) ? data.rows : []).map(rowToEditable));
    } catch (err) {
      setPreview(null);
      setEditedRows([]);
      const msg = await parseApiErrorMessage(err, 'שגיאה בטעינת תצוגת הדוח.');
      setError(msg);
    } finally {
      setLoading(false);
    }
  }, [employerId, academicYear]);

  useEffect(() => {
    loadPreview();
  }, [loadPreview]);

  useEffect(() => {
    const el = modalRef.current;
    if (!el) return undefined;

    bsModal.current = new BSModal(el, { backdrop: 'static', keyboard: true });
    const handleHidden = () => onClose?.();
    el.addEventListener('hidden.bs.modal', handleHidden);
    bsModal.current.show();

    return () => {
      el.removeEventListener('hidden.bs.modal', handleHidden);
      bsModal.current?.dispose();
      bsModal.current = null;
    };
  }, [onClose]);

  const monthHeaders = useMemo(
    () => (Array.isArray(preview?.monthHeaders) ? preview.monthHeaders : []),
    [preview],
  );

  const [columnWidths, setColumnWidths] = useState(() => buildDefaultColumnWidths([]));

  useEffect(() => {
    setColumnWidths((prev) => {
      const defaults = buildDefaultColumnWidths(monthHeaders);
      const next = { ...defaults };
      Object.keys(prev).forEach((key) => {
        if (key in next) next[key] = prev[key];
      });
      return next;
    });
  }, [monthHeaders]);

  const getColumnWidth = useCallback(
    (key) => columnWidths[key] ?? MIN_COL_WIDTH,
    [columnWidths],
  );

  const setColumnWidth = useCallback((key, width) => {
    setColumnWidths((prev) => ({ ...prev, [key]: width }));
  }, []);

  const handleColumnResize = useCallback(
    (columnKey, event) => {
      startColumnResize(columnKey, getColumnWidth, setColumnWidth, event);
    },
    [getColumnWidth, setColumnWidth],
  );

  const allColumnKeys = useMemo(
    () => [ACTIONS_COL_KEY, ...STATIC_FIELDS.map((f) => f.key), ...monthHeaders],
    [monthHeaders],
  );

  const dirtyRows = useMemo(() => {
    if (!preview?.rows) return [];
    return preview.rows
      .map((orig, idx) => ({ orig, edited: editedRows[idx] }))
      .filter(({ orig, edited }) => edited && rowHasChanges(orig, edited))
      .map(({ edited }) => editedToSavePayload(edited));
  }, [preview, editedRows]);

  const updateStaticCell = (rowIndex, fieldKey, value) => {
    setEditedRows((prev) => {
      const next = [...prev];
      const row = { ...next[rowIndex], staticValues: { ...next[rowIndex].staticValues } };
      row.staticValues[fieldKey] = value;
      next[rowIndex] = row;
      return next;
    });
  };

  const updateMonthCell = (rowIndex, monthKey, value) => {
    setEditedRows((prev) => {
      const next = [...prev];
      const row = { ...next[rowIndex], monthCells: { ...next[rowIndex].monthCells } };
      row.monthCells[monthKey] = value;
      next[rowIndex] = row;
      return next;
    });
  };

  const handleSave = async () => {
    if (dirtyRows.length === 0) {
      showAlert('info', 'אין שינויים לשמירה.');
      return;
    }
    setSaving(true);
    try {
      await reportsApi.saveAnnualComparisonOverrides(employerId, academicYear.trim(), dirtyRows);
      showAlert('success', 'השינויים נשמרו.');
      await loadPreview();
      onSaved?.();
    } catch (err) {
      const msg = await parseApiErrorMessage(err, 'שגיאה בשמירת העריכות.');
      showAlert('danger', msg);
    } finally {
      setSaving(false);
    }
  };

  const handleClearRow = async (slotId) => {
    setClearing(true);
    try {
      await reportsApi.clearAnnualComparisonOverrides(employerId, academicYear.trim(), slotId);
      showAlert('success', 'השורה אופסה.');
      await loadPreview();
      onSaved?.();
    } catch (err) {
      const msg = await parseApiErrorMessage(err, 'שגיאה באיפוס השורה.');
      showAlert('danger', msg);
    } finally {
      setClearing(false);
    }
  };

  const handleClearAll = async () => {
    if (!window.confirm('לאפס את כל העריכות לדוח לשנה זו?')) return;
    setClearing(true);
    try {
      await reportsApi.clearAnnualComparisonOverrides(employerId, academicYear.trim());
      showAlert('success', 'כל העריכות אופסו.');
      await loadPreview();
      onSaved?.();
    } catch (err) {
      const msg = await parseApiErrorMessage(err, 'שגיאה באיפוס הדוח.');
      showAlert('danger', msg);
    } finally {
      setClearing(false);
    }
  };

  const handleExport = async () => {
    setExporting(true);
    try {
      const res = await reportsApi.annualComparisonSaved(employerId, academicYear.trim());
      const blob = new Blob([res.data], {
        type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
      });
      const suffix = formatDateDDMMYYYYForFilename(new Date());
      const fallback = `השוואה_שנתית_שמור_${employerId}_${suffix}.xlsx`;
      const cd = res.headers['content-disposition'] ?? res.headers['Content-Disposition'];
      const filename = filenameFromContentDisposition(cd, fallback);
      downloadBlob(filename, blob);
      showAlert('success', 'דוח ההשוואה השנתי הורד.');
    } catch (err) {
      const msg = await parseApiErrorMessage(err, 'שגיאה בייצוא הדוח.');
      showAlert('danger', msg);
    } finally {
      setExporting(false);
    }
  };

  const busy = saving || exporting || clearing;

  return (
    <div
      className="modal fade"
      tabIndex={-1}
      ref={modalRef}
      aria-labelledby="annualReportEditorTitle"
      aria-hidden="true"
    >
      <div className="modal-dialog modal-fullscreen">
        <div className="modal-content">
          <div className="modal-header bg-body-tertiary">
            <h5 className="modal-title" id="annualReportEditorTitle">
              <i className="bi bi-table me-2"></i>
              עריכת דוח השוואה שנתי — {academicYear}
            </h5>
            <button
              type="button"
              className="btn-close"
              aria-label="סגור"
              onClick={() => bsModal.current?.hide()}
              disabled={busy}
            ></button>
          </div>
          <div className="modal-body d-flex flex-column gap-3">
            {alert && (
              <div className={`alert alert-${alert.type} alert-dismissible fade show mb-0`}>
                {alert.msg}
                <button
                  type="button"
                  className="btn-close"
                  onClick={() => setAlert(null)}
                  aria-label="סגור"
                ></button>
              </div>
            )}

            <div className="d-flex flex-wrap gap-2">
              <button
                type="button"
                className="btn btn-primary"
                onClick={handleSave}
                disabled={busy || loading || dirtyRows.length === 0}
              >
                {saving ? (
                  <>
                    <span className="spinner-border spinner-border-sm me-2" role="status"></span>
                    שומר…
                  </>
                ) : (
                  <>
                    <i className="bi bi-save me-2"></i>
                    שמור ({dirtyRows.length})
                  </>
                )}
              </button>
              <button
                type="button"
                className="btn btn-outline-danger"
                onClick={handleClearAll}
                disabled={busy || loading}
              >
                <i className="bi bi-arrow-counterclockwise me-2"></i>
                אפס דוח
              </button>
              <button
                type="button"
                className="btn btn-outline-primary"
                onClick={handleExport}
                disabled={busy || loading}
              >
                {exporting ? (
                  <>
                    <span className="spinner-border spinner-border-sm me-2" role="status"></span>
                    מייצא…
                  </>
                ) : (
                  <>
                    <i className="bi bi-download me-2"></i>
                    ייצוא לאקסל
                  </>
                )}
              </button>
            </div>

            <p className="text-muted small mb-0">
              עריכות בדוח הן דריסת תצוגה בלבד — אינן משנות את נתוני המצבת או את קלט העוקץ החודשי.
              תאים מסומנים בצהוב עברו עריכה ידנית. ניתן לגרור את קצה כותרת העמודה (משמאל) כדי להרחיב ולראות את כל התוכן.
            </p>

            {loading ? (
              <div className="text-center py-5 text-muted">
                <span className="spinner-border spinner-border-sm me-2" role="status"></span>
                טוען תצוגת דוח…
              </div>
            ) : error ? (
              <div className="alert alert-danger">
                {error}
                <button type="button" className="btn btn-sm btn-outline-danger ms-3" onClick={loadPreview}>
                  נסה שוב
                </button>
              </div>
            ) : editedRows.length === 0 ? (
              <div className="text-center py-4 text-muted">אין שורות בדוח.</div>
            ) : (
              <div className="table-responsive flex-grow-1 annual-report-editor-table-wrap">
                <table className="table table-sm table-bordered table-hover mb-0 annual-report-editor-table">
                  <colgroup>
                    {allColumnKeys.map((key) => (
                      <col key={key} style={{ width: getColumnWidth(key) }} />
                    ))}
                  </colgroup>
                  <thead className="table-light sticky-top">
                    <tr>
                      <ResizableHeader
                        columnKey={ACTIONS_COL_KEY}
                        label="פעולות"
                        width={getColumnWidth(ACTIONS_COL_KEY)}
                        onResize={handleColumnResize}
                      />
                      {STATIC_FIELDS.map((f) => (
                        <ResizableHeader
                          key={f.key}
                          columnKey={f.key}
                          label={f.label}
                          width={getColumnWidth(f.key)}
                          onResize={handleColumnResize}
                        />
                      ))}
                      {monthHeaders.map((h) => (
                        <ResizableHeader
                          key={h}
                          columnKey={h}
                          label={h}
                          width={getColumnWidth(h)}
                          onResize={handleColumnResize}
                        />
                      ))}
                    </tr>
                  </thead>
                  <tbody>
                    {editedRows.map((row, rowIndex) => {
                      const origRow = preview?.rows?.[rowIndex];
                      const rowEdited = origRow && rowHasChanges(origRow, row);
                      return (
                        <tr key={row.slotId}>
                          <td className="text-nowrap">
                            {row.isManualEdited || rowEdited ? (
                              <span className="badge bg-warning text-dark me-1">נערך</span>
                            ) : null}
                            <button
                              type="button"
                              className="btn btn-outline-secondary btn-sm"
                              title="אפס שורה"
                              disabled={busy || (!row.isManualEdited && !rowEdited)}
                              onClick={() => handleClearRow(row.slotId)}
                            >
                              <i className="bi bi-arrow-counterclockwise"></i>
                            </button>
                          </td>
                          {STATIC_FIELDS.map(({ key }) => {
                            const origField = origRow?.[key];
                            const isOverridden = origField?.isOverridden
                              || (rowEdited && (row.staticValues[key] ?? '') !== fieldDisplay(origField));
                            const cellValue = row.staticValues[key] ?? '';
                            return (
                              <td key={key} className={cellClassName(isOverridden)}>
                                <ReportCellTextarea
                                  value={cellValue}
                                  title={cellValue}
                                  disabled={busy}
                                  onChange={(e) => updateStaticCell(rowIndex, key, e.target.value)}
                                />
                              </td>
                            );
                          })}
                          {monthHeaders.map((monthKey) => {
                            const origField = origRow?.monthCells?.[monthKey];
                            const isOverridden = origField?.isOverridden
                              || (rowEdited && (row.monthCells[monthKey] ?? '') !== fieldDisplay(origField));
                            const cellValue = row.monthCells[monthKey] ?? '';
                            const computedHint = origField?.computed
                              ? `מחושב: ${origField.computed}`
                              : '';
                            return (
                              <td key={monthKey} className={cellClassName(isOverridden)}>
                                <ReportCellTextarea
                                  value={cellValue}
                                  title={computedHint ? `${cellValue}\n${computedHint}` : cellValue}
                                  disabled={busy}
                                  onChange={(e) => updateMonthCell(rowIndex, monthKey, e.target.value)}
                                />
                              </td>
                            );
                          })}
                        </tr>
                      );
                    })}
                  </tbody>
                </table>
              </div>
            )}
          </div>
          <div className="modal-footer">
            <button
              type="button"
              className="btn btn-secondary"
              onClick={() => bsModal.current?.hide()}
              disabled={busy}
            >
              סגור
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
