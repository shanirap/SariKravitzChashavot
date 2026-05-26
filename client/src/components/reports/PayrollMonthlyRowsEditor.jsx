import { useCallback, useEffect, useRef, useState } from 'react';
import { Modal as BSModal } from 'bootstrap';
import { payrollMonthlyInputsApi } from '../../api';
import { parseApiErrorMessage } from '../../utils/apiErrorMessage';

const MONTH_LABELS = {
  9: 'ספטמבר',
  10: 'אוקטובר',
  11: 'נובמבר',
  12: 'דצמבר',
  1: 'ינואר',
  2: 'פברואר',
  3: 'מרץ',
  4: 'אפריל',
  5: 'מאי',
  6: 'יוני',
  7: 'יולי',
  8: 'אוגוסט',
};

function formatCell(value) {
  if (value == null || value === '') return '—';
  return String(value);
}

function rowToForm(row) {
  return {
    institutionSymbol: row.institutionSymbol ?? '',
    oketzEmployeeNumber: row.oketzEmployeeNumber ?? '',
    idNumber: row.idNumber ?? '',
    fullName: row.fullName ?? '',
    role: row.role ?? '',
    grade: row.grade ?? '',
    seniority: row.seniority != null ? String(row.seniority) : '',
    weeklyHours: row.weeklyHours != null ? String(row.weeklyHours) : '',
    jobBase: row.jobBase != null ? String(row.jobBase) : '',
    jobPercent: row.jobPercent != null ? String(row.jobPercent) : '',
    ageHours: row.ageHours != null ? String(row.ageHours) : '',
    trainingBenefits: row.trainingBenefits != null ? String(row.trainingBenefits) : '',
    doubleDegree: row.doubleDegree != null ? String(row.doubleDegree) : '',
    trainingFund: row.trainingFund != null ? String(row.trainingFund) : '',
    generalMultiplier: row.generalMultiplier != null ? String(row.generalMultiplier) : '',
    manualEditNote: '',
  };
}

function parseOptionalDecimal(value) {
  const trimmed = String(value ?? '').trim();
  if (!trimmed) return null;
  const n = Number(trimmed.replace(',', '.'));
  return Number.isFinite(n) ? n : null;
}

function formToPayload(form) {
  return {
    institutionSymbol: form.institutionSymbol.trim() || null,
    oketzEmployeeNumber: form.oketzEmployeeNumber.trim() || null,
    idNumber: form.idNumber.trim() || null,
    fullName: form.fullName.trim() || null,
    role: form.role.trim() || null,
    grade: form.grade.trim() || null,
    seniority: parseOptionalDecimal(form.seniority),
    weeklyHours: parseOptionalDecimal(form.weeklyHours),
    jobBase: parseOptionalDecimal(form.jobBase),
    jobPercent: parseOptionalDecimal(form.jobPercent),
    ageHours: parseOptionalDecimal(form.ageHours),
    trainingBenefits: parseOptionalDecimal(form.trainingBenefits),
    doubleDegree: parseOptionalDecimal(form.doubleDegree),
    trainingFund: parseOptionalDecimal(form.trainingFund),
    generalMultiplier: parseOptionalDecimal(form.generalMultiplier),
    manualEditNote: form.manualEditNote.trim() || null,
  };
}

/**
 * עורך שורות קלט עוקץ חודשי לחודש בודד.
 * @param {{
 *   employerId: number,
 *   academicYear: string,
 *   month: number,
 *   onClose: () => void,
 *   onSaved?: () => void,
 * }} props
 */
export default function PayrollMonthlyRowsEditor({
  employerId,
  academicYear,
  month,
  onClose,
  onSaved,
}) {
  const listModalRef = useRef(null);
  const listBsModal = useRef(null);
  const editModalRef = useRef(null);
  const editBsModal = useRef(null);

  const [rows, setRows] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [alert, setAlert] = useState(null);
  const [editingRow, setEditingRow] = useState(null);
  const [editForm, setEditForm] = useState(null);
  const [saving, setSaving] = useState(false);

  const monthLabel = MONTH_LABELS[month] ?? `חודש ${month}`;

  const showAlert = useCallback((type, msg) => {
    setAlert({ type, msg });
    setTimeout(() => setAlert(null), 4500);
  }, []);

  const loadRows = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const res = await payrollMonthlyInputsApi.getRows(
        employerId,
        academicYear.trim(),
        month,
      );
      setRows(Array.isArray(res.data) ? res.data : []);
    } catch (err) {
      setRows([]);
      const msg = await parseApiErrorMessage(err, 'שגיאה בטעינת שורות העוקץ.');
      setError(msg);
    } finally {
      setLoading(false);
    }
  }, [employerId, academicYear, month]);

  useEffect(() => {
    loadRows();
  }, [loadRows]);

  useEffect(() => {
    const el = listModalRef.current;
    if (!el) return undefined;

    listBsModal.current = new BSModal(el, { backdrop: 'static', keyboard: true });
    const handleHidden = () => onClose?.();
    el.addEventListener('hidden.bs.modal', handleHidden);
    listBsModal.current.show();

    return () => {
      el.removeEventListener('hidden.bs.modal', handleHidden);
      listBsModal.current?.dispose();
      listBsModal.current = null;
    };
  }, [onClose]);

  useEffect(() => {
    const el = editModalRef.current;
    if (!el || !editingRow) return undefined;

    editBsModal.current = new BSModal(el, { backdrop: 'static', keyboard: true });
    editBsModal.current.show();

    return () => {
      editBsModal.current?.hide();
      editBsModal.current?.dispose();
      editBsModal.current = null;
    };
  }, [editingRow]);

  const openEdit = (row) => {
    setEditingRow(row);
    setEditForm(rowToForm(row));
    setAlert(null);
  };

  const closeEdit = () => {
    editBsModal.current?.hide();
    setEditingRow(null);
    setEditForm(null);
  };

  const handleEditField = (field) => (e) => {
    setEditForm((prev) => (prev ? { ...prev, [field]: e.target.value } : prev));
  };

  const handleSaveEdit = async (e) => {
    e.preventDefault();
    if (!editingRow || !editForm) return;

    setSaving(true);
    try {
      await payrollMonthlyInputsApi.updateRow(editingRow.id, formToPayload(editForm));
      closeEdit();
      showAlert('success', 'השורה נשמרה בהצלחה.');
      await loadRows();
      onSaved?.();
    } catch (err) {
      const msg = await parseApiErrorMessage(err, 'שגיאה בשמירת השורה.');
      showAlert('danger', msg);
    } finally {
      setSaving(false);
    }
  };

  const handleDismissList = () => {
    listBsModal.current?.hide();
  };

  return (
    <>
      <div
        className="modal fade"
        ref={listModalRef}
        tabIndex={-1}
        aria-labelledby="payrollRowsEditorTitle"
        aria-hidden="true"
      >
        <div className="modal-dialog modal-xl modal-dialog-scrollable">
          <div className="modal-content">
            <div className="modal-header">
              <h5 className="modal-title" id="payrollRowsEditorTitle">
                <i className="bi bi-table me-2"></i>
                שורות עוקץ — {monthLabel} {academicYear}
              </h5>
              <button
                type="button"
                className="btn-close"
                aria-label="סגור"
                onClick={handleDismissList}
              ></button>
            </div>
            <div className="modal-body">
              {alert && (
                <div className={`alert alert-${alert.type} alert-dismissible fade show`}>
                  <i
                    className={`bi bi-${
                      alert.type === 'success' ? 'check-circle' : 'exclamation-triangle'
                    } me-2`}
                  ></i>
                  {alert.msg}
                  <button
                    type="button"
                    className="btn-close"
                    onClick={() => setAlert(null)}
                    aria-label="סגור"
                  ></button>
                </div>
              )}

              {error && (
                <div className="alert alert-danger">
                  <i className="bi bi-exclamation-triangle me-2"></i>
                  {error}
                  <button
                    type="button"
                    className="btn btn-sm btn-outline-danger ms-3"
                    onClick={loadRows}
                  >
                    נסה שוב
                  </button>
                </div>
              )}

              {loading ? (
                <div className="text-center py-5 text-muted">
                  <span className="spinner-border spinner-border-sm me-2" role="status"></span>
                  טוען שורות…
                </div>
              ) : !error && rows.length === 0 ? (
                <p className="text-muted mb-0">אין שורות לחודש זה.</p>
              ) : !error ? (
                <div className="table-responsive">
                  <table className="table table-sm table-hover mb-0 align-middle">
                    <thead className="table-light">
                      <tr>
                        <th>סמל מוסד</th>
                        <th>מספר עובד בעוקץ</th>
                        <th>ת&quot;ז</th>
                        <th>שם פרטי+שם משפחה</th>
                        <th>תפקיד</th>
                        <th>דרגה</th>
                        <th>ותק</th>
                        <th>ש&quot;ש</th>
                        <th>בסיס משרה</th>
                        <th>אחוז משרה</th>
                        <th>שעות גיל</th>
                        <th>גמולי השתלמות</th>
                        <th>כפל תואר</th>
                        <th>קרן השתלמות</th>
                        <th>הכפלה כללית</th>
                        <th className="text-end">פעולות</th>
                      </tr>
                    </thead>
                    <tbody>
                      {rows.map((row) => (
                        <tr key={row.id}>
                          <td>{formatCell(row.institutionSymbol)}</td>
                          <td>{formatCell(row.oketzEmployeeNumber)}</td>
                          <td>{formatCell(row.idNumber)}</td>
                          <td>
                            {formatCell(row.fullName)}
                            {row.isManualEdited ? (
                              <span className="badge bg-info text-dark ms-1">נערך</span>
                            ) : null}
                          </td>
                          <td>{formatCell(row.role)}</td>
                          <td>{formatCell(row.grade)}</td>
                          <td>{formatCell(row.seniority)}</td>
                          <td>{formatCell(row.weeklyHours)}</td>
                          <td>{formatCell(row.jobBase)}</td>
                          <td>{formatCell(row.jobPercent)}</td>
                          <td>{formatCell(row.ageHours)}</td>
                          <td>{formatCell(row.trainingBenefits)}</td>
                          <td>{formatCell(row.doubleDegree)}</td>
                          <td>{formatCell(row.trainingFund)}</td>
                          <td>{formatCell(row.generalMultiplier)}</td>
                          <td className="text-end text-nowrap">
                            <button
                              type="button"
                              className="btn btn-outline-primary btn-sm"
                              onClick={() => openEdit(row)}
                            >
                              <i className="bi bi-pencil me-1"></i>
                              עריכה
                            </button>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              ) : null}
            </div>
            <div className="modal-footer">
              <button type="button" className="btn btn-secondary" onClick={handleDismissList}>
                סגור
              </button>
            </div>
          </div>
        </div>
      </div>

      {editingRow && editForm ? (
        <div
          className="modal fade"
          ref={editModalRef}
          tabIndex={-1}
          aria-labelledby="payrollRowEditTitle"
          aria-hidden="true"
        >
          <div className="modal-dialog modal-lg modal-dialog-scrollable">
            <div className="modal-content">
              <form onSubmit={handleSaveEdit}>
                <div className="modal-header">
                  <h5 className="modal-title" id="payrollRowEditTitle">
                    <i className="bi bi-pencil-square me-2"></i>
                    עריכת שורה
                  </h5>
                  <button
                    type="button"
                    className="btn-close"
                    aria-label="סגור"
                    onClick={closeEdit}
                    disabled={saving}
                  ></button>
                </div>
                <div className="modal-body">
                  <p className="text-muted small">
                    {formatCell(editingRow.fullName)}
                    {editingRow.idNumber ? ` · ${editingRow.idNumber}` : ''}
                  </p>
                  <div className="row g-2">
                    <div className="col-md-6">
                      <label className="form-label small fw-semibold">סמל מוסד</label>
                      <input
                        type="text"
                        className="form-control form-control-sm"
                        value={editForm.institutionSymbol}
                        onChange={handleEditField('institutionSymbol')}
                      />
                    </div>
                    <div className="col-md-6">
                      <label className="form-label small fw-semibold">מספר עובד בעוקץ</label>
                      <input
                        type="text"
                        className="form-control form-control-sm"
                        value={editForm.oketzEmployeeNumber}
                        onChange={handleEditField('oketzEmployeeNumber')}
                      />
                    </div>
                    <div className="col-md-6">
                      <label className="form-label small fw-semibold">ת&quot;ז</label>
                      <input
                        type="text"
                        className="form-control form-control-sm"
                        value={editForm.idNumber}
                        onChange={handleEditField('idNumber')}
                      />
                    </div>
                    <div className="col-md-6">
                      <label className="form-label small fw-semibold">שם מלא</label>
                      <input
                        type="text"
                        className="form-control form-control-sm"
                        value={editForm.fullName}
                        onChange={handleEditField('fullName')}
                      />
                    </div>
                    <div className="col-md-6">
                      <label className="form-label small fw-semibold">תפקיד</label>
                      <input
                        type="text"
                        className="form-control form-control-sm"
                        value={editForm.role}
                        onChange={handleEditField('role')}
                      />
                    </div>
                    <div className="col-md-6">
                      <label className="form-label small fw-semibold">דרגה</label>
                      <input
                        type="text"
                        className="form-control form-control-sm"
                        value={editForm.grade}
                        onChange={handleEditField('grade')}
                      />
                    </div>
                    <div className="col-md-4">
                      <label className="form-label small fw-semibold">ותק</label>
                      <input
                        type="text"
                        className="form-control form-control-sm"
                        value={editForm.seniority}
                        onChange={handleEditField('seniority')}
                      />
                    </div>
                    <div className="col-md-4">
                      <label className="form-label small fw-semibold">ש&quot;ש</label>
                      <input
                        type="text"
                        className="form-control form-control-sm"
                        value={editForm.weeklyHours}
                        onChange={handleEditField('weeklyHours')}
                      />
                    </div>
                    <div className="col-md-4">
                      <label className="form-label small fw-semibold">בסיס משרה</label>
                      <input
                        type="text"
                        className="form-control form-control-sm"
                        value={editForm.jobBase}
                        onChange={handleEditField('jobBase')}
                      />
                    </div>
                    <div className="col-md-4">
                      <label className="form-label small fw-semibold">אחוז משרה</label>
                      <input
                        type="text"
                        className="form-control form-control-sm"
                        value={editForm.jobPercent}
                        onChange={handleEditField('jobPercent')}
                      />
                    </div>
                    <div className="col-md-4">
                      <label className="form-label small fw-semibold">שעות גיל</label>
                      <input
                        type="text"
                        className="form-control form-control-sm"
                        value={editForm.ageHours}
                        onChange={handleEditField('ageHours')}
                      />
                    </div>
                    <div className="col-md-4">
                      <label className="form-label small fw-semibold">גמולי השתלמות</label>
                      <input
                        type="text"
                        className="form-control form-control-sm"
                        value={editForm.trainingBenefits}
                        onChange={handleEditField('trainingBenefits')}
                      />
                    </div>
                    <div className="col-md-4">
                      <label className="form-label small fw-semibold">כפל תואר</label>
                      <input
                        type="text"
                        className="form-control form-control-sm"
                        value={editForm.doubleDegree}
                        onChange={handleEditField('doubleDegree')}
                      />
                    </div>
                    <div className="col-md-4">
                      <label className="form-label small fw-semibold">קרן השתלמות</label>
                      <input
                        type="text"
                        className="form-control form-control-sm"
                        value={editForm.trainingFund}
                        onChange={handleEditField('trainingFund')}
                      />
                    </div>
                    <div className="col-md-4">
                      <label className="form-label small fw-semibold">הכפלה כללית</label>
                      <input
                        type="text"
                        className="form-control form-control-sm"
                        value={editForm.generalMultiplier}
                        onChange={handleEditField('generalMultiplier')}
                      />
                    </div>
                    <div className="col-12">
                      <label className="form-label small fw-semibold">הערת עריכה (אופציונלי)</label>
                      <input
                        type="text"
                        className="form-control form-control-sm"
                        value={editForm.manualEditNote}
                        onChange={handleEditField('manualEditNote')}
                        maxLength={500}
                      />
                    </div>
                  </div>
                </div>
                <div className="modal-footer">
                  <button
                    type="button"
                    className="btn btn-secondary"
                    onClick={closeEdit}
                    disabled={saving}
                  >
                    ביטול
                  </button>
                  <button type="submit" className="btn btn-primary" disabled={saving}>
                    {saving ? (
                      <>
                        <span className="spinner-border spinner-border-sm me-2" role="status"></span>
                        שומר…
                      </>
                    ) : (
                      <>
                        <i className="bi bi-check-lg me-1"></i>
                        שמור
                      </>
                    )}
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
      ) : null}
    </>
  );
}
