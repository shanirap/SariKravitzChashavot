import { useState, useEffect, useCallback } from 'react';
import { useParams, useNavigate, Link, useLocation } from 'react-router-dom';
import { employersApi, employeesApi } from '../api';
import Pagination from '../components/Pagination';
import { formatDateDDMMYYYY } from '../utils/dateFormat';

const PAGE_SIZE = 50;

const emptyEmp = {
  idNumber: '', firstName: '', lastName: '', employeeNumber: '', phone: '', birthDate: '', gender: '',
  childBirthDate1: '', childBirthDate2: '', childBirthDate3: '', childBirthDate4: '', childBirthDate5: '',
  childBirthDate6: '', childBirthDate7: '', childBirthDate8: '', childBirthDate9: '', childBirthDate10: ''
};

export default function EmployerDetails() {
  const { id } = useParams();
  const navigate = useNavigate();
  const location = useLocation();
  const [employer, setEmployer] = useState(null);
  const [employees, setEmployees] = useState([]);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState('');
  const [searchInput, setSearchInput] = useState('');
  const [loading, setLoading] = useState(true);
  const [alert, setAlert] = useState(location.state?.success ? { type: 'success', msg: location.state.success } : null);
  const [form, setForm] = useState(emptyEmp);
  const [editTarget, setEditTarget] = useState(null);

  const loadEmployees = useCallback(async (p = page, s = search) => {
    try {
      const res = await employersApi.getEmployees(id, { page: p, pageSize: PAGE_SIZE, search: s || undefined });
      setEmployees(res.data.items);
      setTotalCount(res.data.totalCount);
    } catch {
      showAlert('danger', 'שגיאה בטעינת עובדים.');
    }
  }, [id, page, search]);

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const empRes = await employersApi.getById(id);
      setEmployer(empRes.data);
    } catch {
      showAlert('danger', 'שגיאה בטעינת הנתונים.');
    } finally {
      setLoading(false);
    }
  }, [id]);

  useEffect(() => { load(); }, [id]);
  useEffect(() => { loadEmployees(page, search); }, [page, search]);

  useEffect(() => {
    if (alert) {
      const t = setTimeout(() => setAlert(null), 4000);
      return () => clearTimeout(t);
    }
  }, [alert]);

  const showAlert = (type, msg) => setAlert({ type, msg });

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

  const handleDeleteEmployee = async (emp) => {
    if (!window.confirm(`האם למחוק את "${emp.fullName || emp.idNumber}"?\nפעולה זו אפשרית רק אם אין לעובד נתוני העסקה.`)) return;
    try {
      await employeesApi.delete(emp.id);
      showAlert('success', `העובד "${emp.fullName || emp.idNumber}" נמחק.`);
      loadEmployees(page, search);
    } catch (err) {
      showAlert('danger', err.response?.data?.message || 'שגיאה במחיקה.');
    }
  };

  const handleToggleActiveStatus = async (emp) => {
    const nextIsActive = !emp.isActive;
    if (nextIsActive && !emp.hasEmploymentData) {
      showAlert('warning', 'לא ניתן להגדיר עובד כפעיל ללא נתוני העסקה.');
      return;
    }
    const msg = nextIsActive
      ? `להגדיר את "${emp.fullName || emp.idNumber}" כפעיל?`
      : `להגדיר את "${emp.fullName || emp.idNumber}" כלא פעיל?`;
    if (!window.confirm(msg)) return;
    try {
      await employeesApi.updateActiveStatus(emp.id, nextIsActive);
      showAlert('success', `סטטוס העובד עודכן ל-${nextIsActive ? 'פעיל' : 'לא פעיל'}.`);
      loadEmployees(page, search);
    } catch (err) {
      showAlert('danger', err.response?.data?.message || 'שגיאה בעדכון סטטוס.');
    }
  };

  const openEdit = (emp) => {
    setEditTarget(emp);
    setForm({
      idNumber: emp.idNumber,
      firstName: emp.firstName || '',
      lastName: emp.lastName || '',
      employeeNumber: emp.employeeNumber == null ? '' : String(emp.employeeNumber),
      phone: emp.phone || '',
      birthDate: emp.birthDate || '',
      gender: emp.gender || '',
      childBirthDate1:  emp.childBirthDate1  || '',
      childBirthDate2:  emp.childBirthDate2  || '',
      childBirthDate3:  emp.childBirthDate3  || '',
      childBirthDate4:  emp.childBirthDate4  || '',
      childBirthDate5:  emp.childBirthDate5  || '',
      childBirthDate6:  emp.childBirthDate6  || '',
      childBirthDate7:  emp.childBirthDate7  || '',
      childBirthDate8:  emp.childBirthDate8  || '',
      childBirthDate9:  emp.childBirthDate9  || '',
      childBirthDate10: emp.childBirthDate10 || '',
    });
    document.getElementById('openEditEmpBtn').click();
  };

  const handleEdit = async (e) => {
    e.preventDefault();
    try {
      await employeesApi.update(editTarget.id, {
        ...form,
        employerId: parseInt(id, 10),
        idNumber: form.idNumber.trim(),
        firstName: form.firstName.trim(),
        lastName: form.lastName.trim(),
        employeeNumber: parseOptionalInt(form.employeeNumber),
        birthDate: form.birthDate,
        gender: form.gender,
        phone: form.phone || null,
        childBirthDate1:  form.childBirthDate1  || null,
        childBirthDate2:  form.childBirthDate2  || null,
        childBirthDate3:  form.childBirthDate3  || null,
        childBirthDate4:  form.childBirthDate4  || null,
        childBirthDate5:  form.childBirthDate5  || null,
        childBirthDate6:  form.childBirthDate6  || null,
        childBirthDate7:  form.childBirthDate7  || null,
        childBirthDate8:  form.childBirthDate8  || null,
        childBirthDate9:  form.childBirthDate9  || null,
        childBirthDate10: form.childBirthDate10 || null,
      });
      showAlert('success', 'פרטי העובד עודכנו בהצלחה.');
      setForm(emptyEmp); setEditTarget(null);
      document.getElementById('closeEditEmpModal').click();
      loadEmployees(page, search);
    } catch (err) {
      showAlert('danger', err.response?.data?.message || 'שגיאה בעדכון.');
    }
  };

  if (loading) return <div className="text-center py-5"><div className="spinner-border text-primary"></div></div>;
  if (!employer) return <div className="alert alert-danger">המעסיק לא נמצא.</div>;

  return (
    <>
      <div className="breadcrumb-area">
        <nav><ol className="breadcrumb">
          <li className="breadcrumb-item"><Link to="/"><i className="bi bi-building me-1"></i>מעסיקים</Link></li>
          <li className="breadcrumb-item active">{employer.name}</li>
        </ol></nav>
      </div>

      {alert && (
        <div className={`alert alert-${alert.type} alert-dismissible fade show`}>
          <i className={`bi bi-${alert.type === 'success' ? 'check-circle' : 'exclamation-triangle'} me-2`}></i>
          {alert.msg}
          <button className="btn-close" onClick={() => setAlert(null)}></button>
        </div>
      )}

      <div className="d-flex justify-content-between align-items-center mb-3 flex-wrap gap-2">
        <h2 className="page-title mb-0" style={{ fontSize: '1.3rem' }}>
          <i className="bi bi-people me-2 text-primary"></i>עובדים — {employer.name}
        </h2>
        <div className="d-flex gap-2 flex-wrap">
          <button
            type="button"
            className="btn btn-outline-secondary btn-action px-4"
            onClick={() => navigate(`/employers/${id}/actions`)}
          >
            <i className="bi bi-grid-3x3-gap me-2"></i>פעולות ודוחות
          </button>
          <button
            type="button"
            className="btn btn-outline-primary btn-action px-4"
            onClick={() => navigate(`/employers/${id}/institution-symbols`)}
          >
            <i className="bi bi-bank me-2"></i>סמלי מוסד
          </button>
          <button className="btn btn-outline-info btn-action px-4"
            onClick={() => navigate(`/employers/${id}/import-employees`)}>
            <i className="bi bi-file-earmark-arrow-up me-2"></i>ייבוא עובדים מאקסל
          </button>
          <button className="btn btn-primary btn-action px-4"
            onClick={() => navigate(`/employers/${id}/add-employee`)}>
            <i className="bi bi-person-plus me-2"></i>הוסף עובד
          </button>
        </div>
      </div>

      <div className="card">
        <div className="card-header d-flex align-items-center gap-2">
          <i className="bi bi-people"></i>
          <span>רשימה ({totalCount})</span>
        </div>
        <div className="card-body">
          {/* חיפוש */}
          <form onSubmit={handleSearch} className="d-flex gap-2 mb-3">
            <div className="input-group" style={{ maxWidth: 360 }}>
              <span className="input-group-text"><i className="bi bi-search"></i></span>
              <input type="text" className="form-control" placeholder="חיפוש לפי שם, ת.ז., מספר עובד בעוקץ..."
                value={searchInput} onChange={e => setSearchInput(e.target.value)} />
              {searchInput && (
                <button type="button" className="btn btn-outline-secondary" onClick={clearSearch}>
                  <i className="bi bi-x"></i>
                </button>
              )}
            </div>
            <button type="submit" className="btn btn-primary px-3">חפש</button>
          </form>

          {employees.length === 0 ? (
            <div className="empty-state">
              <i className="bi bi-person-x"></i>
              <p className="mb-0">{search ? `לא נמצאו תוצאות עבור "${search}"` : 'לא נמצאו עובדים. לחץ על "הוסף עובד" להוסיף.'}</p>
            </div>
          ) : (
            <>
              <div className="table-responsive">
                <table className="table table-hover mb-0">
                  <thead>
                    <tr>
                      <th>שם מלא</th>
                      <th>ת.ז.</th>
                      <th>מספר עובד בעוקץ</th>
                      <th>תאריך לידה</th>
                      <th>מין</th>
                      <th>סטטוס</th>
                      <th></th>
                    </tr>
                  </thead>
                  <tbody>
                    {employees.map(emp => {
                      const cannotActivateWithoutEmploymentData = !emp.isActive && !emp.hasEmploymentData;
                      return (
                        <tr key={emp.id}>
                        <td className="fw-semibold">{emp.fullName || '—'}</td>
                        <td>{emp.idNumber}</td>
                        <td>{emp.employeeNumber || '—'}</td>
                        <td>{formatDateDDMMYYYY(emp.birthDate)}</td>
                        <td>{emp.gender || '—'}</td>
                        <td>
                          {emp.isActive ? (
                            <span className="badge bg-success">פעיל</span>
                          ) : (
                            <span className="badge bg-secondary">לא פעיל</span>
                          )}
                        </td>
                        <td className="text-start">
                          <button
                            className={`btn btn-sm btn-action ${
                              emp.hasEmploymentData ? 'btn-outline-primary' : 'btn-primary'
                            }`}
                            onClick={() =>
                              navigate(
                                emp.hasEmploymentData
                                  ? `/employees/${emp.id}/${id}`
                                  : `/employees/${emp.id}/${id}?addEmployment=1`
                              )
                            }
                            title={emp.hasEmploymentData ? 'נתוני העסקה' : 'הוספת נתוני העסקה'}
                          >
                            {emp.hasEmploymentData ? (
                              <>
                                <i className="bi bi-file-text me-1"></i>נתוני העסקה
                              </>
                            ) : (
                              <>
                                <i className="bi bi-plus-circle me-1"></i>הוסף נתוני העסקה
                              </>
                            )}
                          </button>
                          <button className="btn btn-sm btn-outline-secondary btn-action ms-1"
                            onClick={() => openEdit(emp)}>
                            <i className="bi bi-pencil"></i>
                          </button>
                          <button className="btn btn-sm btn-outline-danger btn-action ms-1"
                            onClick={() => handleDeleteEmployee(emp)}>
                            <i className="bi bi-trash"></i>
                          </button>
                          {!cannotActivateWithoutEmploymentData && (
                            <button
                              className={`btn btn-sm btn-action ms-1 ${
                                emp.isActive ? 'btn-outline-warning' : 'btn-outline-success'
                              }`}
                              onClick={() => handleToggleActiveStatus(emp)}
                              title={emp.isActive ? 'הפוך ללא פעיל' : 'הפוך לפעיל'}
                            >
                              {emp.isActive ? (
                                <><i className="bi bi-toggle-off me-1"></i>לא פעיל</>
                              ) : (
                                <><i className="bi bi-toggle-on me-1"></i>פעיל</>
                              )}
                            </button>
                          )}
                        </td>
                        </tr>
                      );
                    })}
                  </tbody>
                </table>
              </div>
              <Pagination page={page} pageSize={PAGE_SIZE} totalCount={totalCount}
                onPage={p => setPage(p)} />
            </>
          )}
        </div>
      </div>

      {/* Modal עריכת עובד */}
      <button id="openEditEmpBtn" className="d-none" data-bs-toggle="modal" data-bs-target="#editEmpModal"></button>
      <div className="modal fade" id="editEmpModal" tabIndex="-1">
        <div className="modal-dialog">
          <div className="modal-content">
            <div className="modal-header">
              <h5 className="modal-title"><i className="bi bi-person-gear me-2"></i>עריכת עובד</h5>
              <button id="closeEditEmpModal" type="button" className="btn-close" data-bs-dismiss="modal"></button>
            </div>
            <form onSubmit={handleEdit}>
              <div className="modal-body">
                <EmployeeForm form={form} setForm={setForm} />
              </div>
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

function EmployeeForm({ form, setForm }) {
  const set = (field) => (e) => setForm(f => ({ ...f, [field]: e.target.value }));
  return (
    <>
      <div className="row g-3">
        <div className="col-6">
          <label className="form-label fw-semibold">שם פרטי <span className="text-danger">*</span></label>
          <input type="text" className="form-control" value={form.firstName} onChange={set('firstName')} required maxLength={100} />
        </div>
        <div className="col-6">
          <label className="form-label fw-semibold">שם משפחה <span className="text-danger">*</span></label>
          <input type="text" className="form-control" value={form.lastName} onChange={set('lastName')} required maxLength={100} />
        </div>
      </div>
      <div className="row g-3 mt-1">
        <div className="col-6">
          <label className="form-label fw-semibold">תעודת זהות <span className="text-danger">*</span></label>
          <input type="text" className="form-control" value={form.idNumber} onChange={set('idNumber')} required maxLength={20} />
        </div>
        <div className="col-6">
          <label className="form-label fw-semibold">מספר עובד בעוקץ</label>
          <input type="number" className="form-control" value={form.employeeNumber || ''} onChange={set('employeeNumber')} min={0} step={1} />
        </div>
        <div className="col-6">
          <label className="form-label fw-semibold">טלפון</label>
          <input type="text" className="form-control" value={form.phone} onChange={set('phone')} maxLength={20} />
        </div>
      </div>
      <div className="row g-3 mt-1">
        <div className="col-6">
          <label className="form-label fw-semibold">תאריך לידה <span className="text-danger">*</span></label>
          <input type="date" className="form-control" value={form.birthDate} onChange={set('birthDate')} required />
        </div>
        <div className="col-6">
          <label className="form-label fw-semibold">מין <span className="text-danger">*</span></label>
          <select className="form-select" value={form.gender} onChange={set('gender')} required>
            <option value="">— בחר —</option>
            <option value="זכר">זכר</option>
            <option value="נקבה">נקבה</option>
          </select>
        </div>
      </div>

      <hr className="my-3" />
      <p className="fw-semibold text-muted mb-2" style={{ fontSize: '0.82rem' }}>
        <i className="bi bi-people me-1"></i>תאריכי לידה של ילדים
      </p>
      <div className="row g-2">
        {[1,2,3,4,5,6,7,8,9,10].map(n => (
          <div className="col-6" key={n}>
            <label className="form-label fw-semibold" style={{ fontSize: '0.8rem' }}>ילד {n}</label>
            <input
              type="date"
              className="form-control form-control-sm"
              value={form[`childBirthDate${n}`] || ''}
              onChange={set(`childBirthDate${n}`)}
            />
          </div>
        ))}
      </div>
    </>
  );
}

function parseOptionalInt(value) {
  const text = String(value ?? '').trim();
  if (!text) return null;
  const parsed = Number.parseInt(text, 10);
  return Number.isNaN(parsed) ? null : parsed;
}
