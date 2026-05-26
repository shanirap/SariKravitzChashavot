import { useCallback, useEffect, useRef, useState } from 'react';
import { payrollMonthlyInputsApi, reportsApi } from '../../api';
import { REPORT_ACADEMIC_YEAR_OPTIONS } from '../../employmentDataHelpers';
import { parseApiErrorMessage } from '../../utils/apiErrorMessage';
import { formatDateDDMMYYYYForFilename } from '../../utils/dateFormat';
import PayrollMonthlyRowsEditor from './PayrollMonthlyRowsEditor';

const ACADEMIC_YEAR_OPTIONS = REPORT_ACADEMIC_YEAR_OPTIONS;
const XLSX_ACCEPT = '.xlsx,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet';

const STATUS_CAPTURED = 'נקלט';
const STATUS_MISSING = 'חסר';

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

function formatUploadedAt(utc) {
  if (!utc) return '—';
  const d = new Date(utc);
  if (Number.isNaN(d.getTime())) return '—';
  return d.toLocaleString('he-IL', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
}

function statusBadgeClass(status) {
  if (status === STATUS_CAPTURED) return 'bg-success';
  if (status === STATUS_MISSING) return 'bg-secondary';
  return 'bg-light text-dark border';
}

function isXlsxFile(file) {
  if (!file) return false;
  const name = file.name?.toLowerCase() ?? '';
  if (name.endsWith('.xlsx')) return true;
  const type = file.type?.toLowerCase() ?? '';
  return (
    type === 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
    || type === 'application/vnd.ms-excel'
  );
}

/**
 * ניהול קליטת עוקץ חודשי והפקת דוח השוואה שנתי משמורים.
 * @param {{ employerId: number }} props
 */
export default function AnnualComparisonSavedPanel({ employerId }) {
  const [academicYear, setAcademicYear] = useState(ACADEMIC_YEAR_OPTIONS[0] ?? '');
  const [months, setMonths] = useState([]);
  const [statusLoading, setStatusLoading] = useState(false);
  const [uploadingMonth, setUploadingMonth] = useState(null);
  const [generating, setGenerating] = useState(false);
  const [alert, setAlert] = useState(null);
  const [editorMonth, setEditorMonth] = useState(null);
  const fileInputRefs = useRef({});

  const showAlert = useCallback((type, msg) => {
    setAlert({ type, msg });
    setTimeout(() => setAlert(null), 4500);
  }, []);

  const loadStatus = useCallback(
    async ({ silent = false } = {}) => {
      if (!employerId || !String(academicYear ?? '').trim()) {
        setMonths([]);
        return;
      }
      if (!silent) setStatusLoading(true);
      try {
        const res = await payrollMonthlyInputsApi.getYearStatus(
          employerId,
          academicYear.trim(),
        );
        setMonths(Array.isArray(res.data) ? res.data : []);
      } catch (err) {
        if (!silent) setMonths([]);
        const msg = await parseApiErrorMessage(err, 'שגיאה בטעינת סטטוס החודשים.');
        showAlert('danger', msg);
      } finally {
        if (!silent) setStatusLoading(false);
      }
    },
    [employerId, academicYear, showAlert],
  );

  useEffect(() => {
    loadStatus();
  }, [loadStatus]);

  const openUploadForMonth = (month) => {
    const year = String(academicYear ?? '').trim();
    if (!year) {
      showAlert('warning', 'יש לבחור שנת לימודים.');
      return;
    }
    fileInputRefs.current[month]?.click();
  };

  const handleFileSelected = async (event, month) => {
    const file = event.target.files?.[0];
    event.target.value = '';
    if (!file) return;

    const year = String(academicYear ?? '').trim();
    if (!year) {
      showAlert('warning', 'יש לבחור שנת לימודים.');
      return;
    }
    if (!isXlsxFile(file)) {
      showAlert('warning', 'יש לבחור קובץ Excel בפורמט .xlsx בלבד.');
      return;
    }

    setUploadingMonth(month);
    try {
      const res = await payrollMonthlyInputsApi.importMonth(
        employerId,
        year,
        month,
        file,
      );
      const serverMsg = res.data?.message;
      showAlert('success', serverMsg || 'הקובץ נקלט בהצלחה.');
      await loadStatus({ silent: true });
    } catch (err) {
      const msg = await parseApiErrorMessage(err, 'שגיאה בקליטת קובץ העוקץ.');
      showAlert('danger', msg);
    } finally {
      setUploadingMonth(null);
    }
  };

  const openRowsEditor = (month) => {
    const year = String(academicYear ?? '').trim();
    if (!year) {
      showAlert('warning', 'יש לבחור שנת לימודים.');
      return;
    }
    setEditorMonth(month);
  };

  const handleGenerateReport = async () => {
    const year = String(academicYear ?? '').trim();
    if (!year) {
      showAlert('warning', 'יש לבחור שנת לימודים.');
      return;
    }
    setGenerating(true);
    try {
      const res = await reportsApi.annualComparisonSaved(employerId, year);
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
      const msg = await parseApiErrorMessage(err, 'שגיאה בהפקת דוח ההשוואה השנתי.');
      showAlert('danger', msg);
    } finally {
      setGenerating(false);
    }
  };

  const uploadInProgress = uploadingMonth != null;
  const editorOpen = editorMonth != null;

  return (
    <>
    <div className="card border shadow-sm">
      <div className="card-header d-flex align-items-center gap-2 bg-body-tertiary flex-wrap">
        <i className="bi bi-calendar-range text-primary"></i>
        <span className="fw-semibold">דוח השוואה שנתי מנתונים שנקלטו במהלך השנה</span>
      </div>
      <div className="card-body">
        {alert && (
          <div className={`alert alert-${alert.type} alert-dismissible fade show`}>
            <i
              className={`bi bi-${
                alert.type === 'success'
                  ? 'check-circle'
                  : alert.type === 'info'
                    ? 'info-circle'
                    : 'exclamation-triangle'
              } me-2`}
            ></i>
            {alert.msg}
            <button
              type="button"
              className="btn-close"
              onClick={() => setAlert(null)}
              aria-label="סגור"
            ></button>
          </div>
        )}

        <p className="text-muted small mb-3">
          קלטו קובץ עוקץ לכל חודש בשנת הלימודים. לאחר מכן ניתן להפיק דוח השוואה שנתי מנתונים
          שמורים במערכת, ללא העלאת קובץ שנתי מלא.
        </p>

        <div className="row g-3 align-items-end mb-4">
          <div className="col-sm-6 col-md-4">
            <label htmlFor="savedAnnualAcademicYear" className="form-label fw-semibold">
              שנת לימודים
            </label>
            <select
              id="savedAnnualAcademicYear"
              className="form-select"
              value={academicYear}
              onChange={(e) => setAcademicYear(e.target.value)}
              disabled={statusLoading || generating || uploadInProgress}
            >
              {ACADEMIC_YEAR_OPTIONS.map((y) => (
                <option key={y} value={y}>
                  {y}
                </option>
              ))}
            </select>
          </div>
          <div className="col-sm-6 col-md-8 d-grid d-md-block">
            <button
              type="button"
              className="btn btn-primary px-4"
              onClick={handleGenerateReport}
              disabled={generating || statusLoading || uploadInProgress || !employerId}
            >
              {generating ? (
                <>
                  <span className="spinner-border spinner-border-sm me-2" role="status"></span>
                  מכין…
                </>
              ) : (
                <>
                  <i className="bi bi-download me-2"></i>
                  הפק דוח השוואה שנתי
                </>
              )}
            </button>
          </div>
        </div>

        <div className="table-responsive">
          <table className="table table-sm table-hover mb-0 align-middle">
            <thead className="table-light">
              <tr>
                <th scope="col">חודש</th>
                <th scope="col">סטטוס</th>
                <th scope="col" className="text-center">
                  מספר שורות
                </th>
                <th scope="col">שם קובץ</th>
                <th scope="col">תאריך קליטה</th>
                <th scope="col" className="text-end">
                  פעולות
                </th>
              </tr>
            </thead>
            <tbody>
              {statusLoading && months.length === 0 ? (
                <tr>
                  <td colSpan={6} className="text-center py-4 text-muted">
                    <span className="spinner-border spinner-border-sm me-2" role="status"></span>
                    טוען…
                  </td>
                </tr>
              ) : months.length === 0 ? (
                <tr>
                  <td colSpan={6} className="text-center py-3 text-muted">
                    אין נתונים לשנת הלימודים שנבחרה.
                  </td>
                </tr>
              ) : (
                months.map((row) => {
                  const isUploading = uploadingMonth === row.month;
                  const isMissing = row.status === STATUS_MISSING;
                  const isCaptured = row.status === STATUS_CAPTURED;
                  const actionsDisabled = isUploading || generating || uploadInProgress || editorOpen;

                  return (
                    <tr key={`${row.month}-${row.gregorianYear}`}>
                      <td>{row.displayName || row.month}</td>
                      <td>
                        <span className={`badge ${statusBadgeClass(row.status)}`}>
                          {row.status || '—'}
                        </span>
                      </td>
                      <td className="text-center">
                        {isCaptured ? row.rowsCount : '—'}
                      </td>
                      <td
                        className="text-truncate"
                        style={{ maxWidth: '12rem' }}
                        title={row.originalFileName ?? ''}
                      >
                        {isCaptured && row.originalFileName ? row.originalFileName : '—'}
                      </td>
                      <td>
                        {isCaptured ? formatUploadedAt(row.uploadedAtUtc) : '—'}
                      </td>
                      <td className="text-end">
                        <input
                          type="file"
                          accept={XLSX_ACCEPT}
                          className="d-none"
                          aria-hidden
                          tabIndex={-1}
                          ref={(el) => {
                            fileInputRefs.current[row.month] = el;
                          }}
                          onChange={(e) => handleFileSelected(e, row.month)}
                          disabled={actionsDisabled}
                        />
                        <div className="d-flex flex-wrap gap-1 justify-content-end">
                          {isMissing && (
                            <button
                              type="button"
                              className="btn btn-outline-success btn-sm"
                              disabled={actionsDisabled}
                              onClick={() => openUploadForMonth(row.month)}
                            >
                              {isUploading ? (
                                <>
                                  <span
                                    className="spinner-border spinner-border-sm me-1"
                                    role="status"
                                  ></span>
                                  מעלה…
                                </>
                              ) : (
                                <>
                                  <i className="bi bi-upload me-1"></i>
                                  העלאה
                                </>
                              )}
                            </button>
                          )}
                          {isCaptured && (
                            <>
                              <button
                                type="button"
                                className="btn btn-outline-primary btn-sm"
                                disabled={actionsDisabled}
                                onClick={() => openRowsEditor(row.month)}
                              >
                                <i className="bi bi-pencil-square me-1"></i>
                                צפייה/עריכה
                              </button>
                              <button
                                type="button"
                                className="btn btn-outline-secondary btn-sm"
                                disabled={actionsDisabled}
                                onClick={() => openUploadForMonth(row.month)}
                              >
                                {isUploading ? (
                                  <>
                                    <span
                                      className="spinner-border spinner-border-sm me-1"
                                      role="status"
                                    ></span>
                                    מעלה…
                                  </>
                                ) : (
                                  <>
                                    <i className="bi bi-arrow-repeat me-1"></i>
                                    החלפת קובץ
                                  </>
                                )}
                              </button>
                            </>
                          )}
                        </div>
                      </td>
                    </tr>
                  );
                })
              )}
            </tbody>
          </table>
        </div>
      </div>
    </div>

    {editorOpen && (
      <PayrollMonthlyRowsEditor
        employerId={employerId}
        academicYear={academicYear.trim()}
        month={editorMonth}
        onClose={() => setEditorMonth(null)}
        onSaved={() => loadStatus({ silent: true })}
      />
    )}
    </>
  );
}
