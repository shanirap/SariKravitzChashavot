import { useState, useEffect, useCallback } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { employersApi } from '../api';
import Pagination from '../components/Pagination';

const PAGE_SIZE = 50;
const emptyForm = { name: '', businessNumber: '', beneficiarySymbol: '', eketzNumber: '' };

export default function Employers() {
  const [employers, setEmployers] = useState([]);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState('');
  const [searchInput, setSearchInput] = useState('');
  const [loading, setLoading] = useState(true);
  const [alert, setAlert] = useState(null);
  const [form, setForm] = useState(emptyForm);
  const [editTarget, setEditTarget] = useState(null);
  const navigate = useNavigate();

  const load = useCallback(async (p = page, s = search) => {
    setLoading(true);
    try {
      const res = await employersApi.getAll({ page: p, pageSize: PAGE_SIZE, search: s || undefined });
      setEmployers(res.data.items);
      setTotalCount(res.data.totalCount);
    } catch {
      showAlert('danger', 'שגיאה בטעינת הנתונים.');
    } finally {
      setLoading(false);
    }
  }, [page, search]);

  useEffect(() => { load(page, search); }, [page, search]);

  const handleSearch = (e) => {
    e.preventDefault();
    setPage(1);
    setSearch(searchInput);
  };

  const clearSearch = () => {
    setSearchInput('');
    setSearch('');
    setPage(1);
  };

  const showAlert = (type, msg) => {
    setAlert({ type, msg });
    setTimeout(() => setAlert(null), 4000);
  };

  const handleAdd = async (e) => {
    e.preventDefault();
    try {
      await employersApi.create(form);
      showAlert('success', 'המעסיק נשמר בהצלחה.');
      setForm(emptyForm);
      document.getElementById('closeAddModal').click();
      load(1, search);
    } catch (err) {
      showAlert('danger', err.response?.data?.message || 'שגיאה בהוספה.');
    }
  };

  const openEdit = (emp) => {
    setEditTarget(emp);
    setForm({ name: emp.name, businessNumber: emp.businessNumber || '', beneficiarySymbol: emp.beneficiarySymbol || '', eketzNumber: emp.eketzNumber || '' });
    document.getElementById('openEditModalBtn').click();
  };

  const handleEdit = async (e) => {
    e.preventDefault();
    try {
      await employersApi.update(editTarget.id, form);
      showAlert('success', `המעסיק "${form.name}" עודכן בהצלחה.`);
      setForm(emptyForm); setEditTarget(null);
      document.getElementById('closeEditModal').click();
      load(page, search);
    } catch (err) {
      showAlert('danger', err.response?.data?.message || 'שגיאה בעדכון.');
    }
  };

  const handleDelete = async (emp) => {
    if (!window.confirm(`האם למחוק את "${emp.name}"?`)) return;
    try {
      await employersApi.delete(emp.id);
      showAlert('success', `המעסיק "${emp.name}" נמחק.`);
      load(page, search);
    } catch (err) {
      showAlert('danger', err.response?.data?.message || 'שגיאה במחיקה.');
    }
  };

  return (
    <>
      <div className="d-flex justify-content-between align-items-center mb-4">
        <h1 className="page-title mb-0">
          <i className="bi bi-building me-2 text-primary"></i>מעסיקים
        </h1>
        <div className="d-flex gap-2 flex-wrap">
          <Link className="btn btn-outline-success btn-action px-3" to="/import/employers">
            <i className="bi bi-file-earmark-arrow-up me-2"></i>ייבוא מעסיקים מאקסל
          </Link>
          <button className="btn btn-primary btn-action px-4" data-bs-toggle="modal" data-bs-target="#addModal">
            <i className="bi bi-plus-circle me-2"></i>הוסף מעסיק
          </button>
        </div>
      </div>

      {alert && (
        <div className={`alert alert-${alert.type} alert-dismissible fade show`}>
          <i className={`bi bi-${alert.type === 'success' ? 'check-circle' : 'exclamation-triangle'} me-2`}></i>
          {alert.msg}
          <button className="btn-close" onClick={() => setAlert(null)}></button>
        </div>
      )}

      <div className="card">
        <div className="card-header d-flex align-items-center gap-2">
          <i className="bi bi-list-ul"></i>
          <span>רשימת מעסיקים ({totalCount})</span>
        </div>
        <div className="card-body">
          {/* חיפוש */}
          <form onSubmit={handleSearch} className="d-flex gap-2 mb-3">
            <div className="input-group" style={{ maxWidth: 380 }}>
              <span className="input-group-text"><i className="bi bi-search"></i></span>
              <input type="text" className="form-control" placeholder="חיפוש לפי שם, ח.פ., סמל מוטב..."
                value={searchInput} onChange={e => setSearchInput(e.target.value)} />
              {searchInput && (
                <button type="button" className="btn btn-outline-secondary" onClick={clearSearch}>
                  <i className="bi bi-x"></i>
                </button>
              )}
            </div>
            <button type="submit" className="btn btn-primary px-3">חפש</button>
          </form>

          {loading ? (
            <div className="text-center py-5"><div className="spinner-border text-primary"></div></div>
          ) : employers.length === 0 ? (
            <div className="empty-state">
              <i className="bi bi-building-x"></i>
              <p className="mb-0">{search ? `לא נמצאו תוצאות עבור "${search}"` : 'לא נמצאו מעסיקים.'}</p>
            </div>
          ) : (
            <>
              <div className="table-responsive">
                <table className="table table-hover mb-0">
                  <thead>
                    <tr>
                      <th>שם מעסיק</th>
                      <th>ח.פ.</th>
                      <th>סמל מוטב</th>
                      <th>מספר עוקץ</th>
                      <th></th>
                    </tr>
                  </thead>
                  <tbody>
                    {employers.map(emp => (
                      <tr key={emp.id}>
                        <td className="fw-semibold">
                          <Link to={`/employers/${emp.id}`} className="link-primary text-decoration-none">
                            {emp.name}
                          </Link>
                        </td>
                        <td>{emp.businessNumber || '—'}</td>
                        <td>{emp.beneficiarySymbol || '—'}</td>
                        <td>{emp.eketzNumber || '—'}</td>
                        <td className="text-start">
                          <button className="btn btn-sm btn-outline-primary btn-action"
                            onClick={() => navigate(`/employers/${emp.id}`)}>
                            <i className="bi bi-people me-1"></i>עובדים
                          </button>
                          <button className="btn btn-sm btn-outline-secondary btn-action ms-1"
                            onClick={() => openEdit(emp)}>
                            <i className="bi bi-pencil"></i>
                          </button>
                          <button className="btn btn-sm btn-outline-danger btn-action ms-1"
                            onClick={() => handleDelete(emp)}>
                            <i className="bi bi-trash"></i>
                          </button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
              <Pagination page={page} pageSize={PAGE_SIZE} totalCount={totalCount}
                onPage={p => setPage(p)} />
            </>
          )}
        </div>
      </div>

      {/* Modal הוסף */}
      <div className="modal fade" id="addModal" tabIndex="-1">
        <div className="modal-dialog">
          <div className="modal-content">
            <div className="modal-header">
              <h5 className="modal-title"><i className="bi bi-building-add me-2"></i>הוסף מעסיק חדש</h5>
              <button id="closeAddModal" type="button" className="btn-close" data-bs-dismiss="modal"></button>
            </div>
            <form onSubmit={handleAdd}>
              <div className="modal-body"><EmployerForm form={form} setForm={setForm} /></div>
              <div className="modal-footer">
                <button type="button" className="btn btn-secondary" data-bs-dismiss="modal">ביטול</button>
                <button type="submit" className="btn btn-primary"><i className="bi bi-check-lg me-1"></i>שמור</button>
              </div>
            </form>
          </div>
        </div>
      </div>

      {/* Modal עריכה */}
      <button id="openEditModalBtn" className="d-none" data-bs-toggle="modal" data-bs-target="#editModal"></button>
      <div className="modal fade" id="editModal" tabIndex="-1">
        <div className="modal-dialog">
          <div className="modal-content">
            <div className="modal-header">
              <h5 className="modal-title"><i className="bi bi-pencil-square me-2"></i>עריכת מעסיק</h5>
              <button id="closeEditModal" type="button" className="btn-close" data-bs-dismiss="modal"></button>
            </div>
            <form onSubmit={handleEdit}>
              <div className="modal-body"><EmployerForm form={form} setForm={setForm} /></div>
              <div className="modal-footer">
                <button type="button" className="btn btn-secondary" data-bs-dismiss="modal">ביטול</button>
                <button type="submit" className="btn btn-primary"><i className="bi bi-check-lg me-1"></i>עדכן</button>
              </div>
            </form>
          </div>
        </div>
      </div>
    </>
  );
}

function EmployerForm({ form, setForm }) {
  const set = (field) => (e) => setForm(f => ({ ...f, [field]: e.target.value }));
  return (
    <>
      <div className="mb-3">
        <label className="form-label fw-semibold">שם מעסיק <span className="text-danger">*</span></label>
        <input type="text" className="form-control" value={form.name} onChange={set('name')} required maxLength={200} />
      </div>
      <div className="row g-3">
        <div className="col-6">
          <label className="form-label fw-semibold">ח.פ.</label>
          <input type="text" className="form-control" value={form.businessNumber} onChange={set('businessNumber')} maxLength={50} />
        </div>
        <div className="col-6">
          <label className="form-label fw-semibold">סמל מוטב</label>
          <input type="text" className="form-control" value={form.beneficiarySymbol} onChange={set('beneficiarySymbol')} maxLength={50} />
        </div>
      </div>
      <div className="mt-3">
        <label className="form-label fw-semibold">מספר עוקץ</label>
        <input type="text" className="form-control" value={form.eketzNumber} onChange={set('eketzNumber')} maxLength={50} />
      </div>
    </>
  );
}
