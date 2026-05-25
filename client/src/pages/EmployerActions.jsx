import { useState, useEffect, useCallback } from 'react';
import { Link, useParams } from 'react-router-dom';
import { employersApi, reportsApi } from '../api';
import { formatDateDDMMYYYY, formatDateDDMMYYYYForFilename } from '../utils/dateFormat';
import { formatHebrewYear } from '../employmentDataHelpers';

// Generate Hebrew academic year options (current −3 … current +2)
function generateAcademicYears() {
  const now = new Date();
  const cur = now.getMonth() >= 8
    ? now.getFullYear() + 3761
    : now.getFullYear() + 3760;
  const years = [];
  for (let y = cur + 2; y >= cur - 3; y--) years.push(formatHebrewYear(y));
  return years;
}
const ACADEMIC_YEAR_OPTIONS = generateAcademicYears();

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
  {
    id: 'employees-csv',
    label: 'רשימת עובדי המעסיק (קובץ CSV)',
    description: 'ייצוא קליל: שם, ת.ז., מספר עובד בעוקץ, תאריך לידה, מין וסטטוס פעילות',
  },
  // ── 7 new reports ──────────────────────────────────────────────────────
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
    id: 'institution-hours',
    label: 'בדיקת שעות לסמל',
    description: 'סיכום שעות גננת וסייעת לסמל מוסד מסוים לשנת לימודים שנבחרה.',
    needsYear: true,
    needsSymbol: true,
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

function escapeCsvCell(v) {
  const s = v == null ? '' : String(v);
  if (/[",\r\n]/.test(s)) return `"${s.replace(/"/g, '""')}"`;
  return s;
}

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
  const [panel, setPanel] = useState('menu'); // menu | reports | compare
  const [alert, setAlert] = useState(null);
  const [reportType, setReportType] = useState('');
  const [reportGenerating, setReportGenerating] = useState(false);
  const [compareBusy, setCompareBusy] = useState(false);
  const [reportYear, setReportYear] = useState(ACADEMIC_YEAR_OPTIONS[0] ?? '');
  const [reportMonth, setReportMonth] = useState(9);
  const [reportSymbol, setReportSymbol] = useState('');
  const [institutionSymbols, setInstitutionSymbols] = useState([]);
  // comparison sub-type: 'payroll' (existing) | 'monthly' | 'annual'
  const [compareSubType, setCompareSubType] = useState('payroll');
  const [compareYear, setCompareYear] = useState(ACADEMIC_YEAR_OPTIONS[0] ?? '');
  const [compareMonth, setCompareMonth] = useState(9);
  const [compareFileBusy, setCompareFileBusy] = useState(false);

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

  const handleComparePayrollSubmit = async (e) => {
    e.preventDefault();
    const form = e.currentTarget;
    const input = form.elements.namedItem('payrollCompareFile');
    const file = input && 'files' in input ? input.files?.[0] : null;
    if (!file) {
      showAlert('warning', 'בחרו קובץ Excel (.xlsx).');
      return;
    }
    setCompareBusy(true);
    try {
      const res = await employersApi.compareMonthlyPayroll(id, file);
      const blob = new Blob([res.data], {
        type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
      });
      const cd = res.headers['content-disposition'] ?? res.headers['Content-Disposition'];
      let filename = `השוואת_שכר_${sanitizeFilenamePart(employer?.name) || id}_${new Date().toISOString().slice(0, 10)}.xlsx`;
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
      showAlert('success', 'דוח ההשוואה הורד.');
      form.reset();
    } catch (err) {
      let msg = 'שגיאה בהעלאה או ביצירת דוח ההשוואה.';
      const res = err.response;
      if (!res) {
        msg =
          'לא ניתן להתחבר לשרת. ודאו שה־API רץ (למשל https://localhost:7068), שהדפדפן מאשר תעודת SSL מקומית, ושניסיתם שוב לאחר הפעלה מחדש של השרת.';
      } else {
        const d = res.data;
        if (d instanceof Blob) {
          try {
            const t = await d.text();
            const j = JSON.parse(t);
            if (typeof j.message === 'string') msg = j.message;
            else if (typeof j.detail === 'string') msg = j.detail;
            else if (typeof j.title === 'string') msg = j.title;
          } catch {
            /* ignore — non-JSON error body */
          }
        } else if (typeof d?.message === 'string') msg = d.message;
      }
      showAlert('danger', msg);
    } finally {
      setCompareBusy(false);
    }
  };

  const handleComparisonReportSubmit = async (e) => {
    e.preventDefault();
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
        res = await reportsApi.monthlyComparison(id, compareYear, compareMonth, file);
        const monthLabel = MONTHS.find(m => m.value === compareMonth)?.label ?? compareMonth;
        filename = `דוח_השוואה_חודשי_${sanitizeFilenamePart(employer?.name)}_${compareYear}_${monthLabel}.xlsx`;
      } else {
        res = await reportsApi.annualComparison(id, compareYear, file);
        filename = `דוח_השוואה_שנתי_${sanitizeFilenamePart(employer?.name)}_${compareYear}.xlsx`;
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
      let msg = 'שגיאה בהעלאה או ביצירת דוח ההשוואה.';
      const res = err.response;
      if (!res) {
        msg = 'לא ניתן להתחבר לשרת. ודאו שה־API רץ ושהדפדפן מאשר תעודת SSL מקומית.';
      } else {
        const d = res.data;
        if (d instanceof Blob) {
          try {
            const t = await d.text();
            const j = JSON.parse(t);
            if (typeof j.message === 'string') msg = j.message;
            else if (typeof j.detail === 'string') msg = j.detail;
          } catch { /* non-JSON */ }
        } else if (typeof d?.message === 'string') msg = d.message;
      }
      showAlert('danger', msg);
    } finally {
      setCompareFileBusy(false);
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

    if (reportType === 'employees-csv') {
      setReportGenerating(true);
      try {
        const res = await employersApi.getEmployees(id, { page: 1, pageSize: 10000, search: undefined });
        const rows = res.data.items ?? [];
        const header = ['שם מלא', 'ת.ז.', 'מספר עובד בעוקץ', 'תאריך לידה', 'מין', 'סטטוס פעילות'];
        const lines = [
          header.map(escapeCsvCell).join(','),
          ...rows.map((emp) => {
            const birth =
              emp.birthDate != null && emp.birthDate !== ''
                ? formatDateDDMMYYYY(emp.birthDate)
                : '';
            const status = emp.isActive ? 'פעיל' : 'לא פעיל';
            return [emp.fullName ?? '', emp.idNumber ?? '', emp.employeeNumber ?? '', birth, emp.gender ?? '', status]
              .map(escapeCsvCell)
              .join(',');
          }),
        ];
        const csv = '\uFEFF' + lines.join('\r\n');
        const blob = new Blob([csv], { type: 'text/csv;charset=utf-8' });
        const suffix = formatDateDDMMYYYYForFilename(new Date());
        const safeName = sanitizeFilenamePart(employer?.name) || `מעסיק_${id}`;
        downloadBlob(`דוח_עובדים_${safeName}_${suffix}.csv`, blob);
        showAlert('success', `הורד דוח "${label}".`);
      } catch {
        showAlert('danger', 'שגיאה ביצירת קובץ הדוח.');
      } finally {
        setReportGenerating(false);
      }
      return;
    }

    // ── 7 new server-generated Excel reports ──────────────────────────
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
    if (selectedOpt.needsSymbol && !reportSymbol) {
      showAlert('warning', 'יש לבחור סמל מוסד.');
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
      'monthly-comparison': () => ({
        fetch: reportsApi.monthlyComparison(id, reportYear, reportMonth),
        filename: `השוואה_חודשית_${reportMonth}_${safeName}_${sanitizeFilenamePart(reportYear)}_${suffix}.xlsx`,
      }),
      'annual-comparison': () => ({
        fetch: reportsApi.annualComparison(id, reportYear),
        filename: `השוואה_שנתית_${safeName}_${sanitizeFilenamePart(reportYear)}_${suffix}.xlsx`,
      }),
      'institution-hours': () => ({
        fetch: reportsApi.institutionHours(id, reportYear, reportSymbol),
        filename: `שעות_סמל_${sanitizeFilenamePart(reportSymbol)}_${safeName}_${suffix}.xlsx`,
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
        let msg = 'שגיאה בהפקת הדוח.';
        const d = err.response?.data;
        if (d instanceof Blob) {
          try { const t = await d.text(); msg = JSON.parse(t)?.message ?? msg; } catch { /* ignore */ }
        } else if (typeof d?.message === 'string') {
          msg = d.message;
        }
        showAlert('danger', msg);
      } finally {
        setReportGenerating(false);
      }
      return;
    }

    showAlert(
      'info',
      `דוח "${label}" זמין בגרסאות הבאות של המערכת. ניתן בינתיים להוריד את רשימת העובדים בפורמט CSV.`
    );
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
              {panel === 'menu' ? 'פעולות ודוחות' : panel === 'reports' ? 'הנפקת דוחות' : 'השוואה'}
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
            <div className="card h-100 border shadow-sm opacity-75">
              <div className="card-header bg-body-tertiary py-3 d-flex align-items-center gap-2">
                <i className="bi bi-three-dots text-secondary fs-4"></i>
                <span className="fw-semibold text-secondary">פעולות נוספות</span>
              </div>
              <div className="card-body">
                <p className="text-muted small mb-0">כאן יופיעו כלים נוספים לפי צורך (יתעדכן בהמשך).</p>
              </div>
            </div>
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

                  {selectedOpt?.needsSymbol && (
                    <div className="col-sm-6 col-md-4">
                      <label className="form-label fw-semibold">סמל מוסד</label>
                      <select
                        className="form-select"
                        value={reportSymbol}
                        onChange={e => setReportSymbol(e.target.value)}
                      >
                        <option value="">— בחר סמל —</option>
                        {institutionSymbols.map(s => (
                          <option key={s.id ?? s.institutionSymbol} value={s.institutionSymbol}>
                            {s.institutionSymbolName
                              ? `${s.institutionSymbol} — ${s.institutionSymbolName}`
                              : s.institutionSymbol}
                          </option>
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

      {panel === 'compare' && (
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
                {[
                  { key: 'payroll', label: 'השוואת שכר חודשי', icon: 'bi-file-earmark-diff' },
                  { key: 'monthly', label: 'דוח השוואה לפי חודש מסוים', icon: 'bi-calendar-month' },
                  { key: 'annual',  label: 'דוח השוואה שנתי', icon: 'bi-calendar-range' },
                ].map(({ key, label, icon }) => (
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

            {/* ── Existing: monthly payroll comparison ── */}
            {compareSubType === 'payroll' && (
              <>
                <p className="text-muted small mb-3">
                  העלו קובץ Excel עם עמודות לפחות: <strong>תז</strong> או <strong>מספר עובד בעוקץ</strong>, וזיהוי חודש (
                  <strong>חודש</strong> + <strong>שנה</strong> גרגוריאניים, או עמודת <strong>תאריך</strong>). כל השורות חייבות
                  להשתייך לאותה שנת לימודים עברית (ספטמבר–אוגוסט). הפלט: <strong>V</strong> כשהנתונים תואמים, תא ריק עם רקע
                  צהוב כשיש אי־התאמה, ועמודת הערות עם פירוט.
                </p>
                <form className="row g-3 align-items-end" onSubmit={handleComparePayrollSubmit}>
                  <div className="col-lg-8">
                    <label htmlFor="payrollCompareFile" className="form-label fw-semibold">
                      קובץ Excel להשוואה
                    </label>
                    <input
                      id="payrollCompareFile"
                      name="payrollCompareFile"
                      type="file"
                      accept=".xlsx"
                      className="form-control"
                      disabled={compareBusy}
                      required
                    />
                  </div>
                  <div className="col-lg-4 d-grid d-lg-block">
                    <button type="submit" className="btn btn-success px-4" disabled={compareBusy}>
                      {compareBusy ? (
                        <><span className="spinner-border spinner-border-sm me-2" role="status"></span>מעבד…</>
                      ) : (
                        <><i className="bi bi-upload me-2"></i>העלה והורד דוח השוואה</>
                      )}
                    </button>
                  </div>
                </form>
              </>
            )}

            {/* ── New: monthly comparison report (with file upload) ── */}
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
      )}
    </div>
  );
}
