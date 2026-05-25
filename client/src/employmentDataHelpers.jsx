export const GRADE_OPTIONS = {
  'יסודי וגנים': ['ב', 'בכיר', 'גננת מוסמכת', 'ד"ר', 'מ.א.', 'מורה מוסמך'],
  'אחיד': ['תומכת חינוך', 'תומכת חינוך חנ"מ'],
  'עוז לתמורה': ['ב', 'בכיר', 'גננת מוסמכת', 'ד"ר', 'מ.א.', 'מורה מוסמך'],
  'אופק חדש': ['1', '1.5', '2', '2.5', '3', '3.5', '4', '4.5', '5', '5.5', '6', '6.5', '7', '7.5', '8', '8.5', '9'],
  'אופק גנים': ['1', '1.5', '2', '2.5', '3', '3.5', '4', '4.5', '5', '5.5', '6', '6.5', '7', '7.5', '8', '8.5', '9'],
};

export const ROLE_OPTIONS = {
  'יסודי וגנים': ['גננת ראשית', 'גננת משלימה', 'גננת שילוב', 'מורה מחנך', 'מורה מקצועי', 'מנהל'],
  'אחיד': ['סייעת ראשית', 'סייעת משלימה', 'סייעת שניה'],
  'עוז לתמורה': ['מורה מחנך', 'מורה מקצועי', 'מנהל'],
  'אופק חדש': ['גננת ראשית', 'גננת משלימה', 'גננת שילוב', 'מורה מחנך', 'מורה מקצועי', 'מנהל', 'פרא רפואי'],
  'אופק גנים': ['גננת ראשית', 'גננת עמיתה', 'פרא רפואי'],
};

export const GRADE_NAMES = Object.keys(GRADE_OPTIONS);

/** אחוז תוספת אם לפי שם דירוג (לאחר מתקיימות ההתניות) */
export const MOTHER_BENEFIT_RATE_BY_GRADE_NAME = {
  'יסודי וגנים': 10,
  אחיד: 7,
  'עוז לתמורה': 10,
  'אופק חדש': 10,
  'אופק גנים': 10,
};

/** הסף למשרה בסיסית (ללא תוספת אם) — "מעלי 79%" */
export const EMPLOYMENT_MOTHER_BENEFIT_BASE_JOB_PERCENT_THRESHOLD = 79;

/** בסיס משרה ברירת מחדל לפי שם דירוג (מוסד) — תואם לטבלת התניות */
export const DEFAULT_JOB_BASE_BY_GRADE_NAME = {
  'יסודי וגנים': 30,
  אחיד: 40,
  'עוז לתמורה': 38,
  'אופק חדש': 36,
  'אופק גנים': 36,
};

/** ערכי בסיס משרה לפי תפקיד (מחליפים ברירת מחדל לפי מוסד). פרא רפואי תלוי מוסד — ראו getJobBaseValue */
export const JOB_BASE_BY_ROLE = {
  'גננת ראשית': 30,
  'גננת עמיתה': 33.8,
};

/**
 * ערך בסיס משרה: מתמלא רק אחרי בחירת תפקיד — קודם התנות תפקיד (כולל פרא רפואי לפי מוסד), אחרת לפי מוסד.
 * @param {string} gradeName
 * @param {string} [role]
 * @returns {string} — מספר כמחרוזת לשדה טופס, או '' בלי שם דירוג / בלי תפקיד
 */
export function getJobBaseValue(gradeName, role) {
  const gn = String(gradeName ?? '').trim();
  const r = String(role ?? '').trim();
  if (!gn || !r) return '';
  if (r === 'פרא רפואי' && gn === 'אופק גנים') {
    return '33.8';
  }
  if (Object.prototype.hasOwnProperty.call(JOB_BASE_BY_ROLE, r)) {
    return String(JOB_BASE_BY_ROLE[r]);
  }
  if (Object.prototype.hasOwnProperty.call(DEFAULT_JOB_BASE_BY_GRADE_NAME, gn)) {
    return String(DEFAULT_JOB_BASE_BY_GRADE_NAME[gn]);
  }
  return '';
}

/**
 * בסיס משרה נטו: בסיס גולמי לפי התניות פחות שעות גיל (מספר באותה דרגה).
 */
export function netJobBaseAfterAgeHours(nominalStr, ageHoursRaw) {
  const grossStr = String(nominalStr ?? '').trim();
  if (!grossStr) return '';
  const gross = N(grossStr);
  if (gross == null || Number.isNaN(gross)) return '';
  const ah = N(ageHoursRaw);
  const deduct = ah == null || Number.isNaN(ah) ? 0 : ah;
  const net = Math.max(0, gross - deduct);
  return String(Math.round(net * 100) / 100);
}

/**
 * מעדכן את jobBase בכל 6 מקטעי הדרגה (band 1|2) לפי שם דירוג, תפקיד ושעות גיל של אותה דרגה.
 */
export function withBandSlotsJobBase(slots, band, gradeName, role, ageHoursRaw) {
  if (!Array.isArray(slots)) return slots;
  const start = (band - 1) * 6;
  const nominal = getJobBaseValue(gradeName, role);
  const jbv = netJobBaseAfterAgeHours(nominal, ageHoursRaw);
  return slots.map((s, i) => (i >= start && i < start + 6 ? { ...s, jobBase: jbv } : s));
}

/** מחיל בסיס משרה (נטו) על כל המקטעים לפי דירוג/תפקיד/שעות גיל, ומסנכרן שורות תוספת למחנך */
export function patchEmploymentSlotJobBases(form) {
  let slots = form.slots || initSlots();
  slots = withBandSlotsJobBase(slots, 1, form.grade1GradeName, form.grade1Role, form.grade1AgeHours);
  slots = withBandSlotsJobBase(slots, 2, form.grade2GradeName, form.grade2Role, form.grade2AgeHours);
  slots = syncTeacherSupplementarySlots(slots, form);
  return { ...form, slots };
}

const HEBREW_YEAR_PARTS = [
  [400, 'ת'],
  [300, 'ש'],
  [200, 'ר'],
  [100, 'ק'],
  [90, 'צ'],
  [80, 'פ'],
  [70, 'ע'],
  [60, 'ס'],
  [50, 'נ'],
  [40, 'מ'],
  [30, 'ל'],
  [20, 'כ'],
  [10, 'י'],
  [9, 'ט'],
  [8, 'ח'],
  [7, 'ז'],
  [6, 'ו'],
  [5, 'ה'],
  [4, 'ד'],
  [3, 'ג'],
  [2, 'ב'],
  [1, 'א'],
];

export function formatHebrewYear(year) {
  let rest = year % 1000;
  const letters = [];
  for (const [value, letter] of HEBREW_YEAR_PARTS) {
    while (rest >= value) {
      if (rest === 15) {
        letters.push('ט', 'ו');
        rest = 0;
        break;
      }
      if (rest === 16) {
        letters.push('ט', 'ז');
        rest = 0;
        break;
      }
      letters.push(letter);
      rest -= value;
    }
  }
  if (letters.length === 1) return `${letters[0]}'`;
  return `${letters.slice(0, -1).join('')}"${letters.at(-1)}`;
}

export function currentHebrewAcademicYear() {
  const now = new Date();
  const gregorianYear = now.getFullYear();
  const hebrewYear = now.getMonth() >= 8 ? gregorianYear + 3761 : gregorianYear + 3760;
  return formatHebrewYear(hebrewYear);
}

/** מפתחות שדות דירוג לפי רמת דרגה (1 / 2) */
export function bandFieldKeys(band) {
  const p = band === 1 ? 'grade1' : 'grade2';
  return {
    gradeName: `${p}GradeName`,
    grade: `${p}Grade`,
    role: `${p}Role`,
    seniority: `${p}Seniority`,
  };
}

export const defaultBandFields = () => ({
  grade1GradeName: '',
  grade1Grade: '',
  grade1Role: '',
  grade1Seniority: '',
  grade2GradeName: '',
  grade2Grade: '',
  grade2Role: '',
  grade2Seniority: '',
});

/** שילובים שבהם מופיעה שורת "3 שעות נוספות למחנך/גננת" במקטע הבא */
export function qualifiesTeacherSupplementaryHoursRow(gradeName, role) {
  const gn = String(gradeName ?? '').trim();
  const r = String(role ?? '').trim();
  return (
    (gn === 'יסודי וגנים' && r === 'גננת ראשית') ||
    (gn === 'עוז לתמורה' && r === 'מורה מחנך')
  );
}

/** מספר המקטע ההורה (1–5) אם שורת המקטע הנוכחית מייצגת שעות נוספות למחנך; null אחרת — תואם למסד */
export const TEACHER_EXTRA_HOURS_WEEKLY = '3';

function isSupplementaryEmploymentSlot(row) {
  return row.supplementaryParentSlotIndex != null && row.supplementaryParentSlotIndex !== '';
}

function slotParentHasSymbolAndHours(row) {
  const sym = String(row.institutionSymbol ?? '').trim();
  if (!sym) return false;
  const w = row.weeklyHours;
  if (w === '' || w == null) return false;
  const n = parseFloat(String(w));
  return !Number.isNaN(n);
}

/** מקטע ריק (למחיקת שורת נוספות) */
export function emptyEmploymentSlot(gradeBand, slotIndex) {
  return {
    gradeBand,
    slotIndex,
    institutionSymbol: '',
    weeklyHours: '',
    jobBase: '',
    supplementaryParentSlotIndex: null,
  };
}

/**
 * לאחר שינוי סמל מוסד/שעות או דירוג — מסנכרן שורות תוספת (3 ש"ש) למחנך/גננת במקטע הבא.
 */
export function syncTeacherSupplementarySlots(slots, form) {
  if (!Array.isArray(slots)) return slots;
  const next = slots.map((row) => ({ ...row }));
  for (let band = 1; band <= 2; band++) {
    const keys = bandFieldKeys(band);
    const ok = qualifiesTeacherSupplementaryHoursRow(form[keys.gradeName], form[keys.role]);
    const base = (band - 1) * 6;
    if (!ok) {
      for (let seg = 1; seg <= 6; seg++) {
        const i = base + seg - 1;
        if (next[i].supplementaryParentSlotIndex != null) {
          next[i] = emptyEmploymentSlot(band, seg);
        }
      }
      continue;
    }
    for (let seg = 1; seg <= 5; seg++) {
      const pIdx = base + seg - 1;
      const cIdx = pIdx + 1;
      const parent = next[pIdx];
      if (
        !slotParentHasSymbolAndHours(parent) ||
        isSupplementaryEmploymentSlot(parent)
      ) {
        const chi = next[cIdx];
        if (
          chi.supplementaryParentSlotIndex != null &&
          Number(chi.supplementaryParentSlotIndex) === seg
        ) {
          next[cIdx] = emptyEmploymentSlot(band, seg + 1);
        }
        continue;
      }
      const jb =
        parent.jobBase !== '' && parent.jobBase != null ? String(parent.jobBase) : '';
      next[cIdx] = {
        ...next[cIdx],
        gradeBand: band,
        slotIndex: seg + 1,
        institutionSymbol: String(parent.institutionSymbol ?? '').trim(),
        weeklyHours: TEACHER_EXTRA_HOURS_WEEKLY,
        jobBase: jb,
        supplementaryParentSlotIndex: seg,
      };
    }
  }
  return next;
}

/**
 * עדכון מקטע יחיד + סנכרון שורות נוספות (אחרי שינוי סמל/שעות)
 */
export function mergeSlotAndSyncTeacherExtras(prevForm, slotIndex, field, value) {
  const merged = prevForm.slots.map((row, i) =>
    i === slotIndex ? { ...row, [field]: value } : row
  );
  const withSlots = {
    ...prevForm,
    slots: syncTeacherSupplementarySlots(merged, prevForm),
  };
  return patchEmploymentTotalsThenJobPercents(withSlots);
}

/** מקטעים: דרגה 1/2 × 6 — תואם EmploymentDataSlotDto (ללא שדות דירוג; הם ברמת נתוני העסקה) */
export function initSlots() {
  const out = [];
  for (let b = 1; b <= 2; b++) {
    for (let i = 1; i <= 6; i++) {
      out.push(emptyEmploymentSlot(b, i));
    }
  }
  return out;
}

export const N = (v) => (v === '' || v == null) ? null : parseFloat(v);

function parseBirthDateForAge(raw) {
  const s = String(raw ?? '').trim();
  if (!s) return null;
  const d = new Date(s.includes('T') ? s : `${s}T12:00:00`);
  return Number.isNaN(d.getTime()) ? null : d;
}

/**
 * גיל מלא בתאריך ייחוס (למשל למדיניות עד גיל 14 כולל).
 */
export function ageInFullYearsAtDate(birthRaw, refDate = new Date()) {
  const birth = parseBirthDateForAge(birthRaw);
  if (!birth) return null;
  const ref = refDate instanceof Date ? refDate : new Date(refDate);
  if (Number.isNaN(ref.getTime())) return null;
  let age = ref.getFullYear() - birth.getFullYear();
  const md = ref.getMonth() - birth.getMonth();
  if (md < 0 || (md === 0 && ref.getDate() < birth.getDate())) age--;
  return age;
}

/**
 * יש לפחות תאריך לידה אחד שמוכיח ילד בגיל עד המקסימום (ברירת מחדל 14 כולל).
 */
export function employmentFormHasChildUpToAgeInclusive(form, maxAgeInclusive = 14, refDate = new Date()) {
  if (!form) return false;
  for (let i = 1; i <= 10; i++) {
    const raw = form[`childBirthDate${i}`];
    const age = ageInFullYearsAtDate(raw, refDate);
    if (age != null && age <= maxAgeInclusive) return true;
  }
  return false;
}

/**
 * תאריכי לידת ילדים 1–10 מרשומת עובד (לחישוב תוספת אם בעריכת נתוני העסקה).
 */
export function childBirthDateFieldsFromEmployee(emp) {
  if (!emp) return {};
  const o = {
    employeeBirthDate: emp.birthDate ?? '',
    employeeGender: emp.gender ?? '',
  };
  for (let i = 1; i <= 10; i++) {
    const k = `childBirthDate${i}`;
    o[k] = emp[k] ?? '';
  }
  return o;
}

/**
 * שעות גיל לפי התניות: מתחת ל־50 → 0; מ־50 עד מתחת ל־55 → 2; מ־55 והלאה → 4.
 * משתמש ב־birthDate של טופס הוספת עובד או ב־employeeBirthDate מפרטי העובד בעריכת נתוני העסקה.
 */
export function computeEmployeeAgeHoursString(form, refDate = new Date()) {
  const raw = form?.birthDate ?? form?.employeeBirthDate;
  const age = ageInFullYearsAtDate(raw, refDate);
  if (age == null) return '';
  if (age < 50) return '0';
  if (age < 55) return '2';
  return '4';
}

export function patchEmploymentAutoAgeHours(form) {
  const h = computeEmployeeAgeHoursString(form);
  return {
    ...form,
    grade1AgeHours: h,
    grade2AgeHours: h,
  };
}

/**
 * בסיס משוקלל (הרמוני) מהמקטעים: Σ ש"ש / Σ(ש"ש/בסיס) — מתאים לכמה בסיסים שונים; במקטע יחיד = אותו בסיס.
 */
export function effectiveHarmonicJobBaseFromSlots(slotRows) {
  if (!slotRows?.length) return null;
  let sumW = 0;
  let sumWOverB = 0;
  for (const row of slotRows) {
    const w = N(row.weeklyHours);
    const b = N(row.jobBase);
    if (w == null || w <= 0 || b == null || b <= 0) continue;
    sumW += w;
    sumWOverB += w / b;
  }
  if (sumW <= 0 || sumWOverB <= 0) return null;
  return sumW / sumWOverB;
}

/**
 * אחוז משרה בסיסי (ללא תוספת אם) — לשימוש בהתניות תוספת אם (סף 79%) בלי מעגל תלות.
 */
export function computeGradeBaseJobPercentNumber(form, band) {
  const totalKey = band === 1 ? 'grade1Total' : 'grade2Total';
  const slotsSlice =
    band === 1 ? (form.slots || []).slice(0, 6) : (form.slots || []).slice(6, 12);
  const equiv = effectiveHarmonicJobBaseFromSlots(slotsSlice);
  const total = N(form[totalKey]);
  if (equiv == null || equiv <= 0) return null;
  if (total == null || Number.isNaN(total)) return null;
  const raw = (total / equiv) * 100;
  if (Number.isNaN(raw)) return null;
  return Math.round(raw * 100) / 100;
}

/**
 * תוספת אם אוטומטית: רק נקבה + דירוג ממפתחות + אחוז משרה בסיסי מעלי 79% + ילד עד גיל 14 כולל; אחרת 0.
 */
export function computeMotherBenefitPercentString(form, band) {
  const keys = bandFieldKeys(band);
  const gn = String(form[keys.gradeName] ?? '').trim();
  if (!gn) return '';
  if (!Object.prototype.hasOwnProperty.call(MOTHER_BENEFIT_RATE_BY_GRADE_NAME, gn)) return '';
  const gender = String(form?.gender ?? form?.employeeGender ?? '').trim();
  if (!(gender === 'נקבה' || gender.toLowerCase() === 'female')) return '0';

  const basePct = computeGradeBaseJobPercentNumber(form, band);
  if (basePct == null) return '';

  if (basePct <= EMPLOYMENT_MOTHER_BENEFIT_BASE_JOB_PERCENT_THRESHOLD) return '0';

  if (!employmentFormHasChildUpToAgeInclusive(form)) return '0';

  return String(MOTHER_BENEFIT_RATE_BY_GRADE_NAME[gn]);
}

export function patchEmploymentAutoMotherBenefit(form) {
  return {
    ...form,
    grade1MotherBenefitPercent: computeMotherBenefitPercentString(form, 1),
    grade2MotherBenefitPercent: computeMotherBenefitPercentString(form, 2),
  };
}

/**
 * אחוז משרה: (סה"כ ש"ש ÷ בסיס משרה) × 100 + אחוז תוספת אם (אם אין — 0)
 */
export function computeGradeJobPercentString(form, band) {
  const totalKey = band === 1 ? 'grade1Total' : 'grade2Total';
  const motherKey = band === 1 ? 'grade1MotherBenefitPercent' : 'grade2MotherBenefitPercent';
  const slotsSlice =
    band === 1 ? (form.slots || []).slice(0, 6) : (form.slots || []).slice(6, 12);
  const equiv = effectiveHarmonicJobBaseFromSlots(slotsSlice);
  const total = N(form[totalKey]);
  const momRaw = N(form[motherKey]);
  const mom = momRaw == null || Number.isNaN(momRaw) ? 0 : momRaw;
  if (equiv == null || equiv <= 0) return '';
  if (total == null || Number.isNaN(total)) return '';
  const raw = (total / equiv) * 100 + mom;
  if (Number.isNaN(raw)) return '';
  return String(Math.round(raw * 100) / 100);
}

export function patchEmploymentAutoJobPercents(form) {
  return {
    ...form,
    grade1JobPercent: computeGradeJobPercentString(form, 1),
    grade2JobPercent: computeGradeJobPercentString(form, 2),
  };
}

/**
 * סכום שעות שבועיות מתוך 6 המקטעים — כבסיס לשדה "סה\"כ ש\"ש".
 * @returns {number|null} null אם לא הוזנה אף שעה במקטע
 */
export function sumWeeklyHoursSlotRows(slotRows) {
  if (!slotRows?.length) return null;
  let sum = 0;
  let hasAny = false;
  for (const row of slotRows) {
    if (row.weeklyHours === '' || row.weeklyHours == null) continue;
    const w = N(row.weeklyHours);
    if (w != null && !Number.isNaN(w)) {
      hasAny = true;
      sum += w;
    }
  }
  if (!hasAny) return null;
  return Math.round(sum * 100) / 100;
}

export function patchEmploymentAutoTotals(form) {
  const slots = form.slots || [];
  const t1 = sumWeeklyHoursSlotRows(slots.slice(0, 6));
  const t2 = sumWeeklyHoursSlotRows(slots.slice(6, 12));
  return {
    ...form,
    grade1Total: t1 === null ? '' : String(t1),
    grade2Total: t2 === null ? '' : String(t2),
  };
}

/** סף "שליש משרה" להתניות קרן השתלמות (= 100/3%) */
export const EMPLOYMENT_ONE_THIRD_JOB_PERCENT = 100 / 3;

/** פרסור ותק (מספר שנים/ערך מספרי בשדה ותק) לצורך התניות אחיד */
export function parseSeniorityYears(raw) {
  const s = String(raw ?? '').trim();
  if (s === '') return null;
  const n = parseFloat(s.replace(',', '.'));
  return Number.isNaN(n) ? null : n;
}

/**
 * אחוז קרן השתלמות לפי שם דירוג ואחוז משרה:
 * פחות משליש משרה → 0; יסודי / עוז לתמורה / אופקים — מעל שליש → 8.4%; באחיד מעל שליש — ותק>2 ⇒ 7.5%, אחרת 0.
 */
export function computeTrainingFundPercentString(form, band) {
  const keys = bandFieldKeys(band);
  const gn = String(form[keys.gradeName] ?? '').trim();
  const jobKey = band === 1 ? 'grade1JobPercent' : 'grade2JobPercent';
  const jobPct = N(form[jobKey]);
  if (!gn || jobPct == null || Number.isNaN(jobPct)) return '';
  if (jobPct < EMPLOYMENT_ONE_THIRD_JOB_PERCENT - 1e-9) return '0';

  if (gn === 'אחיד') {
    const vetek = parseSeniorityYears(form[keys.seniority]);
    if (vetek != null && vetek > 2) return '7.5';
    return '0';
  }

  if (
    gn === 'יסודי וגנים' ||
    gn === 'עוז לתמורה' ||
    gn === 'אופק חדש' ||
    gn === 'אופק גנים'
  ) {
    return '8.4';
  }

  return '';
}

export function patchEmploymentAutoTrainingFund(form) {
  return {
    ...form,
    grade1TrainingFundPercent: computeTrainingFundPercentString(form, 1),
    grade2TrainingFundPercent: computeTrainingFundPercentString(form, 2),
  };
}

/** סה"כ ממקטעים → שעות גיל (לפי גיל העובד) → בסיס משרה במקטעים (נטו) → תוספת אם → אחוז משרה → קרן השתלמות */
export function patchEmploymentTotalsThenJobPercents(form) {
  const withTotals = patchEmploymentAutoTotals(form);
  const withAgeHours = patchEmploymentAutoAgeHours(withTotals);
  const withSlotBases = patchEmploymentSlotJobBases(withAgeHours);
  const withMother = patchEmploymentAutoMotherBenefit(withSlotBases);
  const withJob = patchEmploymentAutoJobPercents(withMother);
  return patchEmploymentAutoTrainingFund(withJob);
}

/** תואם EmploymentSlotPersistence בשרת — רק מקטעים עם תוכן נשלחים ל-API */
export function shouldPersistEmploymentSlot(row) {
  const sup = row.supplementaryParentSlotIndex;
  if (sup != null && sup !== '') {
    const n = Number(sup);
    if (n >= 1 && n <= 5) return true;
  }
  if (String(row.institutionSymbol ?? '').trim()) return true;
  const w = row.weeklyHours;
  if (w !== '' && w != null) {
    const h = parseFloat(String(w));
    if (!Number.isNaN(h) && h > 0) return true;
  }
  return false;
}

export function mapSlotsToDto(slots) {
  return slots.filter(shouldPersistEmploymentSlot).map((s) => ({
    gradeBand: s.gradeBand,
    slotIndex: s.slotIndex,
    institutionSymbol: s.institutionSymbol?.trim() || null,
    weeklyHours: N(s.weeklyHours),
    jobBase: N(s.jobBase),
    supplementaryParentSlotIndex:
      s.supplementaryParentSlotIndex == null || s.supplementaryParentSlotIndex === ''
        ? null
        : Number(s.supplementaryParentSlotIndex),
  }));
}

/** ממלא מקטעים מרשומת API (אחרי טעינה) */
export function apiRecordToFormSlots(rec) {
  if (!rec?.slots?.length) return initSlots();
  const byKey = new Map(rec.slots.map((s) => [`${s.gradeBand}-${s.slotIndex}`, s]));
  return initSlots().map((s) => {
    const a = byKey.get(`${s.gradeBand}-${s.slotIndex}`);
    if (!a) return s;
    return {
      ...s,
      institutionSymbol: a.institutionSymbol ?? '',
      weeklyHours: a.weeklyHours ?? '',
      jobBase: a.jobBase ?? '',
      supplementaryParentSlotIndex:
        a.supplementaryParentSlotIndex != null && a.supplementaryParentSlotIndex !== ''
          ? Number(a.supplementaryParentSlotIndex)
          : null,
    };
  });
}

export function apiRecordBandFields(rec) {
  return {
    grade1GradeName: rec.grade1GradeName ?? '',
    grade1Grade: rec.grade1Grade ?? '',
    grade1Role: rec.grade1Role ?? '',
    grade1Seniority: rec.grade1Seniority ?? '',
    grade2GradeName: rec.grade2GradeName ?? '',
    grade2Grade: rec.grade2Grade ?? '',
    grade2Role: rec.grade2Role ?? '',
    grade2Seniority: rec.grade2Seniority ?? '',
  };
}

const summaryManualG1 = [
  { f: 'grade1TrainingBenefits', label: 'גמולי השתלמות' },
  { f: 'grade1DoubleDegree', label: 'כפל תואר' },
];
const summaryAutoG1 = [
  { f: 'grade1AgeHours', label: 'שעות גיל' },
  { f: 'grade1Total', label: 'סה"כ ש"ש' },
  { f: 'grade1JobPercent', label: 'אחוז משרה' },
  { f: 'grade1TrainingFundPercent', label: 'קרן השתלמות %' },
  { f: 'grade1MotherBenefitPercent', label: 'אחוז תוספת אם' },
];
const summaryManualG2 = [
  { f: 'grade2TrainingBenefits', label: 'גמולי השתלמות' },
  { f: 'grade2DoubleDegree', label: 'כפל תואר' },
];
const summaryAutoG2 = [
  { f: 'grade2AgeHours', label: 'שעות גיל' },
  { f: 'grade2Total', label: 'סה"כ ש"ש' },
  { f: 'grade2JobPercent', label: 'אחוז משרה' },
  { f: 'grade2TrainingFundPercent', label: 'קרן השתלמות %' },
  { f: 'grade2MotherBenefitPercent', label: 'אחוז תוספת אם' },
];

const ACADEMIC_YEAR_OPTIONS = (() => {
  const o = [];
  const now = new Date();
  const gregorianYear = now.getFullYear();
  const current = now.getMonth() >= 8 ? gregorianYear + 3761 : gregorianYear + 3760;
  for (let y = current + 5; y >= current - 20; y--) o.push(formatHebrewYear(y));
  return o;
})();

/**
 * True when the user entered grade names or at least one slot with institution symbol and weekly hours.
 */
export function employmentSectionHasStructuredContent(form) {
  if (!form) return false;
  if (String(form.grade1GradeName ?? '').trim() || String(form.grade2GradeName ?? '').trim()) {
    return true;
  }
  const slots = form.slots || [];
  for (const row of slots) {
    const sym = String(row.institutionSymbol ?? '').trim();
    if (!sym) continue;
    const w = row.weeklyHours;
    if (w === '' || w == null) continue;
    const n = parseFloat(String(w));
    if (!Number.isNaN(n)) return true;
  }
  return false;
}

/**
 * Whether to POST employment-data after creating the employee (add-employee page).
 */
export function shouldCreateEmploymentDataWithAddEmployeeForm(form) {
  const year = String(form?.academicYear ?? '').trim();
  if (!year) return false;
  return employmentSectionHasStructuredContent(form);
}

/**
 * Cross-check academic year vs structured employment fields on the add-employee page.
 * @returns {string|null} Hebrew error message, or null if OK.
 */
export function validateAddEmployeeEmploymentSection(form) {
  const year = String(form?.academicYear ?? '').trim();
  const hasStruct = employmentSectionHasStructuredContent(form);
  if (!year && hasStruct) {
    return 'לבחירת נתוני העסקה יש לבחור שנת לימודים, או למחוק את השדות שמילאתם באזור נתוני ההעסקה.';
  }
  if (year && !hasStruct) {
    return 'נבחרה שנת לימודים — יש למלא לפחות שם דירוג או מקטע עם סמל מוסד ושעות, או לבחור במצב ללא שנת לימודים כדי לשמור עובד בלבד.';
  }
  return null;
}

export function buildEmploymentPayloadFromForm(employeeId, employerId, form) {
  const patched = patchEmploymentTotalsThenJobPercents(form);
  return {
    employeeId,
    employerId,
    academicYear: String(patched.academicYear ?? '').trim(),
    grade1Total: N(patched.grade1Total),
    grade1JobPercent: N(patched.grade1JobPercent),
    grade1TrainingFundPercent: N(patched.grade1TrainingFundPercent),
    grade1AgeHours: N(patched.grade1AgeHours),
    grade1MotherBenefitPercent: N(patched.grade1MotherBenefitPercent),
    grade1TrainingBenefits: N(patched.grade1TrainingBenefits),
    grade1DoubleDegree: N(patched.grade1DoubleDegree),
    grade2Total: N(patched.grade2Total),
    grade2JobPercent: N(patched.grade2JobPercent),
    grade2TrainingFundPercent: N(patched.grade2TrainingFundPercent),
    grade2AgeHours: N(patched.grade2AgeHours),
    grade2MotherBenefitPercent: N(patched.grade2MotherBenefitPercent),
    grade2TrainingBenefits: N(patched.grade2TrainingBenefits),
    grade2DoubleDegree: N(patched.grade2DoubleDegree),
    grade1GradeName: patched.grade1GradeName?.trim() || null,
    grade1Grade: patched.grade1Grade?.trim() || null,
    grade1Role: patched.grade1Role?.trim() || null,
    grade1Seniority: patched.grade1Seniority?.trim() || null,
    grade2GradeName: patched.grade2GradeName?.trim() || null,
    grade2Grade: patched.grade2Grade?.trim() || null,
    grade2Role: patched.grade2Role?.trim() || null,
    grade2Seniority: patched.grade2Seniority?.trim() || null,
    slots: mapSlotsToDto(patched.slots || initSlots()),
  };
}

/**
 * @param {object} p
 * @param {object} p.form
 * @param {function} p.set — setState helper (field) => (e) => void
 * @param {function} p.setSlot — (idx, field) => (e) => void
 * @param {function} p.setBandField — (band, field) => (e) => void  field: 'gradeName'|'grade'|'role'|'seniority'
 */
export function EmploymentDataFormSections({
  form,
  set,
  setSlot,
  setBandField,
  institutionSymbols = [],
  academicYearOptional = false,
}) {
  const yVal = form.academicYear === '' || form.academicYear == null ? '' : String(form.academicYear);
  const yearOpts = new Set(
    [...ACADEMIC_YEAR_OPTIONS, ...(yVal ? [form.academicYear] : [])].filter(
      (x) => x !== '' && x != null,
    ),
  );
  const selYear = (
    <select className="form-select form-select-sm" value={yVal} onChange={set('academicYear')}>
      {academicYearOptional ? (
        <option value="">
          — ללא (עובד בלבד; נתוני העסקה אחר כך) —
        </option>
      ) : null}
      {Array.from(yearOpts).map((y) => (
        <option key={y} value={y}>
          {y}
        </option>
      ))}
    </select>
  );

  const renderSummaryFields = (fields, manualSection = false) =>
    fields.map(({ f, label }) => {
      const autoJobPct = f === 'grade1JobPercent' || f === 'grade2JobPercent';
      const autoWeeklyTotal = f === 'grade1Total' || f === 'grade2Total';
      const autoTrainingFund =
        f === 'grade1TrainingFundPercent' || f === 'grade2TrainingFundPercent';
      const autoMother = f === 'grade1MotherBenefitPercent' || f === 'grade2MotherBenefitPercent';
      const autoAgeHours = f === 'grade1AgeHours' || f === 'grade2AgeHours';
      const locked =
        manualSection ? false : autoJobPct || autoWeeklyTotal || autoTrainingFund || autoMother || autoAgeHours;
      return (
        <div key={f} className="col-6 col-md-4">
          <label className="form-label small mb-0">{label}</label>
          <input
            type="number"
            className="form-control form-control-sm"
            step="0.01"
            value={form[f] ?? ''}
            onChange={set(f)}
            disabled={locked}
          />
        </div>
      );
    });

  const bandRow = (band) => {
    const k = bandFieldKeys(band);
    const gn = form[k.gradeName] ?? '';
    return (
      <div className="row g-2 mb-2">
        <div className="col-md-3 col-6">
          <label className="form-label small mb-0">שם הדירוג</label>
          <select
            className="form-select form-select-sm"
            value={gn}
            onChange={setBandField(band, 'gradeName')}
          >
            <option value=""></option>
            {GRADE_NAMES.map((name) => (
              <option key={name} value={name}>
                {name}
              </option>
            ))}
          </select>
        </div>
        <div className="col-md-2 col-6">
          <label className="form-label small mb-0">דרגה</label>
          <select
            className="form-select form-select-sm"
            value={form[k.grade] ?? ''}
            onChange={setBandField(band, 'grade')}
            disabled={!gn}
          >
            <option value=""></option>
            {(GRADE_OPTIONS[gn] || []).map((grade) => (
              <option key={grade} value={grade}>
                {grade}
              </option>
            ))}
          </select>
        </div>
        <div className="col-md-3 col-6">
          <label className="form-label small mb-0">תפקיד</label>
          <select
            className="form-select form-select-sm"
            value={form[k.role] ?? ''}
            onChange={setBandField(band, 'role')}
            disabled={!gn}
          >
            <option value=""></option>
            {(ROLE_OPTIONS[gn] || []).map((role) => (
              <option key={role} value={role}>
                {role}
              </option>
            ))}
          </select>
        </div>
        <div className="col-md-2 col-6">
          <label className="form-label small mb-0">ותק</label>
          <input
            type="number"
            className="form-control form-control-sm"
            min="0"
            step="1"
            value={form[k.seniority] ?? ''}
            onChange={setBandField(band, 'seniority')}
          />
        </div>
      </div>
    );
  };

  const gradeBlock = (band) => {
    const manualFields = band === 1 ? summaryManualG1 : summaryManualG2;
    const autoFields = band === 1 ? summaryAutoG1 : summaryAutoG2;
    return (
      <div className="card h-100 border shadow-sm">
        <div className="card-header py-2 px-3 d-flex align-items-center gap-2 bg-body-tertiary">
          <i className="bi bi-layers text-primary"></i>
          <span className="fw-semibold">דרגה {band}</span>
        </div>
        <div className="card-body py-3">
          <div className="row g-2 mb-2">
            {renderSummaryFields(manualFields, true)}
            {renderSummaryFields(autoFields, false)}
          </div>
          {bandRow(band)}
          <div className="table-responsive mt-2 pt-2 border-top">
            <table className="table table-sm table-bordered align-middle mb-0">
              <thead className="table-light">
                <tr>
                  <th style={{ width: '2.5rem' }}>#</th>
                  <th>סמל מוסד</th>
                  <th>שעות שבועיות</th>
                  <th>בסיס משרה</th>
                </tr>
              </thead>
              <tbody>
                {Array.from({ length: 6 }, (_, j) => {
                  const seg = j + 1;
                  const idx = (band - 1) * 6 + (seg - 1);
                  const row = form.slots[idx];
                  const isSupplementary =
                    row.supplementaryParentSlotIndex != null &&
                    row.supplementaryParentSlotIndex !== '';
                  return (
                    <tr key={seg} className={isSupplementary ? 'table-info' : undefined}>
                      <td className="text-muted align-middle">
                        <span>{seg}</span>
                      </td>
                      <td>
                        <select
                          className="form-select form-select-sm"
                          value={row.institutionSymbol}
                          onChange={setSlot(idx, 'institutionSymbol')}
                          disabled={isSupplementary}
                        >
                          <option value=""></option>
                          {institutionSymbols.map((symbol) => (
                            <option key={symbol.institutionSymbol} value={symbol.institutionSymbol}>
                              {symbol.institutionSymbolName
                                ? `${symbol.institutionSymbol} - ${symbol.institutionSymbolName}`
                                : symbol.institutionSymbol}
                            </option>
                          ))}
                        </select>
                      </td>
                      <td>
                        <input
                          type="number"
                          className="form-control form-control-sm"
                          step="0.01"
                          value={row.weeklyHours}
                          onChange={setSlot(idx, 'weeklyHours')}
                          disabled={isSupplementary}
                        />
                      </td>
                      <td>
                        <input
                          type="number"
                          className="form-control form-control-sm"
                          step="0.01"
                          value={row.jobBase}
                          onChange={setSlot(idx, 'jobBase')}
                          readOnly
                          title="מחושב אוטומטית: בסיס לפי דירוג ותפקיד פחות שעות גיל"
                        />
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        </div>
      </div>
    );
  };

  return (
    <>
      <div className="row g-3 mb-3">
        <div className="col-md-4 col-lg-3">
          <label className="form-label">
            שנת לימודים {academicYearOptional ? '(אופציונלי)' : '*'}
          </label>
          {selYear}
        </div>
      </div>

      <div className="row g-3">
        <div className="col-lg-6">{gradeBlock(1)}</div>
        <div className="col-lg-6">{gradeBlock(2)}</div>
      </div>
    </>
  );
}

function EmploymentDataSlotsReadonlyTable({ rows, fmt, fmtNum }) {
  const sorted = [...(rows || [])].sort((a, b) => a.slotIndex - b.slotIndex);
  return (
    <div className="table-responsive mt-2 pt-2 border-top">
      <table className="table table-sm table-bordered align-middle mb-0">
        <thead className="table-light">
          <tr>
            <th style={{ width: '2.5rem' }}>#</th>
            <th>סמל מוסד</th>
            <th>שעות שבועיות</th>
            <th>בסיס משרה</th>
          </tr>
        </thead>
        <tbody>
          {sorted.map((s) => {
            const supplementary =
              s.supplementaryParentSlotIndex != null && s.supplementaryParentSlotIndex !== '';
            return (
              <tr key={s.id ?? `${s.gradeBand}-${s.slotIndex}`} className={supplementary ? 'table-info' : undefined}>
                <td className="text-muted align-middle">
                  <span>{s.slotIndex}</span>
                  {supplementary ? (
                    <span className="d-block small text-primary">שעות +מחנך</span>
                  ) : null}
                </td>
                <td>{fmt(s.institutionSymbol)}</td>
                <td>{fmtNum(s.weeklyHours)}</td>
                <td>{fmtNum(s.jobBase)}</td>
              </tr>
            );
          })}
        </tbody>
      </table>
    </div>
  );
}

/** אותה מערכת כותרות ומבנה כמו EmploymentDataFormSections — להצגה בלבד */
function EmploymentDataGradeReadonlyBlock({ rec, band, fmtNum, fmt }) {
  const manualFields = band === 1 ? summaryManualG1 : summaryManualG2;
  const autoFields = band === 1 ? summaryAutoG1 : summaryAutoG2;
  const p = band === 1 ? 'grade1' : 'grade2';
  const slotRows = (rec.slots || []).filter((s) => s.gradeBand === band);
  return (
    <div className="card h-100 border shadow-sm">
      <div className="card-header py-2 px-3 d-flex align-items-center gap-2 bg-body-tertiary">
        <i className="bi bi-layers text-primary"></i>
        <span className="fw-semibold">דרגה {band}</span>
      </div>
      <div className="card-body py-3">
        <div className="row g-2 mb-2">
          {manualFields.map(({ f, label }) => (
            <div key={f} className="col-6 col-md-4">
              <label className="form-label small mb-0">{label}</label>
              <div className="form-control form-control-sm bg-light border">{fmtNum(rec[f])}</div>
            </div>
          ))}
          {autoFields.map(({ f, label }) => (
            <div key={f} className="col-6 col-md-4">
              <label className="form-label small mb-0">{label}</label>
              <div className="form-control form-control-sm bg-light border">{fmtNum(rec[f])}</div>
            </div>
          ))}
        </div>
        <div className="row g-2 mb-2">
          <div className="col-md-3 col-6">
            <label className="form-label small mb-0">שם הדירוג</label>
            <div className="form-control form-control-sm bg-light border">{fmt(rec[`${p}GradeName`])}</div>
          </div>
          <div className="col-md-2 col-6">
            <label className="form-label small mb-0">דרגה</label>
            <div className="form-control form-control-sm bg-light border">{fmt(rec[`${p}Grade`])}</div>
          </div>
          <div className="col-md-3 col-6">
            <label className="form-label small mb-0">תפקיד</label>
            <div className="form-control form-control-sm bg-light border">{fmt(rec[`${p}Role`])}</div>
          </div>
          <div className="col-md-2 col-6">
            <label className="form-label small mb-0">ותק</label>
            <div className="form-control form-control-sm bg-light border">{fmt(rec[`${p}Seniority`])}</div>
          </div>
        </div>
        <EmploymentDataSlotsReadonlyTable rows={slotRows} fmt={fmt} fmtNum={fmtNum} />
      </div>
    </div>
  );
}

export function EmploymentDataRecordDisplay({ rec, omitYearRow = false, fmtNum, fmt }) {
  const textFmt = fmt ?? ((v) => (v == null || v === '' ? '—' : String(v)));
  return (
    <>
      {!omitYearRow ? (
        <div className="row g-3 mb-3">
          <div className="col-md-4 col-lg-3">
            <label className="form-label">שנת לימודים *</label>
            <div className="form-control form-control-sm bg-light border">{String(rec.academicYear ?? '')}</div>
          </div>
        </div>
      ) : null}
      <div className="row g-3">
        <div className="col-lg-6">
          <EmploymentDataGradeReadonlyBlock rec={rec} band={1} fmtNum={fmtNum} fmt={textFmt} />
        </div>
        <div className="col-lg-6">
          <EmploymentDataGradeReadonlyBlock rec={rec} band={2} fmtNum={fmtNum} fmt={textFmt} />
        </div>
      </div>
    </>
  );
}
