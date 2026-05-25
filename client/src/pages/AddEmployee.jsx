import { useEffect, useState } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { employeesApi, employersApi, employmentDataApi } from '../api';
import {
  initSlots,
  buildEmploymentPayloadFromForm,
  EmploymentDataFormSections,
  defaultBandFields,
  bandFieldKeys,
  GRADE_OPTIONS,
  ROLE_OPTIONS,
  mergeSlotAndSyncTeacherExtras,
  patchEmploymentTotalsThenJobPercents,
  validateAddEmployeeEmploymentSection,
  shouldCreateEmploymentDataWithAddEmployeeForm,
} from '../employmentDataHelpers';
import './AddEmployee.css';

const initForm = {
  // Employee identity
  lastName: '',
  firstName: '',
  idNumber: '',
  employeeNumber: '',
  gender: '',
  birthDate: '',
  phone: '',
  childBirthDate1: '',
  childBirthDate2: '',
  childBirthDate3: '',
  childBirthDate4: '',
  childBirthDate5: '',
  childBirthDate6: '',
  childBirthDate7: '',
  childBirthDate8: '',
  childBirthDate9: '',
  childBirthDate10: '',
  // Employment data on this page is optional until the user selects a Hebrew academic year plus structured rows.
  academicYear: '',
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
};

export default function AddEmployee() {
  const { employerId } = useParams();
  const navigate = useNavigate();
  const [form, setForm] = useState(initForm);
  const [institutionSymbols, setInstitutionSymbols] = useState([]);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState(null);

  useEffect(() => {
    employersApi.getInstitutionSymbols(employerId)
      .then(res => setInstitutionSymbols(res.data))
      .catch(() => setError('שגיאה בטעינת סמלי המוסד.'));
  }, [employerId]);

  const set = (f) => (e) => {
    const val = e.target.type === 'checkbox' ? e.target.checked : e.target.value;
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

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!form.lastName.trim()) {
      setError('משפחה היא שדה חובה.');
      return;
    }
    if (!form.firstName.trim()) {
      setError('פרטי הוא שדה חובה.');
      return;
    }
    if (!form.idNumber.trim()) {
      setError('תעודת זהות היא שדה חובה.');
      return;
    }
    if (!form.gender) {
      setError('מין הוא שדה חובה.');
      return;
    }
    if (!form.birthDate) {
      setError('תאריך לידה הוא שדה חובה.');
      return;
    }
    const birth = new Date(`${form.birthDate}T12:00:00`);
    const todayEnd = new Date();
    todayEnd.setHours(23, 59, 59, 999);
    if (birth > todayEnd) {
      setError('תאריך לידה לא יכול להיות בעתיד — בדרך כלל טעות בשנת הלידה (למשל 1976 במקום 2026).');
      return;
    }

    const employmentSectionErr = validateAddEmployeeEmploymentSection(form);
    if (employmentSectionErr) {
      setError(employmentSectionErr);
      return;
    }

    const eid = parseInt(employerId, 10);
    if (Number.isNaN(eid) || eid <= 0) {
      setError('מזהה מעסיק לא תקין.');
      return;
    }

    let hintRes;
    try {
      hintRes = await employeesApi.precreateHint(eid, form.idNumber.trim());
    } catch {
      setError('לא ניתן לבדוק את סטטוס העובד מול השרת. נסו שוב.');
      return;
    }

    const h = hintRes.data;
    if (h?.employerMissing) {
      setError('המעסיק לא נמצא במערכת.');
      return;
    }

    if (
      h?.willRestoreSoftDeletedEmployee === true &&
      h?.hasActiveEmployeeWithSameTz !== true &&
      !window.confirm(
        'עובד זה היה קיים בעבר במערכת. האם לשחזר את רשומת העובד?',
      )
    ) {
      return;
    }

    const postEmployment = shouldCreateEmploymentDataWithAddEmployeeForm(form);

    setSaving(true);
    setError(null);
    try {
      const empRes = await employeesApi.create({
        employerId: parseInt(employerId, 10),
        idNumber: form.idNumber.trim(),
        firstName: form.firstName.trim(),
        lastName: form.lastName.trim(),
        employeeNumber: parseOptionalInt(form.employeeNumber),
        birthDate: form.birthDate,
        gender: form.gender,
        phone: form.phone || null,
        childBirthDate1: form.childBirthDate1 || null,
        childBirthDate2: form.childBirthDate2 || null,
        childBirthDate3: form.childBirthDate3 || null,
        childBirthDate4: form.childBirthDate4 || null,
        childBirthDate5: form.childBirthDate5 || null,
        childBirthDate6: form.childBirthDate6 || null,
        childBirthDate7: form.childBirthDate7 || null,
        childBirthDate8: form.childBirthDate8 || null,
        childBirthDate9: form.childBirthDate9 || null,
        childBirthDate10: form.childBirthDate10 || null,
      });
      if (postEmployment) {
        await employmentDataApi.create(
          buildEmploymentPayloadFromForm(
            empRes.data.id,
            parseInt(employerId, 10),
            patchEmploymentTotalsThenJobPercents(form),
          ),
        );
      }
      const restored = empRes.data?.restoredFromSoftDelete === true;
      let successMsg;
      if (restored) {
        successMsg = postEmployment
          ? 'רשומת העובד עם תעודת זהות זו הייתה מוסתרת (מחיקה רכה); היא שוחזרה ועודכנה, ונתוני העסקה נשמרו.'
          : 'רשומת העובד עם תעודת זהות זו הייתה מוסתרת (מחיקה רכה); היא שוחזרה ועודכנה.';
      } else {
        successMsg = postEmployment
          ? 'העובד נוסף בהצלחה עם נתוני העסקה.'
          : 'העובד נוסף בהצלחה. ניתן להוסיף נתוני העסקה מעמוד העובד.';
      }
      navigate(`/employers/${employerId}`, {
        state: { success: successMsg },
      });
    } catch (err) {
      const d = err.response?.data;
      const status = err.response?.status;
      /** Avoid dumping ASP.NET DeveloperExceptionPage HTML into the banner. */
      const fromJsonObject =
        d && typeof d === 'object' && !Array.isArray(d)
          ? d.message ?? d.title ?? d.detail ?? null
          : null;
      const fromPlainString =
        typeof d === 'string' &&
        !/^\s*</.test(d) &&
        !d.toLowerCase().includes('<!doctype')
          ? d.trim()
          : null;
      const duplicateHint =
        status === 500 && !fromJsonObject && !fromPlainString
          ? 'הפעולה נכשלה בשרת. אם הוספתם עובד עם תז של עובד שמחקתם בעבר — הפעילו מחדש את ה־API לאחר הבנייה, והריצו את מיגרציות מסד הנתונים. אחרת ודאו שאין חפיפת תז באותו מעסיק.'
          : null;
      const msg =
        fromJsonObject ||
        fromPlainString ||
        duplicateHint ||
        (err.code === 'ERR_NETWORK'
          ? 'לא ניתן להתחבר לשרת. ודאי שהשרת רץ ובכתובת הנכונה (למשל https://localhost:7068).'
          : null) ||
        err.message;
      setError(msg?.trim() || 'שגיאה בשמירה.');
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="pf-wrap" dir="rtl">
      <div className="breadcrumb-area mb-2">
        <nav>
          <ol className="breadcrumb mb-0">
            <li className="breadcrumb-item">
              <Link to="/">מעסיקים</Link>
            </li>
            <li className="breadcrumb-item">
              <Link to={`/employers/${employerId}`}>חזרה</Link>
            </li>
            <li className="breadcrumb-item active">הוספת עובד חדש</li>
          </ol>
        </nav>
      </div>

      {error && (
        <div className="alert alert-danger py-1 mb-2 d-flex justify-content-between align-items-center">
          <span>
            <i className="bi bi-exclamation-triangle me-2"></i>
            {error}
          </span>
          <button className="btn-close btn-sm" type="button" onClick={() => setError(null)}></button>
        </div>
      )}

      <form onSubmit={handleSubmit}>
        <div className="card mb-3">
          <div className="card-header py-2 d-flex align-items-center gap-2">
            <i className="bi bi-person-vcard"></i>
            <span className="fw-semibold">נתוני עובד</span>
          </div>
          <div className="card-body">
            <div className="pf-row">
              <Field label="משפחה *">
                <Txt f="lastName" w={90} form={form} set={set} required />
              </Field>
              <Field label="פרטי *">
                <Txt f="firstName" w={90} form={form} set={set} required />
              </Field>
              <Field label="ת.ז. *">
                <input
                  type="text"
                  className="pf-txt"
                  style={{ width: 90 }}
                  value={form.idNumber}
                  onChange={set('idNumber')}
                  required
                />
              </Field>
              <Field label="מספר עובד בעוקץ">
                <input
                  type="number"
                  className="pf-txt"
                  style={{ width: 90 }}
                  value={form.employeeNumber ?? ''}
                  onChange={set('employeeNumber')}
                  min={0}
                  step={1}
                />
              </Field>
              <Field label="מין *">
                <select className="pf-sel" style={{ width: 100 }} value={form.gender} onChange={set('gender')} required>
                  <option value=""></option>
                  <option value="זכר">זכר</option>
                  <option value="נקבה">נקבה</option>
                </select>
              </Field>
              <Field label="לידה *">
                <Dt f="birthDate" form={form} set={set} required />
              </Field>
              <Field label="טל">
                <Txt f="phone" w={80} form={form} set={set} />
              </Field>
            </div>
            <div className="pf-row pf-children-row">
              <span className="pf-child-title">ילדים:</span>
              {[1, 2, 3, 4, 5, 6, 7, 8, 9, 10].map((n) => (
                <div key={n} className="pf-child-cell">
                  <span className="pf-child-num">{n}</span>
                  <input
                    type="date"
                    className="pf-dt pf-dt-child"
                    value={form[`childBirthDate${n}`]}
                    onChange={set(`childBirthDate${n}`)}
                  />
                </div>
              ))}
            </div>
          </div>
        </div>

        <div className="card mb-3">
          <div className="card-header py-2 d-flex align-items-center gap-2">
            <i className="bi bi-briefcase"></i>
            <span className="fw-semibold">נתוני העסקה</span>
          </div>
          <div className="card-body">
            <p className="small text-muted mb-2">
              אפשר לשמור עובד בלבד ולמלא נתוני העסקה מאוחר מעמוד העובד. אם בוחרים שנת לימודים, יש להשלים לפחות
              דירוג או מקטע עם סמל מוסד ושעות.
            </p>
            <EmploymentDataFormSections
              form={form}
              set={set}
              setSlot={setSlot}
              setBandField={setBandField}
              institutionSymbols={institutionSymbols}
              academicYearOptional
            />
          </div>
        </div>
        <div className="pf-actions">
          <button type="submit" className="btn btn-success px-5" disabled={saving}>
            {saving ? (
              <>
                <span className="spinner-border spinner-border-sm me-2"></span>שומר...
              </>
            ) : (
              <>
                <i className="bi bi-floppy me-2"></i>שמירה ויציאה
              </>
            )}
          </button>
          <Link to={`/employers/${employerId}`} className="btn btn-outline-danger px-4">
            <i className="bi bi-x-lg me-2"></i>יציאה ללא שמירה
          </Link>
        </div>
      </form>
    </div>
  );
}

function Field({ label, children }) {
  return (
    <div className="pf-field">
      <div className="pf-lbl-top">{label}</div>
      {children}
    </div>
  );
}
function Txt({ f, w = 80, form, set, required }) {
  return (
    <input
      type="text"
      className="pf-txt"
      style={{ width: w }}
      value={form[f] ?? ''}
      onChange={set(f)}
      required={required}
    />
  );
}
function Dt({ f, form, set, required }) {
  return <input type="date" className="pf-dt" value={form[f] ?? ''} onChange={set(f)} required={required} />;
}

function parseOptionalInt(value) {
  const text = String(value ?? '').trim();
  if (!text) return null;
  const parsed = Number.parseInt(text, 10);
  return Number.isNaN(parsed) ? null : parsed;
}
