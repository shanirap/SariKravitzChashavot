import { useEffect, useRef, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { api, employersApi, bulkImportApi } from '../api';

export default function ImportEmployees() {
  const { employerId } = useParams();
  const [employer, setEmployer] = useState(null);
  const [file, setFile] = useState(null);
  const [loading, setLoading] = useState(false);
  const [results, setResults] = useState(null);
  const [error, setError] = useState(null);
  const [templateLoading, setTemplateLoading] = useState(false);
  const inputRef = useRef();

  useEffect(() => {
    if (!employerId) return;
    employersApi.getById(employerId)
      .then(res => setEmployer(res.data))
      .catch(() => setError('שגיאה בטעינת המעסיק.'));
  }, [employerId]);

  const handleDownloadTemplate = async () => {
    setTemplateLoading(true);
    setError(null);
    try {
      const eid = employerId ? parseInt(employerId, 10) : undefined;
      await bulkImportApi.downloadEmployeesTemplate(
        Number.isFinite(eid) ? { employerId: eid } : {},
      );
    } catch {
      setError('לא ניתן להוריד את התבנית. ודאו שאתם מחוברים לרשת ונסו שוב.');
    } finally {
      setTemplateLoading(false);
    }
  };

  const handleFileChange = (e) => {
    setFile(e.target.files[0] || null);
    setResults(null);
    setError(null);
  };

  const handleUpload = async () => {
    if (!file) return;
    setLoading(true); setError(null); setResults(null);
    const formData = new FormData();
    formData.append('file', file);
    try {
      const url = employerId
        ? `/bulk-import/employers/${employerId}/employees`
        : '/bulk-import/employees';
      const res = await api.post(url, formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
      });
      setResults(res.data);
    } catch (err) {
      setError(err.response?.data?.message || 'שגיאה בייבוא הקובץ.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div dir="rtl">
      <div className="breadcrumb-area mb-3">
        <nav><ol className="breadcrumb mb-0">
          <li className="breadcrumb-item"><Link to="/">מעסיקים</Link></li>
          {employerId && <li className="breadcrumb-item"><Link to={`/employers/${employerId}`}>{employer?.name || 'מעסיק'}</Link></li>}
          <li className="breadcrumb-item active">ייבוא עובדים מאקסל</li>
        </ol></nav>
      </div>

      <div className="card mb-4">
        <div className="card-header d-flex align-items-center gap-2">
          <i className="bi bi-file-earmark-arrow-up text-primary"></i>
          <span className="fw-semibold">ייבוא עובדים ונתוני העסקה מאקסל</span>
        </div>
        <div className="card-body">
          <div className="alert alert-info py-2 mb-3" style={{ fontSize: '0.88rem' }}>
            <i className="bi bi-info-circle me-2"></i>
            {employerId ? (
              <>העובדים יתווספו אוטומטית למעסיק <strong>{employer?.name || ''}</strong>. כל שורה חייבת לכלול <strong>תז</strong> ו-<strong>שנת_לימודים</strong>. שדות אופציונליים כוללים <strong>מספר_עובד_בעוקץ</strong>, טלפון ותאריכי לידה של ילדים — כמו בטופס הוספת עובד.</>
            ) : (
              <>כל שורה חייבת לכלול <strong>שם_מעסיק</strong> תואם למעסיק קיים במערכת, <strong>תז</strong> ו-<strong>שנת_לימודים</strong>. ניתן למלא גם <strong>מספר_עובד_בעוקץ</strong> ושאר פרטי העובד לפי התבנית.</>
            )}
            לכל דרגה: עמודות דירוג אחת (<code>דרגה1_שם_הדירוג</code> …) ואז מקטעים 1–6 (<code>דרגה1_1_סמל_מוסד</code> …) לפי התבנית.
            עובד שלא קיים ייווצר אוטומטית.{' '}
            <button
              type="button"
              className="btn btn-link p-0 align-baseline fw-semibold text-decoration-underline"
              disabled={templateLoading}
              onClick={handleDownloadTemplate}
            >
              {templateLoading ? (
                <span className="spinner-border spinner-border-sm me-1" role="status" />
              ) : (
                <i className="bi bi-download me-1"></i>
              )}
              הורד תבנית
            </button>
          </div>

          <div className="d-flex align-items-center gap-3 flex-wrap">
            <input ref={inputRef} type="file" accept=".xlsx"
              className="form-control" style={{ maxWidth: 340 }}
              onChange={handleFileChange} />
            <button className="btn btn-primary px-4" onClick={handleUpload}
              disabled={!file || loading}>
              {loading
                ? <><span className="spinner-border spinner-border-sm me-2"></span>מייבא...</>
                : <><i className="bi bi-upload me-2"></i>ייבא</>}
            </button>
          </div>

          {error && (
            <div className="alert alert-danger mt-3 py-2">
              <i className="bi bi-exclamation-triangle me-2"></i>{error}
            </div>
          )}
        </div>
      </div>

      {results && <ResultsTable results={results} columns={['שורה','ת.ז.','מעסיק','סטטוס','הודעה']}
        rows={results.rows.map(r => [r.row, r.idNumber||'—', r.employerName||'—', r.success, r.message])} />}
    </div>
  );
}

function ResultsTable({ results, columns, rows }) {
  return (
    <div className="card">
      <div className="card-header d-flex align-items-center gap-3">
        <i className="bi bi-clipboard-data text-primary"></i>
        <span className="fw-semibold">תוצאות הייבוא</span>
        <span className="badge bg-success ms-auto">{results.imported} הצליחו</span>
        {results.errors > 0 && <span className="badge bg-danger">{results.errors} נכשלו</span>}
      </div>
      <div className="card-body p-0">
        <div className="table-responsive">
          <table className="table table-sm mb-0">
            <thead><tr>{columns.map(c => <th key={c}>{c}</th>)}</tr></thead>
            <tbody>
              {rows.map((r, i) => {
                const success = r[columns.indexOf('סטטוס')];
                return (
                  <tr key={i} className={success ? '' : 'table-danger'}>
                    {r.map((cell, j) =>
                      columns[j] === 'סטטוס' ? (
                        <td key={j}>
                          {cell
                            ? <span className="badge bg-success"><i className="bi bi-check-lg"></i></span>
                            : <span className="badge bg-danger"><i className="bi bi-x-lg"></i></span>}
                        </td>
                      ) : <td key={j} style={{ fontSize: '0.85rem' }}>{cell}</td>
                    )}
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
