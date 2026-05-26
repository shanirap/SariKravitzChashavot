import { useState, useEffect, useCallback } from 'react';
import { useParams, Link } from 'react-router-dom';
import { employersApi } from '../api';
import {
  INSTITUTION_TYPE_DEFAULT,
  INSTITUTION_TYPE_OPTIONS,
  normalizeInstitutionType,
} from '../institutionTypes';

const emptySymbolForm = () => ({
  institutionSymbol: '',
  institutionSymbolName: '',
  institutionType: INSTITUTION_TYPE_DEFAULT,
});

export default function EmployerInstitutionSymbols() {
  const { id } = useParams();
  const [employer, setEmployer] = useState(null);
  const [institutionSymbols, setInstitutionSymbols] = useState([]);
  const [symbolForm, setSymbolForm] = useState(emptySymbolForm);
  const [editingId, setEditingId] = useState(null);
  const [editForm, setEditForm] = useState({ institutionSymbolName: '', institutionType: INSTITUTION_TYPE_DEFAULT });
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
      setInstitutionSymbols(
        (res.data ?? []).map((s) => ({
          ...s,
          institutionType: normalizeInstitutionType(s.institutionType),
        }))
      );
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
      setSymbolForm(emptySymbolForm());
      showAlert('success', 'סמל המוסד נוסף בהצלחה.');
      loadInstitutionSymbols();
    } catch (err) {
      showAlert('danger', err.response?.data?.message || 'שגיאה בהוספת סמל מוסד.');
    }
  };

  const startEdit = (symbol) => {
    setEditingId(symbol.id);
    setEditForm({
      institutionSymbolName: symbol.institutionSymbolName ?? '',
      institutionType: normalizeInstitutionType(symbol.institutionType),
    });
  };

  const cancelEdit = () => {
    setEditingId(null);
  };

  const handleSaveEdit = async (symbolId) => {
    try {
      await employersApi.updateInstitutionSymbol(id, symbolId, {
        institutionSymbolName: editForm.institutionSymbolName,
        institutionType: editForm.institutionType,
      });
      showAlert('success', 'סמל המוסד עודכן.');
      setEditingId(null);
      loadInstitutionSymbols();
    } catch (err) {
      const status = err.response?.status;
      const msg = err.response?.data?.message;
      let text = msg || 'שגיאה בעדכון סמל מוסד.';
      if (status === 404)
        text = 'עדכון סמל מוסד לא נתמך — הפעילו מחדש את שרת ה-API לאחר בנייה.';
      else if (status >= 500 && !msg)
        text = 'שגיאת שרת — ודאו שהמיגרציה לעמודת "סוג מוסד" הוחלה והשרת הופעל מחדש.';
      showAlert('danger', text);
    }
  };

  const handleDeleteInstitutionSymbol = async (symbol) => {
    if (!window.confirm(`האם למחוק את סמל המוסד "${symbol.institutionSymbol}"?`)) return;
    try {
      await employersApi.deleteInstitutionSymbol(id, symbol.id);
      showAlert('success', 'סמל המוסד נמחק.');
      if (editingId === symbol.id) setEditingId(null);
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
            <div className="col-md-2">
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
            <div className="col-md-4">
              <label className="form-label fw-semibold">שם סמל מוסד</label>
              <input
                type="text"
                className="form-control"
                value={symbolForm.institutionSymbolName}
                onChange={(e) => setSymbolForm((f) => ({ ...f, institutionSymbolName: e.target.value }))}
              />
            </div>
            <div className="col-md-3">
              <label className="form-label fw-semibold">סוג מוסד</label>
              <select
                className="form-select"
                value={symbolForm.institutionType}
                onChange={(e) => setSymbolForm((f) => ({ ...f, institutionType: e.target.value }))}
              >
                {INSTITUTION_TYPE_OPTIONS.map((opt) => (
                  <option key={opt} value={opt}>
                    {opt}
                  </option>
                ))}
              </select>
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
                    <th>סוג מוסד</th>
                    <th></th>
                  </tr>
                </thead>
                <tbody>
                  {institutionSymbols.map((symbol) => (
                    <tr key={symbol.id}>
                      <td className="fw-semibold">{symbol.institutionSymbol}</td>
                      {editingId === symbol.id ? (
                        <>
                          <td>
                            <input
                              type="text"
                              className="form-control form-control-sm"
                              value={editForm.institutionSymbolName}
                              onChange={(e) =>
                                setEditForm((f) => ({ ...f, institutionSymbolName: e.target.value }))
                              }
                            />
                          </td>
                          <td>
                            <select
                              className="form-select form-select-sm"
                              value={editForm.institutionType}
                              onChange={(e) =>
                                setEditForm((f) => ({ ...f, institutionType: e.target.value }))
                              }
                            >
                              {INSTITUTION_TYPE_OPTIONS.map((opt) => (
                                <option key={opt} value={opt}>
                                  {opt}
                                </option>
                              ))}
                            </select>
                          </td>
                          <td className="text-start text-nowrap">
                            <button
                              type="button"
                              className="btn btn-sm btn-success me-1"
                              onClick={() => handleSaveEdit(symbol.id)}
                            >
                              <i className="bi bi-check-lg"></i>
                            </button>
                            <button type="button" className="btn btn-sm btn-outline-secondary" onClick={cancelEdit}>
                              <i className="bi bi-x-lg"></i>
                            </button>
                          </td>
                        </>
                      ) : (
                        <>
                          <td>{symbol.institutionSymbolName || '—'}</td>
                          <td>{normalizeInstitutionType(symbol.institutionType)}</td>
                          <td className="text-start text-nowrap">
                            <button
                              type="button"
                              className="btn btn-sm btn-outline-primary me-1"
                              onClick={() => startEdit(symbol)}
                              title="עריכה"
                            >
                              <i className="bi bi-pencil"></i>
                            </button>
                            <button
                              type="button"
                              className="btn btn-sm btn-outline-danger"
                              onClick={() => handleDeleteInstitutionSymbol(symbol)}
                            >
                              <i className="bi bi-trash"></i>
                            </button>
                          </td>
                        </>
                      )}
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
