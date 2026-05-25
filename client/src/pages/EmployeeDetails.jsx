import { useState, useEffect, useRef } from 'react';
import { useParams, Link, useSearchParams, useNavigate } from 'react-router-dom';
import { employersApi, employeesApi, employmentDataApi } from '../api';
import { formatDateDDMMYYYY } from '../utils/dateFormat';
import {
  buildEmploymentPayloadFromForm,
  apiRecordToFormSlots,
  apiRecordBandFields,
  EmploymentDataFormSections,
  EmploymentDataRecordDisplay,
  bandFieldKeys,
  GRADE_OPTIONS,
  ROLE_OPTIONS,
  mergeSlotAndSyncTeacherExtras,
  patchEmploymentTotalsThenJobPercents,
  childBirthDateFieldsFromEmployee,
  currentHebrewAcademicYear,
  defaultBandFields,
  initSlots,
} from '../employmentDataHelpers';

function createEmploymentFormSetters(setForm) {
  const set = (f) => (e) => {
    const val = e.target.value;
    setForm((p) => patchEmploymentTotalsThenJobPercents({ ...p, [f]: val }));
  };
  const setSlot = (idx, field) => (e) => {
    const v = e.target.value;
    setForm((p) => mergeSlotAndSyncTeacherExtras(p, idx, field, v));
  };
  const setBandField = (band, field) => (e) => {
    const v = e.target.value;
    const keys = bandFieldKeys(band);
    setForm((p) => {
      if (field === 'gradeName') {
        const newGrade = GRADE_OPTIONS[v]?.includes(p[keys.grade]) ? p[keys.grade] : '';
        const newRole = ROLE_OPTIONS[v]?.includes(p[keys.role]) ? p[keys.role] : '';
        const next = { ...p, [keys.gradeName]: v, [keys.grade]: newGrade, [keys.role]: newRole };
        return patchEmploymentTotalsThenJobPercents(next);
      }
      if (field === 'role') {
        const next = { ...p, [keys.role]: v };
        return patchEmploymentTotalsThenJobPercents(next);
      }
      return patchEmploymentTotalsThenJobPercents({ ...p, [keys[field]]: v });
    });
  };
  return { set, setSlot, setBandField };
}

const newEmploymentForm = (emp) =>
  patchEmploymentTotalsThenJobPercents({
    academicYear: currentHebrewAcademicYear(),
    grade1Total: '',
    grade1JobPercent: '',
    grade1TrainingFundPercent: '',
    grade1AgeHours: '',
    grade1MotherBenefitPercent: '',
    grade2Total: '',
    grade2JobPercent: '',
    grade2TrainingFundPercent: '',
    grade2AgeHours: '',
    grade2MotherBenefitPercent: '',
    grade1TrainingBenefits: '',
    grade1DoubleDegree: '',
    grade2TrainingBenefits: '',
    grade2DoubleDegree: '',
    ...defaultBandFields(),
    slots: initSlots(),
    ...childBirthDateFieldsFromEmployee(emp),
  });

const toFormRec = (rec, emp) =>
  patchEmploymentTotalsThenJobPercents({
    academicYear: rec.academicYear,
    grade1Total: rec.grade1Total ?? '',
    grade1JobPercent: rec.grade1JobPercent ?? '',
    grade1TrainingFundPercent: rec.grade1TrainingFundPercent ?? '',
    grade1AgeHours: rec.grade1AgeHours ?? '',
    grade1MotherBenefitPercent: rec.grade1MotherBenefitPercent ?? '',
    grade2Total: rec.grade2Total ?? '',
    grade2JobPercent: rec.grade2JobPercent ?? '',
    grade2TrainingFundPercent: rec.grade2TrainingFundPercent ?? '',
    grade2AgeHours: rec.grade2AgeHours ?? '',
    grade2MotherBenefitPercent: rec.grade2MotherBenefitPercent ?? '',
    grade1TrainingBenefits: rec.grade1TrainingBenefits ?? '',
    grade1DoubleDegree: rec.grade1DoubleDegree ?? '',
    grade2TrainingBenefits: rec.grade2TrainingBenefits ?? '',
    grade2DoubleDegree: rec.grade2DoubleDegree ?? '',
    ...apiRecordBandFields(rec),
    slots: apiRecordToFormSlots(rec),
    ...childBirthDateFieldsFromEmployee(emp),
  });


export default function EmployeeDetails() {
  const { employeeId, employerId } = useParams();
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();
  const autoOpenAddFromUrlDone = useRef(false);
  const employmentAddSectionRef = useRef(null);
  const [blockedReason, setBlockedReason] = useState(null);
  const [employee, setEmployee] = useState(null);
  const [employer, setEmployer] = useState(null);
  const [records, setRecords] = useState([]);
  const [institutionSymbols, setInstitutionSymbols] = useState([]);
  const [loading, setLoading] = useState(true);
  const [alert, setAlert] = useState(null);
  const [editRecord, setEditRecord] = useState(null);
  const [editForm, setEditForm] = useState(null);
  const [createForm, setCreateForm] = useState(null);
  const [showEmploymentAddInline, setShowEmploymentAddInline] = useState(false);
  const [saving, setSaving] = useState(false);

  const editFns = createEmploymentFormSetters(setEditForm);
  const createFns = createEmploymentFormSetters(setCreateForm);

  const showAlert = (type, msg) => {
    setAlert({ type, msg });
    setTimeout(() => setAlert(null), 4000);
  };

  const load = async () => {
    setLoading(true);
    setBlockedReason(null);
    setEmployee(null);
    setEmployer(null);
    setRecords([]);
    setInstitutionSymbols([]);
    setCreateForm(null);
    setShowEmploymentAddInline(false);
    try {
      const [empRes, emplrRes] = await Promise.all([
        employeesApi.getById(employeeId),
        employersApi.getById(employerId),
      ]);
      const emp = empRes.data;
      const emplr = emplrRes.data;
      if (Number(emp.employerId) !== Number(employerId)) {
        setBlockedReason('הקישור אינו תקין — העובד אינו משויך למעסיק בכתובת זו.');
        return;
      }
      setEmployee(emp);
      setEmployer(emplr);

      try {
        const dataRes = await employmentDataApi.getByEmployeeAndEmployer(
          employeeId,
          employerId
        );
        setRecords(dataRes.data ?? []);
      } catch {
        showAlert('danger', 'שגיאה בטעינת רשימת נתוני העסקה.');
      }

      try {
        const symbolsRes = await employersApi.getInstitutionSymbols(employerId);
        setInstitutionSymbols(symbolsRes.data ?? []);
      } catch {
        showAlert(
          'warning',
          'לא נטענו סמלי מוסד — אפשר להמשיך, אך ייתכן שחסרים ערכים בבחירת סמל.'
        );
      }
    } catch (err) {
      const status = err.response?.status;
      const conn =
        err.code === 'ERR_NETWORK' ||
        String(err.message || '').includes('Network Error');
      const msg =
        status === 404
          ? 'עובד או מעסיק לא נמצא במערכת (ייתכן שהמזהה שגוי).'
          : conn
            ? 'לא נוצר קשר לשרת. ודאו שה־API רץ (dotnet run) ונסו מהדפדפן את אותו מיקום עם ה־CORS שהוגדר (למשל http://localhost:5173 עם השרת ב־5036).'
            : err.response?.data?.message ||
              err.message ||
              'שגיאה בטעינת הנתונים.';
      setBlockedReason(typeof msg === 'string' ? msg : 'שגיאה בטעינת הנתונים.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    setBlockedReason(null);
    autoOpenAddFromUrlDone.current = false;
    load();
  }, [employeeId, employerId]);

  useEffect(() => {
    if (loading || !employee || !employer) return;
    if (searchParams.get('addEmployment') !== '1') return;
    if (autoOpenAddFromUrlDone.current) return;
    autoOpenAddFromUrlDone.current = true;
    setSearchParams({}, { replace: true });
    if (records.length > 0) return;
    setCreateForm(newEmploymentForm(employee));
    setShowEmploymentAddInline(true);
  }, [loading, employee, employer, records, searchParams, setSearchParams]);

  useEffect(() => {
    if (!showEmploymentAddInline || !createForm) return undefined;
    const id = requestAnimationFrame(() => {
      employmentAddSectionRef.current?.scrollIntoView?.({ behavior: 'smooth', block: 'start' });
    });
    return () => cancelAnimationFrame(id);
  }, [showEmploymentAddInline, createForm]);

  const openAdd = () => {
    if (!employee) return;
    setCreateForm(newEmploymentForm(employee));
    setShowEmploymentAddInline(true);
  };

  const cancelInlineAdd = () => {
    setCreateForm(null);
    setShowEmploymentAddInline(false);
  };

  const openEdit = (rec) => {
    setEditRecord(rec);
    setEditForm(toFormRec(rec, employee));
    document.getElementById('openEditBtn').click();
  };

  const handleEdit = async (e) => {
    e.preventDefault();
    if (!String(editForm.academicYear ?? '').trim()) {
      showAlert('danger', 'שנת לימודים נדרשת.');
      return;
    }
    setSaving(true);
    try {
      await employmentDataApi.update(
        editRecord.id,
        buildEmploymentPayloadFromForm(
          parseInt(employeeId, 10),
          parseInt(employerId, 10),
          patchEmploymentTotalsThenJobPercents(editForm)
        )
      );
      showAlert('success', 'הרשומה עודכנה בהצלחה.');
      document.getElementById('closeEditModal').click();
      load();
    } catch (err) {
      showAlert('danger', err.response?.data?.message || 'שגיאה בעדכון.');
    } finally {
      setSaving(false);
    }
  };

  const handleCreate = async (e) => {
    e.preventDefault();
    if (!String(createForm.academicYear ?? '').trim()) {
      showAlert('danger', 'שנת לימודים נדרשת.');
      return;
    }
    setSaving(true);
    try {
      await employmentDataApi.create(
        buildEmploymentPayloadFromForm(
          parseInt(employeeId, 10),
          parseInt(employerId, 10),
          patchEmploymentTotalsThenJobPercents(createForm)
        )
      );
      setCreateForm(null);
      setShowEmploymentAddInline(false);
      navigate(`/employers/${employerId}`, {
        replace: false,
        state: { success: 'נתוני העסקה נשמרו בהצלחה.' },
      });
    } catch (err) {
      showAlert('danger', err.response?.data?.message || 'שגיאה בשמירה.');
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async (id) => {
    if (!window.confirm('האם למחוק רשומה זו?')) return;
    try {
      await employmentDataApi.delete(id);
      showAlert('success', 'הרשומה נמחקה.');
      load();
    } catch {
      showAlert('danger', 'שגיאה במחיקה.');
    }
  };

  const fmt = (v) => (v == null || v === '' ? '—' : v);
  const fmtNum = (v) =>
    v == null ? '—' : Number(v).toLocaleString('he-IL', { minimumFractionDigits: 2, maximumFractionDigits: 2 });

  if (loading) {
    return (
      <div className="text-center py-5">
        <div className="spinner-border text-primary"></div>
      </div>
    );
  }
  if (blockedReason) {
    return (
      <div className="container py-4">
        <div className="alert alert-danger" role="alert">
          <i className="bi bi-exclamation-triangle-fill me-2"></i>
          {blockedReason}
        </div>
        <Link to={`/employers/${employerId}`} className="btn btn-outline-primary">
          חזרה למסך המעסיק
        </Link>
      </div>
    );
  }

  if (!employee || !employer) {
    return <div className="alert alert-danger">הנתונים לא נמצאו.</div>;
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
              <Link to={`/employers/${employerId}`}>{employer.name}</Link>
            </li>
            <li className="breadcrumb-item active">{employee.fullName}</li>
          </ol>
        </nav>
      </div>

      <div className="info-card">
        <div className="info-title">
          <i className="bi bi-person-badge me-2 text-primary"></i>
          {employee.fullName}
        </div>
        <div className="row g-4">
          <div className="col-auto">
            <div className="field-label">תעודת זהות</div>
            <div className="field-value">{employee.idNumber}</div>
          </div>
          <div className="col-auto">
            <div className="field-label">מספר עובד בעוקץ</div>
            <div className="field-value">{employee.employeeNumber || '—'}</div>
          </div>
          <div className="col-auto">
            <div className="field-label">תאריך לידה</div>
            <div className="field-value">
              {formatDateDDMMYYYY(employee.birthDate)}
            </div>
          </div>
          <div className="col-auto">
            <div className="field-label">מין</div>
            <div className="field-value">{employee.gender || '—'}</div>
          </div>
          <div className="col-auto">
            <div className="field-label">מעסיק</div>
            <div className="field-value fw-semibold text-primary">{employer.name}</div>
          </div>
        </div>
      </div>

      {alert && (
        <div className={`alert alert-${alert.type} alert-dismissible fade show`}>
          <i
            className={`bi bi-${
              alert.type === 'success' ? 'check-circle' : 'exclamation-triangle'
            } me-2`}
          ></i>
          {alert.msg}
          <button className="btn-close" type="button" onClick={() => setAlert(null)}></button>
        </div>
      )}

      <div className="mb-3">
        <h2 className="page-title mb-0" style={{ fontSize: '1.3rem' }}>
          <i className="bi bi-file-earmark-text me-2 text-primary"></i>נתוני העסקה
        </h2>
        {records.length === 0 && showEmploymentAddInline && createForm ? (
          <p className="text-muted small mb-0 mt-2">
            מלאו את הנתונים לעובד זה. עם השמירה תועברו לרשימת העובדים.
          </p>
        ) : null}
      </div>

      <div className="card">
        <div className="card-header d-flex align-items-center gap-2 flex-wrap">
          <i className={`bi ${records.length > 0 ? 'bi-file-earmark-text' : 'bi-table'}`}></i>
          {records.length === 0 && showEmploymentAddInline && createForm ? (
            <span>טופס — הוספת נתוני העסקה</span>
          ) : (
            <span>רשומות ({records.length})</span>
          )}
        </div>
        <div className="card-body p-0">
          {records.length === 0 && showEmploymentAddInline && createForm ? (
            <form
              id="employment-add-section"
              ref={employmentAddSectionRef}
              onSubmit={handleCreate}
              className="p-3"
            >
              <div className="p-2 p-md-3">
                <EmploymentDataFormSections
                  form={createForm}
                  set={createFns.set}
                  setSlot={createFns.setSlot}
                  setBandField={createFns.setBandField}
                  institutionSymbols={institutionSymbols}
                />
              </div>
              <div className="border-top bg-light px-3 py-3 d-flex gap-2 flex-wrap justify-content-end">
                <button
                  type="button"
                  className="btn btn-outline-secondary"
                  onClick={cancelInlineAdd}
                  disabled={saving}
                >
                  ביטול
                </button>
                <button type="submit" className="btn btn-primary px-4" disabled={saving}>
                  {saving ? (
                    <>
                      <span className="spinner-border spinner-border-sm me-2"></span>שומר...
                    </>
                  ) : (
                    <>
                      <i className="bi bi-check-lg me-1"></i>שמור
                    </>
                  )}
                </button>
              </div>
            </form>
          ) : records.length === 0 ? (
            <div className="empty-state py-4 px-3">
              <i className="bi bi-file-earmark-x"></i>
              <p className="mb-3">לא נמצאו נתוני העסקה.</p>
              <button type="button" className="btn btn-primary" onClick={openAdd}>
                <i className="bi bi-plus-circle me-1"></i>הוסף נתוני העסקה
              </button>
            </div>
          ) : (
            <div className="employment-records-detail">
              {records.map((rec, idx) => (
                <div
                  key={rec.id}
                  className={`px-3 py-3${idx < records.length - 1 ? ' border-bottom' : ''}`}
                >
                  <div className="d-flex flex-wrap justify-content-between align-items-center gap-2 mb-3">
                    <div className="d-flex align-items-center gap-2">
                      <span className="text-muted small">שנת לימודים</span>
                      <span className="badge-month">{rec.periodDisplay ?? rec.academicYear}</span>
                    </div>
                    <div>
                      <button
                        type="button"
                        className="btn btn-sm btn-outline-secondary btn-action"
                        title="עריכה"
                        onClick={() => openEdit(rec)}
                      >
                        <i className="bi bi-pencil"></i>
                      </button>
                      <button
                        type="button"
                        className="btn btn-sm btn-outline-danger btn-action ms-1"
                        title="מחיקה"
                        onClick={() => handleDelete(rec.id)}
                      >
                        <i className="bi bi-trash"></i>
                      </button>
                    </div>
                  </div>
                  <EmploymentDataRecordDisplay
                    rec={rec}
                    fmt={fmt}
                    fmtNum={fmtNum}
                    omitYearRow
                  />
                </div>
              ))}
            </div>
          )}
        </div>
      </div>

      <button id="openEditBtn" className="d-none" data-bs-toggle="modal" data-bs-target="#editModal"></button>
      <div className="modal fade" id="editModal" tabIndex="-1">
        <div className="modal-dialog modal-xl">
          <div className="modal-content">
            <div className="modal-header">
              <h5 className="modal-title">
                <i className="bi bi-pencil-square me-2"></i>עריכת נתוני העסקה
              </h5>
              <button id="closeEditModal" type="button" className="btn-close" data-bs-dismiss="modal"></button>
            </div>
            {editForm && (
              <form onSubmit={handleEdit}>
                <div className="modal-body" style={{ maxHeight: '80vh', overflowY: 'auto' }}>
                  <EmploymentDataFormSections
                    form={editForm}
                    set={editFns.set}
                    setSlot={editFns.setSlot}
                    setBandField={editFns.setBandField}
                    institutionSymbols={institutionSymbols}
                  />
                </div>
                <div className="modal-footer">
                  <button type="button" className="btn btn-secondary" data-bs-dismiss="modal">
                    ביטול
                  </button>
                  <button type="submit" className="btn btn-primary" disabled={saving}>
                    {saving ? (
                      <>
                        <span className="spinner-border spinner-border-sm me-2"></span>שומר...
                      </>
                    ) : (
                      <>
                        <i className="bi bi-check-lg me-1"></i>שמור
                      </>
                    )}
                  </button>
                </div>
              </form>
            )}
          </div>
        </div>
      </div>

    </>
  );
}



