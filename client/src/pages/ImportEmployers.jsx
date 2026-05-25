import { useState, useRef } from 'react';
import { Link } from 'react-router-dom';
import { api, bulkImportApi } from '../api';

export default function ImportEmployers() {
  const [file, setFile] = useState(null);
  const [loading, setLoading] = useState(false);
  const [results, setResults] = useState(null);
  const [error, setError] = useState(null);
  const inputRef = useRef();

  const [templateLoading, setTemplateLoading] = useState(false);

  const handleDownloadTemplate = async () => {
    setTemplateLoading(true);
    setError(null);
    try {
      await bulkImportApi.downloadEmployersTemplate();
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
      const res = await api.post('/bulk-import/employers', formData, {
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
          <li className="breadcrumb-item active">ייבוא מעסיקים מאקסל</li>
        </ol></nav>
      </div>

      <div className="card mb-4">
        <div className="card-header d-flex align-items-center gap-2">
          <i className="bi bi-building-up text-primary"></i>
          <span className="fw-semibold">ייבוא מעסיקים מאקסל</span>
        </div>
        <div className="card-body">
          <div className="alert alert-info py-2 mb-3" style={{ fontSize: '0.88rem' }}>
            <i className="bi bi-info-circle me-2"></i>
            כל שורה חייבת לכלול <strong>ח.פ.</strong> מעסיק שכבר קיים (לפי ח.פ.) ידולג.{' '}
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

      {results && (
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
                <thead><tr><th>שורה</th><th>ח.פ.</th><th>שם מעסיק</th><th>סטטוס</th><th>הודעה</th></tr></thead>
                <tbody>
                  {results.rows.map((r, i) => (
                    <tr key={i} className={r.success ? '' : 'table-danger'}>
                      <td>{r.row}</td>
                      <td>{r.businessNumber || '—'}</td>
                      <td>{r.employerName || '—'}</td>
                      <td>
                        {r.success
                          ? <span className="badge bg-success"><i className="bi bi-check-lg"></i></span>
                          : <span className="badge bg-danger"><i className="bi bi-x-lg"></i></span>}
                      </td>
                      <td style={{ fontSize: '0.85rem' }}>{r.message}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
