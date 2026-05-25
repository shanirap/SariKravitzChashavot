import { useState, useEffect, useCallback } from 'react';
import { useParams, Link } from 'react-router-dom';
import { employersApi } from '../api';

export default function EmployerInstitutionSymbols() {
  const { id } = useParams();
  const [employer, setEmployer] = useState(null);
  const [institutionSymbols, setInstitutionSymbols] = useState([]);
  const [symbolForm, setSymbolForm] = useState({ institutionSymbol: '', institutionSymbolName: '' });
  const [alert, setAlert] = useState(null);
  const [loading, setLoading] = useState(true);

  const showAlert = (type, msg) => {
    setAlert({ type, msg });
    setTimeout(() => setAlert(null), 4000);
  };

  const loadEmployer = useCallback(async () => {
    try {
      const res = await employersApi.getById(id);
      setEmployer(res.data);
    } catch {
      showAlert('danger', 'שגיאה בטעינת המעסיק.');
    } finally {
      setLoading(false);
    }
  }, [id]);

  const loadInstitutionSymbols = useCallback(async () => {
    try {
      const res = await employersApi.getInstitutionSymbols(id);
      setInstitutionSymbols(res.data);
    } catch {
      showAlert('danger', 'שגיאה בטעינת סמלי מוסד.');
    }
  }, [id]);

  useEffect(() => {
    loadEmployer();
  }, [loadEmployer]);

  useEffect(() => {
    if (employer) loadInstitutionSymbols();
  }, [employer, loadInstitutionSymbols]);

  const handleAddInstitutionSymbol = async (e) => {
    e.preventDefault();
    try {
      await employersApi.createInstitutionSymbol(id, symbolForm);
      setSymbolForm({ institutionSymbol: '', institutionSymbolName: '' });
      showAlert('success', 'סמל המוסד נוסף בהצלחה.');
      loadInstitutionSymbols();
    } catch (err) {
      showAlert('danger', err.response?.data?.message || 'שגיאה בהוספת סמל מוסד.');
    }
  };

  const handleDeleteInstitutionSymbol = async (symbol) => {
    if (!window.confirm(`האם למחוק את סמל המוסד "${symbol.institutionSymbol}"?`)) return;
    try {
      await employersApi.deleteInstitutionSymbol(id, symbol.id);
      showAlert('success', 'סמל המוסד נמחק.');
      loadInstitutionSymbols();
    } catch (err) {
      showAlert('danger', err.response?.data?.message || 'שגיאה במחיקת סמל מוסד.');
    }
  };

  if (loading) {
    return (
      <div className="text-center py-5">
        <div className="spinner-border text-primary"></div>
      </div>
    );
  }

  if (!employer) {
    return <div className="alert alert-danger">המעסיק לא נמצא.</div>;
  }

  return (
    <>
      <div className="breadcrumb-area">
        <nav>
          <ol className="breadcrumb">
            <li className="breadcrumb-item">
              <Link to="/">
                <i className="bi bi-building me-1"></i>מעסיקים
              </Link>
            </li>
            <li className="breadcrumb-item">
              <Link to={`/employers/${id}`}>{employer.name}</Link>
            </li>
            <li className="breadcrumb-item active">סמלי מוסד</li>
          </ol>
        </nav>
      </div>

      <div className="d-flex justify-content-between align-items-center mb-3 flex-wrap gap-2">
        <h2 className="page-title mb-0" style={{ fontSize: '1.3rem' }}>
          <i className="bi bi-bank me-2 text-primary"></i>סמלי מוסד — {employer.name}
        </h2>
        <Link to={`/employers/${id}`} className="btn btn-outline-secondary btn-action px-4">
          <i className="bi bi-arrow-right me-2"></i>חזרה לעובדים
        </Link>
      </div>

      {alert && (
        <div className={`alert alert-${alert.type} alert-dismissible fade show`}>
          <i
            className={`bi bi-${alert.type === 'success' ? 'check-circle' : 'exclamation-triangle'} me-2`}
          ></i>
          {alert.msg}
          <button className="btn-close" type="button" onClick={() => setAlert(null)}></button>
        </div>
      )}

      <div className="card">
        <div className="card-header d-flex align-items-center gap-2">
          <i className="bi bi-bank"></i>
          <span>ניהול סמלי מוסד</span>
        </div>
        <div className="card-body">
          <form onSubmit={handleAddInstitutionSymbol} className="row g-2 align-items-end mb-3">
            <div className="col-md-3">
              <label className="form-label fw-semibold">
                סמל מוסד <span className="text-danger">*</span>
              </label>
              <input
                type="text"
                className="form-control"
                value={symbolForm.institutionSymbol}
                onChange={(e) => setSymbolForm((f) => ({ ...f, institutionSymbol: e.target.value }))}
                required
              />
            </div>
            <div className="col-md-5">
              <label className="form-label fw-semibold">שם סמל מוסד</label>
              <input
                type="text"
                className="form-control"
                value={symbolForm.institutionSymbolName}
                onChange={(e) => setSymbolForm((f) => ({ ...f, institutionSymbolName: e.target.value }))}
              />
            </div>
            <div className="col-md-auto">
              <button type="submit" className="btn btn-primary px-4">
                <i className="bi bi-plus-lg me-1"></i>הוסף
              </button>
            </div>
          </form>

          {institutionSymbols.length === 0 ? (
            <div className="text-muted">עדיין לא הוגדרו סמלי מוסד למעסיק הזה.</div>
          ) : (
            <div className="table-responsive">
              <table className="table table-sm table-hover mb-0">
                <thead>
                  <tr>
                    <th>סמל מוסד</th>
                    <th>שם סמל מוסד</th>
                    <th></th>
                  </tr>
                </thead>
                <tbody>
                  {institutionSymbols.map((symbol) => (
                    <tr key={symbol.id}>
                      <td className="fw-semibold">{symbol.institutionSymbol}</td>
                      <td>{symbol.institutionSymbolName || '—'}</td>
                      <td className="text-start">
                        <button
                          type="button"
                          className="btn btn-sm btn-outline-danger"
                          onClick={() => handleDeleteInstitutionSymbol(symbol)}
                        >
                          <i className="bi bi-trash"></i>
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      </div>
    </>
  );
}
