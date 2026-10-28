import { useState, useEffect, useCallback } from 'react';
import { Link, useParams } from 'react-router-dom';
import { employersApi, reportsApi, employmentDataApi } from '../api';
import { parseApiErrorMessage } from '../utils/apiErrorMessage';
import { formatDateDDMMYYYYForFilename } from '../utils/dateFormat';
import { REPORT_ACADEMIC_YEAR_OPTIONS, generateAcademicYearOptions, formatHebrewYear } from '../employmentDataHelpers';
import AnnualComparisonSavedPanel from '../components/reports/AnnualComparisonSavedPanel';

/** Returns the Hebrew academic year that comes after the current one (e.g. current תשפ"ז → next תשפ"ח). */
function nextAcademicYearFromNow() {
  const now = new Date();
  const gy = now.getFullYear();
  // current Hebrew year (September-based)
  const currentHebrew = now.getMonth() >= 8 ? gy + 3761 : gy + 3760;
  return formatHebrewYear(currentHebrew + 1);
}

const ACADEMIC_YEAR_OPTIONS = REPORT_ACADEMIC_YEAR_OPTIONS;

const MONTHS = [
  { value: 9,  label: 'ספטמבר' },
  { value: 10, label: 'אוקטובר' },
  { value: 11, label: 'נובמבר' },
  { value: 12, label: 'דצמבר' },
  { value: 1,  label: 'ינואר' },
  { value: 2,  label: 'פברואר' },
  { value: 3,  label: 'מרץ' },
  { value: 4,  label: 'אפריל' },
  { value: 5,  label: 'מאי' },
  { value: 6,  label: 'יוני' },
  { value: 7,  label: 'יולי' },
  { value: 8,  label: 'אוגוסט' },
];

const EMPLOYER_REPORT_OPTIONS = [
  {
    id: 'employer-full-xlsx',
    label: 'כל הנתונים — קובץ Excel (.xlsx)',
    description:
      'גיליונות: מעסיק, עובדים (כולל ילדים וסטטוס), סמלי מוסד, נתוני העסקה ומקטעים — לפי מעסיק זה בלבד',
  },
  // ── דוחות Excel בשרת ─────────────────────────────────────────────────
  {
    id: 'kindergarten-annual',
    label: 'מצבת גנים שנתי',
    description: 'ייצוא נתוני עסקה של עובדי גן (גננות) לשנת לימודים שנבחרה.',
    needsYear: true,
  },
  {
    id: 'school-annual',
    label: 'מצבת בית ספר שנתי',
    description: 'ייצוא נתוני עסקה של עובדי בית ספר (מורות, מנהלים) לשנת לימודים שנבחרה.',
    needsYear: true,
  },
  {
    id: 'employees-personal',
    label: 'עובדים אישיים',
    description: 'ייצוא פרטים אישיים של כל עובדי המעסיק (שם, ת.ז., תאריך לידה, מין וכו\').',
  },
  {
    id: 'employees-employment-data',
    label: 'עובדים נתוני העסקה',
    description: 'ייצוא נתוני עסקה מפורטים לשנת לימודים שנבחרה (דירוג, תפקיד, ש"ש, אחוז משרה וכו\').',
    needsYear: true,
  },
];

const COMPARE_SUBTYPES = [
  { key: 'monthly', label: 'דוח השוואה לפי חודש מסוים', icon: 'bi-calendar-month' },
  { key: 'annual', label: 'דוח השוואה שנתי מקובץ חד-פעמי', icon: 'bi-calendar-range' },
  {
    key: 'institution-hours',
    label: 'בדיקת שעות לסמל',
    icon: 'bi-clock-history',
    description: 'בדיקת שעות לפי סמל מוסד או לכל הסמלים יחד ושנת לימודים',
  },
];

function downloadBlob(filename, blob) {
  const url = URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = filename;
  a.click();
  URL.revokeObjectURL(url);
}

function sanitizeFilenamePart(name) {
  return String(name ?? '')
    .trim()
    .replace(/[<>:"/\\|?*]/g, '_')
    .slice(0, 80);
}

export default function EmployerActions() {
  const { id } = useParams();
  const [employer, setEmployer] = useState(null);
  const [loading, setLoading] = useState(true);
  const [panel, setPanel] = useState('menu'); // menu | reports | compare | bulk-next-year
  const [alert, setAlert] = useState(null);
  const [reportType, setReportType] = useState('');
  const [reportGenerating, setReportGenerating] = useState(false);
  const [reportYear, setReportYear] = useState(ACADEMIC_YEAR_OPTIONS[0] ?? '');
  const [reportMonth, setReportMonth] = useState(9);
  const [institutionSymbols, setInstitutionSymbols] = useState([]);
  // comparison sub-type: monthly | annual | institution-hours (legacy payroll API kept on server)
  const [compareSubType, setCompareSubType] = useState('monthly');
  const [compareYear, setCompareYear] = useState(ACADEMIC_YEAR_OPTIONS[0] ?? '');
  const [compareMonth, setCompareMonth] = useState(9);
  const [compareSymbol, setCompareSymbol] = useState('');
  const [compareFileBusy, setCompareFileBusy] = useState(false);
  const [institutionHoursBusy, setInstitutionHoursBusy] = useState(false);
  // bulk next year — default to the academic year that immediately follows the current one
  const [bulkTargetYear, setBulkTargetYear] = useState(() => nextAcademicYearFromNow());
  const [bulkSourceYear, setBulkSourceYear] = useState('');
  const [bulkBusy, setBulkBusy] = useState(false);
  const [bulkResult, setBulkResult] = useState(null); // BulkNextYearResultDto | null
  const [bulkConfirmed, setBulkConfirmed] = useState(false);

  const showAlert = useCallback((type, msg) => {
    setAlert({ type, msg });
    setTimeout(() => setAlert(null), 4500);
  }, []);

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const res = await employersApi.getById(id);
      setEmployer(res.data);
    } catch {
      setEmployer(null);
      showAlert('danger', 'שגיאה בטעינת המעסיק.');
    } finally {
      setLoading(false);
    }
  }, [id, showAlert]);

  useEffect(() => {
    load();
  }, [load]);

  useEffect(() => {
    if (!id) return;
    employersApi.getInstitutionSymbols(id)
      .then(res => setInstitutionSymbols(res.data ?? []))
      .catch(() => {});
  }, [id]);

  const handleComparisonReportSubmit = async (e) => {
    e.preventDefault();
    const year = compareYear?.trim();
    if (!year) {
      showAlert('warning', 'יש לבחור שנת לימודים.');
      return;
    }
    const form = e.currentTarget;
    const input = form.elements.namedItem('comparisonFile');
    const file = input && 'files' in input ? input.files?.[0] : null;
    if (!file) {
      showAlert('warning', 'בחרו קובץ Excel (.xlsx) להשוואה.');
      return;
    }
    setCompareFileBusy(true);
    try {
      let res;
      let filename;
      if (compareSubType === 'monthly') {
        res = await reportsApi.monthlyComparison(id, year, compareMonth, file);
        const monthLabel = MONTHS.find(m => m.value === compareMonth)?.label ?? compareMonth;
        filename = `דוח_השוואה_חודשי_${sanitizeFilenamePart(employer?.name)}_${year}_${monthLabel}.xlsx`;
      } else {
        res = await reportsApi.annualComparison(id, year, file);
        filename = `דוח_השוואה_שנתי_${sanitizeFilenamePart(employer?.name)}_${year}.xlsx`;
      }
      const blob = new Blob([res.data], {
        type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
      });
      const cd = res.headers['content-disposition'] ?? res.headers['Content-Disposition'];
      if (cd && typeof cd === 'string') {
        const star = /filename\*=UTF-8''([^;\s]+)/i.exec(cd);
        const quoted = /filename="([^"]+)"/i.exec(cd);
        const raw = star?.[1] ?? quoted?.[1];
        if (raw) {
          try { filename = decodeURIComponent(raw.replace(/\+/g, '%20')); } catch { filename = raw; }
        }
      }
      downloadBlob(filename, blob);
      showAlert('success', 'דוח ההשוואה הורד.');
      form.reset();
    } catch (err) {
      const msg = await parseApiErrorMessage(err, 'שגיאה בהעלאה או ביצירת דוח ההשוואה.');
      showAlert('danger', msg);
    } finally {
      setCompareFileBusy(false);
    }
  };

  const handleInstitutionHoursSubmit = async (e) => {
    e.preventDefault();
    const year = compareYear?.trim();
    const symbol = compareSymbol?.trim();
    if (!year) {
      showAlert('warning', 'יש לבחור שנת לימודים.');
      return;
    }
    if (!symbol) {
      showAlert('warning', 'יש לבחור סמל מוסד או "כל הסמלים".');
      return;
    }
    setInstitutionHoursBusy(true);
    try {
      const res = await reportsApi.institutionHours(id, year, symbol);
      const blob = new Blob([res.data], {
        type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
      });
      const suffix = formatDateDDMMYYYYForFilename(new Date());
      const safeName = sanitizeFilenamePart(employer?.name) || `מעסיק_${id}`;
      const symbolPart = symbol === '*' ? 'כל_הסמלים' : sanitizeFilenamePart(symbol);
      const filename = `שעות_סמל_${symbolPart}_${safeName}_${suffix}.xlsx`;
      downloadBlob(filename, blob);
      showAlert('success', 'הורד דוח "בדיקת שעות לסמל".');
    } catch (err) {
      const msg = await parseApiErrorMessage(err, 'שגיאה בהפקת הדוח.');
      showAlert('danger', msg);
    } finally {
      setInstitutionHoursBusy(false);
    }
  };

  const handleBulkNextYear = async () => {
    if (!bulkTargetYear) {
      showAlert('warning', 'יש לבחור שנת לימודים יעד.');
      return;
    }
    setBulkBusy(true);
    setBulkResult(null);
    try {
      const res = await employmentDataApi.bulkAddNextYear(
        id,
        bulkTargetYear,
        bulkSourceYear || null,
      );
      setBulkResult(res.data);
      const { created, skipped, errors } = res.data;
      if (errors > 0 && created === 0) {
        showAlert('danger', `הפעולה הסתיימה עם ${errors} שגיאות ולא נוצרו רשומות חדשות.`);
      } else if (created > 0 && errors > 0) {
        showAlert('warning', `נוצרו ${created} רשומות — אך אירעו ${errors} שגיאות. בדוק את הפירוט.`);
      } else if (created > 0) {
        showAlert('success', `נוצרו ${created} רשומות חדשות לשנת ${bulkTargetYear}.`);
      } else if (errors === 0) {
        showAlert('info', 'לא נוצרו רשומות חדשות — כל העובדים הרלוונטיים כבר מעודכנים.');
      }
    } catch (err) {
      const msg = await parseApiErrorMessage(err, 'שגיאה בביצוע הפעולה.');
      showAlert('danger', msg);
    } finally {
      setBulkBusy(false);
      setBulkConfirmed(false);
    }
  };

  const handleIssueReport = async (e) => {
    e.preventDefault();
    if (!reportType) {
      showAlert('warning', 'בחרו סוג דוח מהרשימה.');
      return;
    }

    const opt = EMPLOYER_REPORT_OPTIONS.find((o) => o.id === reportType);
    const label = opt?.label ?? reportType;

    if (reportType === 'employer-full-xlsx') {
      setReportGenerating(true);
      try {
        const res = await employersApi.exportFullExcel(id);
        const blob = new Blob([res.data], {
          type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
        });
        const cd = res.headers['content-disposition'] ?? res.headers['Content-Disposition'];
        let filename = `דוח_מלא_${sanitizeFilenamePart(employer?.name) || id}_${new Date().toISOString().slice(0, 10)}.xlsx`;
        if (cd && typeof cd === 'string') {
          const star = /filename\*=UTF-8''([^;\s]+)/i.exec(cd);
          const quoted = /filename="([^"]+)"/i.exec(cd);
          const raw = star?.[1] ?? quoted?.[1];
          if (raw) {
            try {
              filename = decodeURIComponent(raw.replace(/\+/g, '%20'));
            } catch {
              filename = raw;
            }
          }
        }
        downloadBlob(filename, blob);
        showAlert('success', `הורד דוח "${label}".`);
      } catch {
        showAlert('danger', 'שגיאה בהורדת קובץ האקסל. ודאו שהשרת זמין.');
      } finally {
        setReportGenerating(false);
      }
      return;
    }

    // ── דוחות Excel בשרת ──────────────────────────────────────────────
    const selectedOpt = EMPLOYER_REPORT_OPTIONS.find(o => o.id === reportType);
    if (!selectedOpt) return;

    if (selectedOpt.needsYear && !reportYear) {
      showAlert('warning', 'יש לבחור שנת לימודים.');
      return;
    }
    if (selectedOpt.needsMonth && (!reportMonth || reportMonth < 1 || reportMonth > 12)) {
      showAlert('warning', 'יש לבחור חודש.');
      return;
    }
    const suffix = formatDateDDMMYYYYForFilename(new Date());
    const safeName = sanitizeFilenamePart(employer?.name) || `מעסיק_${id}`;

    const reportFetchers = {
      'kindergarten-annual': () => ({
        fetch: reportsApi.kindergartenAnnual(id, reportYear),
        filename: `מצבת_גנים_${safeName}_${sanitizeFilenamePart(reportYear)}_${suffix}.xlsx`,
      }),
      'school-annual': () => ({
        fetch: reportsApi.schoolAnnual(id, reportYear),
        filename: `מצבת_בית_ספר_${safeName}_${sanitizeFilenamePart(reportYear)}_${suffix}.xlsx`,
      }),
      'employees-personal': () => ({
        fetch: reportsApi.employeesPersonal(id),
        filename: `עובדים_אישיים_${safeName}_${suffix}.xlsx`,
      }),
      'employees-employment-data': () => ({
        fetch: reportsApi.employeesEmploymentData(id, reportYear),
        filename: `עובדים_נתוני_העסקה_${safeName}_${sanitizeFilenamePart(reportYear)}_${suffix}.xlsx`,
      }),
    };

    const fetcher = reportFetchers[reportType];
    if (fetcher) {
      setReportGenerating(true);
      const { fetch: fetchPromise, filename } = fetcher();
      try {
        const res = await fetchPromise;
        const blob = new Blob([res.data], {
          type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
        });
        downloadBlob(filename, blob);
        showAlert('success', `הורד דוח "${label}".`);
      } catch (err) {
        const msg = await parseApiErrorMessage(err, 'שגיאה בהפקת הדוח.');
        showAlert('danger', msg);
      } finally {
        setReportGenerating(false);
      }
      return;
    }

    showAlert('warning', `סוג הדוח "${label}" אינו נתמך.`);
  };

  if (loading) {
    return (
      <div className="text-center py-5">
        <div className="spinner-border text-primary"></div>
      </div>
    );
  }
  if (!employer) {
    return (
      <div className="container py-4">
        <div className="alert alert-danger mb-0">המעסיק לא נמצא.</div>
        <Link to="/" className="btn btn-link mt-2">
          חזרה למעסיקים
        </Link>
      </div>
    );
  }

  return (
    <div className="container py-3">
      <div className="breadcrumb-area mb-3">
        <nav>
          <ol className="breadcrumb mb-0">
            <li className="breadcrumb-item">
              <Link to="/">
                <i className="bi bi-building me-1"></i>מעסיקים
              </Link>
            </li>
            <li className="breadcrumb-item">
              <Link to={`/employers/${id}`}>{employer.name}</Link>
            </li>
            <li className="breadcrumb-item active">
              {panel === 'menu' ? 'פעולות ודוחות'
                : panel === 'reports' ? 'הנפקת דוחות'
                : panel === 'compare' ? 'השוואה'
                : 'הוספת שנת לימודים'}
            </li>
          </ol>
        </nav>
      </div>

      {alert && (
        <div className={`alert alert-${alert.type} alert-dismissible fade show`}>
          <i className={`bi bi-${alert.type === 'success' ? 'check-circle' : 'exclamation-triangle'} me-2`}></i>
          {alert.msg}
          <button type="button" className="btn-close" onClick={() => setAlert(null)}></button>
        </div>
      )}

      <div className="d-flex justify-content-between align-items-start flex-wrap gap-2 mb-4">
        <div>
          <h1 className="page-title h4 mb-1">
            <i className="bi bi-grid-3x3-gap text-primary me-2"></i>
            פעולות ודוחות
          </h1>
          <p className="text-muted small mb-0">{employer.name}</p>
        </div>
        {panel !== 'menu' && (
          <button type="button" className="btn btn-outline-secondary" onClick={() => setPanel('menu')}>
            <i className="bi bi-arrow-right me-2"></i>חזרה לתפריט הפעולות
          </button>
        )}
      </div>

      {panel === 'menu' && (
        <div className="row g-3">
          <div className="col-md-6 col-lg-4">
            <button
              type="button"
              className="card h-100 border shadow-sm text-start w-100 btn btn-light p-0 overflow-hidden"
              onClick={() => setPanel('reports')}
            >
              <div className="card-header bg-body-tertiary py-3 d-flex align-items-center gap-2">
                <i className="bi bi-file-earmark-bar-graph text-primary fs-4"></i>
                <span className="fw-semibold">הנפקת דוחות</span>
              </div>
              <div className="card-body">
                <p className="text-muted small mb-0">דוחות וייצוא קבצים הקשורים למעסיק זה בלבד.</p>
              </div>
            </button>
          </div>
          <div className="col-md-6 col-lg-4">
            <button
              type="button"
              className="card h-100 border shadow-sm text-start w-100 btn btn-light p-0 overflow-hidden"
              onClick={() => setPanel('compare')}
            >
              <div className="card-header bg-body-tertiary py-3 d-flex align-items-center gap-2">
                <i className="bi bi-arrow-left-right text-primary fs-4"></i>
                <span className="fw-semibold">השוואה</span>
              </div>
              <div className="card-body">
                <p className="text-muted small mb-0">השוואות דיווח ונתונים.</p>
              </div>
            </button>
          </div>
          <div className="col-md-6 col-lg-4">
            <button
              type="button"
              className="card h-100 border shadow-sm text-start w-100 btn btn-light p-0 overflow-hidden"
              onClick={() => { setBulkResult(null); setBulkConfirmed(false); setPanel('bulk-next-year'); }}
            >
              <div className="card-header bg-body-tertiary py-3 d-flex align-items-center gap-2">
                <i className="bi bi-calendar-plus text-success fs-4"></i>
                <span className="fw-semibold">הוספת שנת לימודים לכולם</span>
              </div>
              <div className="card-body">
                <p className="text-muted small mb-0">יצירת נתוני העסקה לשנה הבאה לכלל העובדים.</p>
              </div>
            </button>
          </div>
        </div>
      )}

      {panel === 'reports' && (
        <div className="card border-primary-subtle shadow-sm">
          <div className="card-header d-flex align-items-center gap-2 bg-body-tertiary">
            <i className="bi bi-file-earmark-bar-graph text-primary"></i>
            <span className="fw-semibold">הנפקת דוחות — {employer.name}</span>
          </div>
          <div className="card-body">
            <p className="text-muted small mb-3">
              בחרו את סוג הדוח ולחצו על <strong>הנפק דוח</strong>. דוח ה־Excel המלא נוצר בשרת וכולל את כל העובדים והנתונים
              הקשורים למעסיק זה.
            </p>
            {(() => {
              const selectedOpt = EMPLOYER_REPORT_OPTIONS.find(o => o.id === reportType);
              return (
                <form className="row g-3 align-items-end" onSubmit={handleIssueReport}>
                  <div className="col-12">
                    <label htmlFor="employerReportSelect" className="form-label fw-semibold">
                      סוג דוח
                    </label>
                    <select
                      id="employerReportSelect"
                      className="form-select"
                      value={reportType}
                      onChange={(e) => setReportType(e.target.value)}
                    >
                      <option value="">— בחר דוח —</option>
                      {EMPLOYER_REPORT_OPTIONS.map((o) => (
                        <option key={o.id} value={o.id}>
                          {o.label}
                        </option>
                      ))}
                    </select>
                    {selectedOpt ? (
                      <p className="form-text mb-0 small mt-1">{selectedOpt.description}</p>
                    ) : null}
                  </div>

                  {/* ── Conditional filters ──────────────────────────────── */}
                  {selectedOpt?.needsYear && (
                    <div className="col-sm-6 col-md-4">
                      <label className="form-label fw-semibold">שנת לימודים</label>
                      <select
                        className="form-select"
                        value={reportYear}
                        onChange={e => setReportYear(e.target.value)}
                      >
                        {ACADEMIC_YEAR_OPTIONS.map(y => (
                          <option key={y} value={y}>{y}</option>
                        ))}
                      </select>
                    </div>
                  )}

                  {selectedOpt?.needsMonth && (
                    <div className="col-sm-6 col-md-3">
                      <label className="form-label fw-semibold">חודש</label>
                      <select
                        className="form-select"
                        value={reportMonth}
                        onChange={e => setReportMonth(Number(e.target.value))}
                      >
                        {MONTHS.map(m => (
                          <option key={m.value} value={m.value}>{m.label}</option>
                        ))}
                      </select>
                    </div>
                  )}

                  <div className="col-12 d-grid d-lg-block">
                    <button type="submit" className="btn btn-primary px-4" disabled={reportGenerating || !reportType}>
                      {reportGenerating ? (
                        <>
                          <span className="spinner-border spinner-border-sm me-2" role="status"></span>
                          מכין…
                        </>
                      ) : (
                        <>
                          <i className="bi bi-download me-2"></i>הנפק דוח
                        </>
                      )}
                    </button>
                  </div>
                </form>
              );
            })()}
          </div>
        </div>
      )}

      {panel === 'bulk-next-year' && (
        <div className="card border-success-subtle shadow-sm">
          <div className="card-header d-flex align-items-center gap-2 bg-body-tertiary">
            <i className="bi bi-calendar-plus text-success"></i>
            <span className="fw-semibold">הוספת שנת לימודים לכלל העובדים — {employer.name}</span>
          </div>
          <div className="card-body">
            <p className="text-muted small mb-3">
              הפעולה יוצרת רשומת נתוני העסקה לשנת לימודים חדשה עבור <strong>כל עובד פעיל</strong> שיש לו
              נתוני העסקה בשנת המקור. הנתונים מועתקים מהרשומה הקיימת והוותק מוגדל ב-1.
              עובדים שכבר יש להם רשומה לשנת היעד — יידולגו.
            </p>

            <div className="row g-3 mb-4">
              <div className="col-sm-6 col-md-4">
                <label className="form-label fw-semibold">
                  שנת לימודים יעד <span className="text-danger">*</span>
                </label>
                <select
                  className="form-select"
                  value={bulkTargetYear}
                  onChange={e => setBulkTargetYear(e.target.value)}
                  disabled={bulkBusy}
                >
                  {ACADEMIC_YEAR_OPTIONS.map(y => <option key={y} value={y}>{y}</option>)}
                </select>
                <div className="form-text">השנה החדשה שתיצור.</div>
              </div>
              <div className="col-sm-6 col-md-4">
                <label className="form-label fw-semibold">שנת לימודים מקור (אופציונלי)</label>
                <select
                  className="form-select"
                  value={bulkSourceYear}
                  onChange={e => setBulkSourceYear(e.target.value)}
                  disabled={bulkBusy}
                >
                  <option value="">— השנה האחרונה לכל עובד —</option>
                  {ACADEMIC_YEAR_OPTIONS.map(y => <option key={y} value={y}>{y}</option>)}
                </select>
                <div className="form-text">ברירת מחדל: השנה האחרונה הקיימת לכל עובד.</div>
              </div>
            </div>

            {!bulkResult && (
              <div className="d-flex align-items-center gap-3 flex-wrap">
                {!bulkConfirmed ? (
                  <button
                    type="button"
                    className="btn btn-success px-4"
                    onClick={() => setBulkConfirmed(true)}
                    disabled={bulkBusy || !bulkTargetYear}
                  >
                    <i className="bi bi-calendar-plus me-2"></i>
                    הוסף שנת לימודים לכולם
                  </button>
                ) : (
                  <div className="alert alert-warning d-flex align-items-center gap-3 mb-0 py-2 px-3">
                    <span>
                      <i className="bi bi-exclamation-triangle-fill me-2"></i>
                      האם לבצע? פעולה זו תיצור רשומות חדשות לשנת <strong>{bulkTargetYear}</strong> לכל עובד רלוונטי.
                    </span>
                    <button
                      type="button"
                      className="btn btn-danger btn-sm"
                      onClick={handleBulkNextYear}
                      disabled={bulkBusy}
                    >
                      {bulkBusy
                        ? <><span className="spinner-border spinner-border-sm me-1"></span>מבצע…</>
                        : <><i className="bi bi-check-lg me-1"></i>כן, בצע</>}
                    </button>
                    <button
                      type="button"
                      className="btn btn-outline-secondary btn-sm"
                      onClick={() => setBulkConfirmed(false)}
                      disabled={bulkBusy}
                    >
                      ביטול
                    </button>
                  </div>
                )}
              </div>
            )}

            {bulkResult && (
              <div className="mt-3">
                {/* Summary badges */}
                <div className="d-flex flex-wrap gap-2 mb-3">
                  <span className="badge bg-success fs-6 px-3 py-2">
                    <i className="bi bi-check-circle me-1"></i>נוצרו: {bulkResult.created}
                  </span>
                  <span className="badge bg-secondary fs-6 px-3 py-2">
                    <i className="bi bi-skip-forward me-1"></i>דולגו: {bulkResult.skipped}
                  </span>
                  {bulkResult.errors > 0 && (
                    <span className="badge bg-danger fs-6 px-3 py-2">
                      <i className="bi bi-x-circle me-1"></i>שגיאות: {bulkResult.errors}
                    </span>
                  )}
                </div>

                {/* Details table */}
                {bulkResult.details && bulkResult.details.length > 0 && (
                  <div className="table-responsive" style={{ maxHeight: 400, overflowY: 'auto' }}>
                    <table className="table table-sm table-bordered table-hover mb-0">
                      <thead className="table-light sticky-top">
                        <tr>
                          <th>שם עובד</th>
                          <th>סטטוס</th>
                          <th>הערה</th>
                        </tr>
                      </thead>
                      <tbody>
                        {bulkResult.details.map((d, i) => (
                          <tr key={i} className={
                            d.status === 'created' ? 'table-success'
                            : d.status === 'error' ? 'table-danger'
                            : ''
                          }>
                            <td>{d.employeeName}</td>
                            <td>
                              {d.status === 'created' && <span className="badge bg-success">נוצר</span>}
                              {d.status === 'skipped' && <span className="badge bg-secondary">דולג</span>}
                              {d.status === 'error' && <span className="badge bg-danger">שגיאה</span>}
                            </td>
                            <td className="text-muted small">{d.message}</td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                )}

                <button
                  type="button"
                  className="btn btn-outline-secondary btn-sm mt-3"
                  onClick={() => { setBulkResult(null); setBulkConfirmed(false); }}
                >
                  <i className="bi bi-arrow-counterclockwise me-1"></i>הפעל שוב
                </button>
              </div>
            )}
          </div>
        </div>
      )}

      {panel === 'compare' && (
        <>
        <div className="card border shadow-sm">
          <div className="card-header d-flex align-items-center gap-2 bg-body-tertiary">
            <i className="bi bi-arrow-left-right text-primary"></i>
            <span className="fw-semibold">השוואה — {employer.name}</span>
          </div>
          <div className="card-body">
            {/* ── Compare type selector ── */}
            <div className="mb-4">
              <label className="form-label fw-semibold">סוג השוואה</label>
              <div className="d-flex flex-wrap gap-2">
                {COMPARE_SUBTYPES.map(({ key, label, icon }) => (
                  <button
                    key={key}
                    type="button"
                    className={`btn btn-sm px-3 ${compareSubType === key ? 'btn-primary' : 'btn-outline-secondary'}`}
                    onClick={() => setCompareSubType(key)}
                  >
                    <i className={`bi ${icon} me-1`}></i>{label}
                  </button>
                ))}
              </div>
            </div>

            {/* ── Monthly Okets comparison report (with file upload) ── */}
            {compareSubType === 'monthly' && (
              <>
                <p className="text-muted small mb-3">
                  בחרו שנת לימודים וחודש, העלו קובץ Excel עם נתוני עוקץ (שכר חודשי), וקבלו דוח השוואה בין המצבת לנתוני
                  העוקץ לאותו חודש.
                </p>
                <form className="row g-3 align-items-end" onSubmit={handleComparisonReportSubmit}>
                  <div className="col-sm-6 col-md-4">
                    <label className="form-label fw-semibold">שנת לימודים</label>
                    <select className="form-select" value={compareYear} onChange={e => setCompareYear(e.target.value)}>
                      {ACADEMIC_YEAR_OPTIONS.map(y => <option key={y} value={y}>{y}</option>)}
                    </select>
                  </div>
                  <div className="col-sm-6 col-md-3">
                    <label className="form-label fw-semibold">חודש</label>
                    <select className="form-select" value={compareMonth} onChange={e => setCompareMonth(Number(e.target.value))}>
                      {MONTHS.map(m => <option key={m.value} value={m.value}>{m.label}</option>)}
                    </select>
                  </div>
                  <div className="col-md-5">
                    <label htmlFor="compMonthlyFile" className="form-label fw-semibold">
                      קובץ עוקץ להשוואה (.xlsx)
                    </label>
                    <input
                      id="compMonthlyFile"
                      name="comparisonFile"
                      type="file"
                      accept=".xlsx"
                      className="form-control"
                      disabled={compareFileBusy}
                      required
                    />
                  </div>
                  <div className="col-12 d-grid d-lg-block">
                    <button type="submit" className="btn btn-success px-4" disabled={compareFileBusy}>
                      {compareFileBusy ? (
                        <><span className="spinner-border spinner-border-sm me-2" role="status"></span>מעבד…</>
                      ) : (
                        <><i className="bi bi-upload me-2"></i>העלה והורד דוח השוואה חודשי</>
                      )}
                    </button>
                  </div>
                </form>
              </>
            )}

            {/* ── Institution hours check (no file upload) ── */}
            {compareSubType === 'institution-hours' && (
              <>
                <p className="text-muted small mb-3">
                  {COMPARE_SUBTYPES.find((o) => o.key === 'institution-hours')?.description ??
                    'בדיקת שעות לפי סמל מוסד ושנת לימודים'}
                  . בחרו שנת לימודים וסמל מוסד (או כל הסמלים), ולחצו להורדת דוח Excel.
                </p>
                <form className="row g-3 align-items-end" onSubmit={handleInstitutionHoursSubmit}>
                  <div className="col-sm-6 col-md-4">
                    <label className="form-label fw-semibold">שנת לימודים</label>
                    <select
                      className="form-select"
                      value={compareYear}
                      onChange={(e) => setCompareYear(e.target.value)}
                    >
                      {ACADEMIC_YEAR_OPTIONS.map((y) => (
                        <option key={y} value={y}>
                          {y}
                        </option>
                      ))}
                    </select>
                  </div>
                  <div className="col-sm-6 col-md-4">
                    <label className="form-label fw-semibold">סמל מוסד</label>
                    <select
                      className="form-select"
                      value={compareSymbol}
                      onChange={(e) => setCompareSymbol(e.target.value)}
                    >
                      <option value="">— בחר סמל —</option>
                      <option value="*">כל הסמלים</option>
                      {institutionSymbols.map((s) => (
                        <option key={s.id ?? s.institutionSymbol} value={s.institutionSymbol}>
                          {s.institutionSymbolName
                            ? `${s.institutionSymbol} — ${s.institutionSymbolName}`
                            : s.institutionSymbol}
                        </option>
                      ))}
                    </select>
                  </div>
                  <div className="col-12 d-grid d-lg-block">
                    <button type="submit" className="btn btn-primary px-4" disabled={institutionHoursBusy}>
                      {institutionHoursBusy ? (
                        <>
                          <span className="spinner-border spinner-border-sm me-2" role="status"></span>
                          מכין…
                        </>
                      ) : (
                        <>
                          <i className="bi bi-download me-2"></i>הורד דוח בדיקת שעות לסמל
                        </>
                      )}
                    </button>
                  </div>
                </form>
              </>
            )}

            {/* ── New: annual comparison report (with file upload) ── */}
            {compareSubType === 'annual' && (
              <>
                <p className="text-muted small mb-3">
                  בחרו שנת לימודים והעלו קובץ Excel עם נתוני עוקץ שנתיים, וקבלו דוח השוואה שנתי בין המצבת לנתוני העוקץ.
                </p>
                <form className="row g-3 align-items-end" onSubmit={handleComparisonReportSubmit}>
                  <div className="col-sm-6 col-md-4">
                    <label className="form-label fw-semibold">שנת לימודים</label>
                    <select className="form-select" value={compareYear} onChange={e => setCompareYear(e.target.value)}>
                      {ACADEMIC_YEAR_OPTIONS.map(y => <option key={y} value={y}>{y}</option>)}
                    </select>
                  </div>
                  <div className="col-sm-6 col-md-8">
                    <label htmlFor="compAnnualFile" className="form-label fw-semibold">
                      קובץ עוקץ שנתי להשוואה (.xlsx)
                    </label>
                    <input
                      id="compAnnualFile"
                      name="comparisonFile"
                      type="file"
                      accept=".xlsx"
                      className="form-control"
                      disabled={compareFileBusy}
                      required
                    />
                  </div>
                  <div className="col-12 d-grid d-lg-block">
                    <button type="submit" className="btn btn-success px-4" disabled={compareFileBusy}>
                      {compareFileBusy ? (
                        <><span className="spinner-border spinner-border-sm me-2" role="status"></span>מעבד…</>
                      ) : (
                        <><i className="bi bi-upload me-2"></i>העלה והורד דוח השוואה שנתי</>
                      )}
                    </button>
                  </div>
                </form>
              </>
            )}
          </div>
        </div>

        <div className="mt-4">
          <AnnualComparisonSavedPanel employerId={Number(id)} />
        </div>
        </>
      )}
    </div>
  );
}
