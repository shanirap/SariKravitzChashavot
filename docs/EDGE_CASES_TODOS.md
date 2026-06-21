# TODO — מקרי קצה ופתרונות מוצעים

> **לפני פרודקשן — רשימה מתומצתת:** [PRE_PRODUCTION_TODOS.md](./PRE_PRODUCTION_TODOS.md)

מסמך זה הוא **ארכיון מפורט** של כל הבעיות שזוהו בסקירה (Backend + Frontend), כולל תיקונים שבוצעו והסברים. לעבודה יומיומית לפני עלייה — השתמש ב-`PRE_PRODUCTION_TODOS.md`.

**סטטוס בדיקות נוכחי:** 421 בדיקות Backend + 112 Frontend = **533 עוברות**.

**עדכון אחרון:** מאי 2026 (לאחר תיקוני production)

---

## תיקוני production שבוצעו (מאי 2026)

| פריט | מה נעשה | קבצים עיקריים |
|------|---------|----------------|
| ייבוא לפי שם/ח.פ. | `ResolveEmployerForImportRowAsync` — ח.פ. קודם; שם לא ייחודי → שגיאה; עמודת `חפ` בתבנית | `BulkImportService.cs` |
| שנת לימודים בייבוא | `TryValidateAndCanonicalize` במקום `Normalize` | `BulkImportService.cs` |
| שינוי EmployerId | חסימה מלאה ב-`UpdateAsync` | `EmployeeService.cs` |
| slots null | validation → 400 `"מקטעי העסקה חסרים."` | `EmploymentDataService.cs`, `EmploymentDataDto.cs` |
| הרשאות כתיבה | `[AdminWrite]` (= AdminOnly) על POST/PUT/PATCH/DELETE | כל ה-controllers העסקיים |
| README פריסה | `db/migration.sql`, חובת backup | `deploy-output/README_FOR_IT.txt` |
| בדיקות | `ProductionCriticalFixesTests.cs` (8 בדיקות) | `AccountingProject.Tests/` |

**לא בוצע / לא רלוונטי:**
- **`/settings`** — אין route, אין קומפוננטה, לא היה בהיסטוריית git → לא שוחזר.
- **TODO-1.1 מלא** — הפרדה לפי מעסיק ב-GET; רק כתיבה הוגבלה ל-Admin.

---

## סיכום מנהלים

| חומרה | פתוח | טופל / חלקי | דוגמאות |
|--------|------|-------------|---------|
| **גבוהה** | 1 | 3 ✅ + 1 🔄 | נשאר: הפרדת קריאה לפי מעסיק; טופל: ייבוא, שנה, EmployerId |
| **בינונית** | ~18 | 1 ✅ + 1 🔄 | slots null ✅; שנת לימודים בדוחות 🔄 |
| **נמוכה** | ~6 | 1 ✅ | soft delete employment מתועד |

**Legend:** ⬜ לא התחיל | 🔄 חלקי | ✅ הושלם | 📋 מתועד (by design)

---

## שלב 1 — עדיפות גבוהה (קריטי)

### TODO-1.1 — אין הפרדת הרשאות לפי מעסיק

**בעיה:**  
כל משתמש מחובר יכול **לקרוא** (GET) נתונים של **כל** מעסיק, אם יודע את ה-`employerId` / `employeeId` / `rowId`.  
`Program.cs` מגדיר `FallbackPolicy = authenticated` בלבד; אין scope למעסיק.

**מה כבר תוקן (מאי 2026):**  
פעולות **כתיבה/מחיקה/ייבוא/עדכון** מוגבלות ל-**Admin** בלבד (`[AdminWrite]` על POST/PUT/PATCH/DELETE). Viewer/PayrollManager — read-only לכתיבה.

**מה עדיין פתוח:**  
- גישת **GET cross-employer** לכל משתמש מחובר
- **PayrollManager** — read-only גם לכתיבה (אם צריך לו הרשאות — policy נפרד)
- multi-tenant: `UserEmployerAccess`, authorization handler לפי מעסיק

**קבצים רלוונטיים:**
- `server/Program.cs`
- `server/Infrastructure/AuthPolicies.cs` — `AdminWriteAttribute`
- `server/Controllers/*.cs`

**פתרון מוצע (המשך):**
1. טבלת `UserEmployerAccess` (UserId + EmployerId)
2. Middleware / Authorization handler שבודק `employerId` בבקשה
3. integration tests: משתמש A לא יכול לגשת ל-`employerId` של B

**בדיקות:**
- ✅ `ProductionCriticalFixesTests.Employees_Create_AsViewer_Returns403`
- ⬜ הרחב `ApiRoleAuthorizationIntegrationTests` — cross-employer GET

**סטטוס:** 🔄 **חלקי** — Admin-only writes ✅; employer-scoped read ⬜

---

### TODO-1.2 — ייבוא מוני: חיפוש מעסיק לפי שם בלבד

**בעיה (לפני תיקון):**  
חיפוש `FirstOrDefaultAsync(e => e.Name == ...)` — שם לא ייחודי → מעסיק שגוי.

**מה בוצע (מאי 2026):**
- `ResolveEmployerForImportRowAsync` — **ח.פ. (`חפ`) קודם** לשם
- שם לא ייחודי → `"שם המעסיק \"...\" אינו ייחודי — יש להזין ח.פ. בעמודת \"חפ\""`
- עמודת **`חפ`** בתבנית ייבוא (כש-`includeEmployerName=true`)
- ייבוא מתוך מסך מעסיק — `employerId` קבוע (ללא חיפוש)

**קבצים:** `server/Services/BulkImportService.cs`

**בדיקות:**
- ✅ `ProductionCriticalFixesTests.ImportEmployees_NonUniqueEmployerName_ReturnsRowError`
- ✅ `ProductionCriticalFixesTests.ImportEmployees_ResolvesEmployerByBusinessNumber_WhenNameIsDuplicate`
- ✅ `BulkImportEmployeeFieldsTests` — עמודת `חפ` בתבנית

**סטטוס:** ✅ **הושלם**

---

### TODO-1.3 — ייבוא מוני: שנת לימודים לא מאומתת כמו ב-API

**בעיה (לפני תיקון):**  
`HebrewAcademicYear.Normalize` בייבוא — ערכים לא תקינים נשמרו.

**מה בוצע (מאי 2026):**  
`TryValidateAndCanonicalize` בייבוא — שורה לא תקינה נדחית.

**מה עדיין פתוח:**
- `ReportExportService` — `e.AcademicYear == academicYear` (התאמה מדויקת) → **TODO-2.10**
- Migration לנרמול שנים ישנות ב-DB (אופציונלי)

**בדיקות:**
- ✅ `ProductionCriticalFixesTests.ImportEmployees_InvalidAcademicYear_ReturnsRowError`

**סטטוס:** 🔄 **חלקי** — ייבוא ✅; דוחות ⬜

---

### TODO-1.4 — עדכון עובד: שינוי EmployerId בלי בדיקת עקביות

**בעיה (לפני תיקון):**  
`UpdateAsync` עדכן `EmployerId` ללא בדיקה → נתונים לא עקביים.

**מה בוצע (מאי 2026):**  
חסימה **מלאה** של שינוי `EmployerId` בעריכה — `"לא ניתן לשנות את המעסיק של עובד קיים."`

**קבצים:** `server/Services/EmployeeService.cs`

**בדיקות:**
- ✅ `ProductionCriticalFixesTests.UpdateAsync_ChangingEmployerId_ThrowsHebrewValidationError`
- ✅ `ProductionCriticalFixesTests.UpdateAsync_ChangingEmployerId_WithoutEmployment_StillThrows`

**פתרון עתידי (אופציונלי):** Frontend — `EmployerId` readonly בטופס עריכה.

**סטטוס:** ✅ **הושלם** (backend)

---

## שלב 2 — Backend: שלמות נתונים ו-validation

### TODO-2.1 — EmploymentData: `slots: null` גורם ל-500

**בעיה (לפני תיקון):**  
`foreach (var s in dto.Slots)` לפני null-check → `NullReferenceException`.

**מה בוצע (מאי 2026):**
- `ValidateAsync`: `if (dto.Slots == null) return "מקטעי העסקה חסרים.";`
- `EmploymentDataDto.Slots` — nullable (`List<...>?`) כדי ש-null יגיע ל-validation

**בדיקות:**
- ✅ `ProductionCriticalFixesTests.CreateAsync_NullSlots_ReturnsValidationMessage`
- ✅ `ProductionCriticalFixesTests.EmploymentData_Create_WithNullSlots_Returns400`

**סטטוס:** ✅ **הושלם**

---

### TODO-2.2 — מחיקת עובד: employment ממעסיק אחר חוסם מחיקה

**בעיה:**  
`EmployeeService.DeleteAsync` — `_db.EmploymentData.AnyAsync(ed => ed.EmployeeId == id)` **ללא** סינון `EmployerId`.

**השפעה:** עובד אצל מעסיק A עם employment רק אצל B (אותה ת.ז. במערכת כרשומות נפרדות — לא רלוונטי), אבל **אותה רשומת Employee** עם employment תחת employer אחר (אם הועבר ב-bug 1.4) — נחסם.

**פתרון:** סנן לפי `ed.EmployerId == employee.EmployerId` (employment של אותו מעסיק בלבד).

**בדיקות:** הרחב `EmployeeIntegrityTests.DeleteAsync_WhenEmploymentDataExists_ReturnsFailure` — employment באותו מעסיק חוסם; employment במעסיק אחר (אם אפשרי במודל) לא חוסם.

**סטטוס:** ⬜ לא טופל

---

### TODO-2.3 — סימון עובד כ"פעיל": employment ממעסיק אחר מספיק

**בעיה:**  
`SetManualActiveStatusAsync` — `AnyAsync(ed => ed.EmployeeId == id)` ללא `EmployerId`.

**פתרון:** דרוש employment פעיל **לאותו** `employee.EmployerId`.

**בדיקות:** integration test — employment רק במעסיק אחר → PATCH active-status נדחה.

**סטטוס:** ⬜ לא טופל

---

### TODO-2.4 — Race conditions על unique indexes

**בעיה:**  
Create/restore במקביל (employee, employer, payroll batch) — check-then-act ללא isolation → `DbUpdateException` לא ממופה → 500.

**מיקומים:**
- `EmployeeService.CreateOrGetAsync`
- `EmployerService.CreateAsync`
- `PayrollMonthlyInputService.ImportMonthAsync`

**פתרון:**
1. תפוס `DbUpdateException` ב-controllers/services והחזר 409 Conflict עם הודעה בעברית.
2. **אופציונלי:** retry idempotent ב-`CreateOrGetAsync`.
3. **אופציונלי:** transaction + serializable לייבוא payroll חודשי.

**בדיקות:** concurrency tests (2 tasks parallel) — לפחות אחד מצליח, השני מקבל 409 לא 500.

**סטטוס:** ⬜ לא טופל

---

### TODO-2.5 — דוח השוואה: `.First()` על TZ/מספר עובד כפול

**בעיה:**  
`ComparisonReportService` — `GroupBy(...).ToDictionary(..., g => g.First())` — אם יש 2 עובדים עם אותו מספר/TZ, נבחר אחד שרירותי.

**פתרון:**
1. זיהוי כפילויות → שגיאה או אזהרה בדוח.
2. או: כלל עסקי ברור (למשל לפי `EmployeeId` מהקובץ).

**בדיקות:** 2 employees, אותו `EmployeeNumber` → behavior מוגדר (error/warning).

**סטטוס:** ⬜ לא טופל

---

### TODO-2.6 — ייבוא מוני: validation לא זהה ל-API

**בעיה:**  
`BulkImportService` יוצר slots ישירות — לא קורא `EmploymentDataService.ValidateAsync`, `TeacherSupplementarySlotRules`, mother-benefit rules.

**השפעה:** נתונים לא תקינים ב-DB; עריכה ב-UI/API תידחה.

**פתרון:**
1. Refactor: בנה `EmploymentDataDto` וקרא `ValidateAsync` לפני שמירה.
2. או: shared validator class ל-API + bulk import.

**בדיקות:** parity tests — אותה שורה שנדחית ב-API נדחית גם בייבוא.

**סטטוס:** ⬜ לא טופל

---

### TODO-2.7 — ייבוא מעסיקים: ללא transaction

**בעיה:**  
`ImportEmployersAsync` — `SaveChangesAsync` כל 100 שורות, ללא transaction. כשל באמצע → חלק מהמעסיקים נשמרו.

**פתרון:** `BeginTransactionAsync` + rollback on fatal error; או idempotent re-import (restore soft-deleted).

**בדיקות:** simulate failure mid-file → 0 או הכל (לפי policy).

**סטטוס:** ⬜ לא טופל

---

### TODO-2.8 — ייבוא עובדים: import חלקי (by design)

**בעיה:**  
Transaction מתבצע commit גם כשחלק מהשורות נכשלו — `Imported` חלקי.

**פתרון (מוצר):**
1. **אופציה A:** all-or-nothing (rollback אם יש שגיאות).
2. **אופציה B:** שמור partial + דו"ח מפורט (קיים?) — וודא UI מציג בבירור.

**בדיקות:** קובץ 10 שורות, 3 שגויות → assert policy.

**סטטוס:** ⬜ לא טופל / לתעד כ-hknown behavior

---

### TODO-2.9 — עריכת שורות payroll: ללא validation

**בעיה:**  
`PayrollMonthlyInputService.UpdateRowAsync` / `ApplyEdit` — שדות שליליים, TZ ריק, וכו'.

**פתרון:** validation layer (שעות ≥ 0, TZ format, שדות חובה) — mirror rules מ-upload parser.

**בדיקות:** `PayrollMonthlyInputServiceEdgeCaseTests` — negative hours → reject.

**סטטוס:** ⬜ לא טופל

---

### TODO-2.10 — דוחות: התאמת שנת לימודים לא אחידה

**בעיה:**  
- `ReportExportService`: `e.AcademicYear == academicYear` (exact)
- `ComparisonReportService`: `CanonAcademicYear(ed.AcademicYear) == academicYearCanon`

**פתרון:** utility משותף `EmploymentMatchesAcademicYear(ed, canonicalYear)` בכל הדוחות.

**בדיקות:** employment עם `5786` נמצא בדוח שמבקש `תשפ"ו`.

**סטטוס:** ⬜ לא טופל

---

### TODO-2.11 — `CanonAcademicYear` fallback למחרוזת גולמית

**בעיה:**  
`PayrollComparisonUploadSupport.CanonAcademicYear` — אם normalize נכשל, מחזיר trimmed raw.

**פתרון:** throw / reject במקום fallback; או log warning + reject at service boundary.

**בדיקות:** garbage input → exception, not silent pass-through.

**סטטוס:** ⬜ לא טופל

---

### TODO-2.12 — soft delete employment: יצירה חוזרת לאותה שנה

**בעיה (ידוע / by design):**  
אחרי soft delete, `CreateAsync` מאפשר רשומה חדשה — נשארות 2 שורות ב-DB (אחת deleted).

**פתרון (אם רוצים לשנות):**
- restore במקום create חדש
- או: unique index filtered על active only (כבר קיים) + UI שמציע "שחזר רשומה קודמת"

**בדיקות:** ✅ `EmploymentDataSoftDeleteTests` — קיים

**סטטוס:** ✅ מתועד; החלטת מוצר אם לשנות UX

---

## שלב 3 — Frontend: UX, validation, state

### TODO-3.1 — AddEmployee: עובד נוצר, employment נכשל (orphan)

**בעיה:**  
`AddEmployee.handleSubmit` — `employeesApi.create` ואז `employmentDataApi.create` ברצף; כשל בשני → עובד כבר קיים ב-DB.

**קבצים:** `client/src/pages/AddEmployee.jsx` (~171–199)

**פתרון:**
1. **Backend:** endpoint אטומי `POST /employees/with-employment` (transaction).
2. **Frontend:** אם employment נכשל — הצג "עובד נוצר; נסה להוסיף נתוני העסקה" + קישור לעמוד עובד; אופציונלי soft-delete employee.
3. **UX:** confirm restore אם `restoredFromSoftDelete`.

**בדיקות:** mock employment failure → assert message + navigation.

**סטטוס:** ⬜ לא טופל

---

### TODO-3.2 — Validation לא סימטרי: AddEmployee vs EmployeeDetails

**בעיה:**  
AddEmployee משתמש ב-`validateAddEmployeeEmploymentSection`; EmployeeDetails דורש רק `academicYear` לא ריק.

**פתרון:**  
חלץ validator משותף; קרא משני הדפים; align עם server `ValidateAsync`.

**בדיקות:** component tests — sparse employment נדחה בשני הדפים.

**סטטוס:** ⬜ לא טופל

---

### TODO-3.3 — Slot עם סמל בלי שעות נשלח ל-API

**בעיה:**  
`shouldPersistEmploymentSlot` שומר slot עם symbol גם בלי hours; `employmentSectionHasStructuredContent` דורש שניהם.

**פתרון:**  
align rules — אל תשלח slot בלי `weeklyHours` parseable > 0 (או server ידחה).

**בדיקות:** unit test `buildEmploymentPayloadFromForm` — symbol only → slots empty.

**סטטוס:** ⬜ לא טופל

---

### TODO-3.4 — Grade name בלבד עובר validation

**בעיה:**  
`employmentSectionHasStructuredContent` true אם רק `grade1GradeName` מלא → save עם `slots: []`.

**פתרון:** דרוש לפחות role + slot עם symbol+hours, או explicit "טיוטה" mode.

**סטטוס:** ⬜ לא טופל

---

### TODO-3.5 — EmployeeDetails: race בטעינה

**בעיה:**  
`load()` ללא `AbortController` / mounted guard — navigation מהירה → state ישן דורס חדש.

**פתרון:**  
```javascript
useEffect(() => {
  const ac = new AbortController();
  load(ac.signal);
  return () => ac.abort();
}, [employeeId, employerId]);
```
+ ignore results if aborted.

**בדיקות:** rapid route change test (RTL).

**סטטוס:** ⬜ לא טופל

---

### TODO-3.6 — Pagination: עמוד ריק אחרי מחיקה/סינון

**בעיה:**  
`EmployerDetails` מאפס `page` רק בשינוי filter, לא כש-`totalCount` קטן (מחיקה בעמוד 3).

**פתרון:**  
`useEffect` — if `page > totalPages` set `page` to `Math.max(1, totalPages)`.

**קבצים:** `client/src/pages/EmployerDetails.jsx`, `Pagination.jsx`

**בדיקות:** `EmployerDetails.filters.test.jsx` — pagination edge case.

**סטטוס:** ⬜ לא טופל

---

### TODO-3.7 — AnnualComparisonSavedReportEditor: אובדן עריכות לא שמורות

**בעיה:**  
סגירת modal / `loadPreview()` — `dirtyRows` נזרקים ללא confirm.

**פתרון:**  
`beforeunload` / modal `onHide` — "יש שינויים שלא נשמרו".

**בדיקות:** close with dirty → confirm shown.

**סטטוס:** ⬜ לא טופל

---

### TODO-3.8 — ייצוא לפני שמירה — Excel לא תואם למסך

**בעיה:**  
`handleExport` לא שומר `dirtyRows` קודם.

**פתרון:**  
disable export if dirty + tooltip; או auto-save prompt; או export from client state (מורכב).

**סטטוס:** ⬜ לא טופל

---

### TODO-3.9 — PayrollMonthlyRowsEditor: מספר לא תקין → null בשקט

**בעיה:**  
`parseOptionalDecimal` → null; user חושב שנשמר.

**פתרון:** validation inline + error message per field.

**בדיקות:** `PayrollMonthlyRowsEditor.test.jsx` — invalid input shows error.

**סטטוס:** ⬜ לא טופל

---

### TODO-3.10 — 401 redirect מאבד טופס

**בעיה:**  
`api.js` interceptor — `window.location.replace('/login')` על 401.

**פתרון (אופציונלי):**  
session expiry modal + save draft to sessionStorage לפני redirect.

**בדיקות:** ✅ `api.interceptors.test.js` — קיים

**סטטוס:** ⬜ לא טופל (UX improvement)

---

### TODO-3.11 — תאריכי לידת ילדים stale בעריכת employment

**בעיה:**  
`toFormRec` snapshot בפתיחה; עדכון ילדים ב-EmployerDetails לא מתעדכן ב-form פתוח.

**פתרון:**  
reload employee on focus / refetch before save; או הודעה "נתוני ילדים עודכנו — רענן".

**סטטוס:** ⬜ לא טופל

---

### TODO-3.12 — EmployerDetails: אין בדיקת תאריך לידה עתידי

**בעיה:**  
AddEmployee חוסם future birthDate; EmployerDetails edit לא.

**פתרון:** shared `validateEmployeeFields` ב-client + server.

**סטטוס:** ⬜ לא טופל

---

### TODO-3.13 — ImportEmployees: ללא בדיקת סוג קובץ

**בעיה:**  
כל קובץ נשלח לשרת; `AnnualComparisonSavedPanel` בודק `.xlsx`.

**פתרון:** `isXlsxFile` לפני upload + הודעה.

**סטטוס:** ⬜ לא טופל

---

## שלב 4 — חישובים ושנות לימודים (client)

### TODO-4.1 — שעות גיל מוחקות job base → job % ריק

**בעיה:**  
`netJobBaseAfterAgeHours` → 0; harmonic skip → `computeGradeJobPercentString` מחזיר `''` בעוד totals מוצגים.

**פתרון:**  
הצג 0% או הודעה "לא ניתן לחשב"; align display rules.

**בדיקות:** edge case test — all rows skipped.

**סטטוס:** ⬜ לא טופל

---

### TODO-4.2 — שעות שליליות / אפס ב-client

**בעיה:**  
`N()` / `parseFloat` ללא lower bound.

**פתרון:** clamp או validation `weeklyHours >= 0`.

**בדיקות:** negative hours → error or clamp.

**סטטוס:** ⬜ לא טופל

---

### TODO-4.3 — mother benefit לפני בחירת שנת לימודים

**בעיה:**  
שנה ריקה → fallback ל-`currentHebrewAcademicYear()` — preview שונה מהשנה שתיבחר.

**פתרון:**  
אל תחשב mother benefit עד ש-year נבחר; או disable preview.

**בדיקות:** ✅ partial — `motherBenefit.test.js`

**סטטוס:** ⬜ לשפר

---

### TODO-4.4 — שנה עברית לא תקינה → ref date = היום

**בעיה:**  
`parseSeptemberGregorianYear('xyz')` → null → `new Date()` (לא Sep 1).

**פתרון:**  
align עם server — treat invalid as error / disable calculations.

**בדיקות:** invalid year string behavior.

**סטטוס:** ⬜ לא טופל

---

### TODO-4.5 — טווחי שנים: טפסים vs דוחות

**בעיה:**  
Employment: −20/+5; Reports: ±7 — שנים תקינות ב-DB לא ניתנות לבחירה בדוחות.

**פתרון:**  
constants משותפים; או dynamic range from API; min/max config.

**סטטוס:** ⬜ לא טופל (נמוך)

---

### TODO-4.6 — client לא מנרמל שנים מספריות

**בעיה:**  
Server מקבל `5786`; client dropdown רק אותיות עבריות.

**פתרון:**  
shared normalization helper (port `HebrewAcademicYear` logic or API endpoint).

**סטטוס:** ⬜ לא טופל

---

### TODO-4.7 — teacher supplementary slots רק מ-segments 1–5

**בעיה:**  
`syncTeacherSupplementarySlots` loop `1..5` — slot 6 parent לא יוצר +3h.

**פתרון:**  
הרחב ל-6 אם business rules מאפשר; או document limitation.

**סטטוס:** ⬜ לא טופל (נמוך)

---

## שלב 5 — דוחות ו-API (נמוך)

### TODO-5.1 — דוח שנתי למעסיק לא קיים → Excel ריק (200)

**בעיה:**  
`ReportExportService.BuildAnnualRosterByInstitutionTypeAsync` — no employer check.

**פתרון:**  
`EnsureEmployerExists` → 404.

**סטטוס:** ⬜ לא טופל

---

### TODO-5.2 — Annual comparison preview ללא slots → exception

**בעיה:**  
`GetPreviewAsync` throws `"לא נמצאו מקטעי העסקה להשוואה."`

**פתרון:**  
empty preview + status "חסר" (consistent with payroll status endpoint).

**סטטוס:** ⬜ לא טופל

---

### TODO-5.3 — חיפוש ב-EmployerDetails דורש submit

**בעיה:**  
`searchInput` ≠ `search` — UX confusion.

**פתרון:**  
debounced auto-search או label ברור "לחץ חפש".

**סטטוס:** ⬜ לא טופל (נמוך)

---

## מה כבר מכוסה בבדיקות

| נושא | קובץ בדיקות |
|------|-------------|
| שחזור עובד soft delete | `EmployeeRestoreTests.cs` |
| שחזור מעסיק soft delete | `EmployerRestoreTests.cs` |
| employment אחרי soft delete | `EmploymentDataSoftDeleteTests.cs` |
| מחיקת עובד עם employment | `EmployeeIntegrityTests.cs` |
| validation שנת לימודים API | `InvalidAcademicYearApiIntegrationTests` |
| payroll import edge cases | `PayrollMonthlyInputServiceEdgeCaseTests` |
| employment calculations | `employmentDataHelpers.*.test.js` |
| אופק גנים job base | `employmentDataHelpers.edgeCases.test.js` |
| **תיקוני production (מאי 2026)** | `ProductionCriticalFixesTests.cs` |
| ייבוא — שדות ותבנית | `BulkImportEmployeeFieldsTests.cs`, `BulkImportDateParsingTests.cs` |
| Admin-only writes | `ProductionCriticalFixesTests.Employees_Create_AsViewer_Returns403` |

---

## פריסה ותפעול

| נושא | סטטוס |
|------|--------|
| `db/migration.sql` (לא `create-db.sql`) | ✅ `README_FOR_IT.txt` + `Create-FinalDeploy.ps1` |
| חובת backup לפני migration | ✅ מתועד |
| `/settings` route | ❌ לא קיים — אין קומפוננטה; לא שוחזר |

---

## סדר ביצוע מומלץ (מעודכן)

```
✅ הושלם (production fixes)
├── 1.2  ייבוא לפי ח.פ. + שם ייחודי
├── 1.4  חסימת שינוי EmployerId
├── 2.1  slots null → 400
├── 1.1  Admin-only writes (חלקי)
├── 1.3  validation שנת לימודים בייבוא (חלקי)
└── README + בדיקות ProductionCriticalFixesTests

⬜ לפני פרודקשן (אם multi-tenant / Viewer פעיל)
├── 1.1  employer-scoped GET + UserEmployerAccess
├── 3.1  AddEmployee atomic / compensation
├── 2.2, 2.3  סינון EmployerId ב-delete/active
└── 2.10  canonical academic year בדוחות

⬜ יציבות
├── 2.4  race → 409
├── 2.6  bulk import validation parity
└── 3.5, 3.6  UI races + pagination

⬜ UX + נמוך
├── 3.7, 3.8  unsaved changes
├── 4.x       חישובים client
└── 5.x       דוחות edge cases
```

---

## הערות למ implementer

1. **אל תשנה soft-delete semantics** (TODO-2.12) בלי החלטת מוצר — כבר יש בדיקות.
2. **ייבוא מוני** — עמודת `חפ` בתבנית; מומלץ למלא כשיש מעסיקים עם אותו שם.
3. **הרשאות (TODO-1.1)** — Admin-only writes כבר פעיל; employer-scoped read — לפי מודל עסקי.
4. **PayrollManager** — כיום read-only לכתיבה; אם צריך הרשאות — policy ייעודי.
5. אחרי כל TODO — הוסף בדיקה; הרץ `dotnet test` + `npm test`.

---

## מעקב סטטוס

| ID | כותרת | עדיפות | סטטוס |
|----|--------|--------|--------|
| 1.1 | הרשאות לפי מעסיק | גבוהה | 🔄 Admin writes ✅; GET scope ⬜ |
| 1.2 | ייבוא לפי שם מעסיק | גבוהה | ✅ |
| 1.3 | שנת לימודים בייבוא | גבוהה | 🔄 ייבוא ✅; דוחות ⬜ |
| 1.4 | שינוי EmployerId | גבוהה | ✅ |
| 2.1 | slots null | בינונית | ✅ |
| 2.2 | delete employee scope | בינונית | ⬜ |
| 2.3 | active status scope | בינונית | ⬜ |
| 2.4 | race conditions | בינונית | ⬜ |
| 2.5 | comparison First() | בינונית | ⬜ |
| 2.6 | bulk validation parity | בינונית | ⬜ |
| 2.7 | employer import tx | בינונית | ⬜ |
| 2.8 | partial import policy | בינונית | ⬜ |
| 2.9 | payroll row validation | בינונית | ⬜ |
| 2.10 | academic year in reports | בינונית | ⬜ |
| 2.11 | CanonAcademicYear fallback | בינונית | ⬜ |
| 2.12 | soft delete employment | ידוע | 📋 מתועד |
| 3.1 | AddEmployee orphan | בינונית | ⬜ |
| 3.2–3.13 | Frontend validation/UX | בינונית–נמוכה | ⬜ |
| 4.1–4.7 | Client calculations | בינונית–נמוכה | ⬜ |
| 5.1–5.3 | Reports/API low | נמוכה | ⬜ |
| — | README / migration.sql | תפעול | ✅ |
| — | `/settings` route | — | ❌ לא קיים |

**Legend:** ⬜ לא התחיל | 🔄 חלקי | ✅ הושלם | 📋 מתועד (by design) | ❌ לא רלוונטי
