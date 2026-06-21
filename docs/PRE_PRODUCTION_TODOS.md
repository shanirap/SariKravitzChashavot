# TODO לפני העלאה לפרודקשן

רשימה מתומצתת — רק מה **נותר לעשות**.  
מדיניות נוכחית ותיקונים שכבר בוצעו: ר' [EDGE_CASES_TODOS.md](./EDGE_CASES_TODOS.md) (appendix).

**בדיקות:** 430+ Backend + 112 Frontend (מאי 2026)

---

## החלטת מוצר — הרשאות

**כל המשתמשים במערכת הם Admin בלבד.**

- יצירת משתמש בשרת תמיד שומרת `Role = Admin` (שדה Role מה-client מתעלמים ממנו).
- migration `NormalizeAllUsersToAdmin` + `server/Scripts/NormalizeAllUsersToAdmin.sql` מעדכנים משתמשים קיימים.
- **employer-scoped read permissions — לא נדרש כרגע** (מדיניות Admin-only).
- Policies `AdminOnly` / `AdminWrite` נשארות — כל משתמש מחובר עובר אותן.
- **אם בעתיד יתווספו משתמשים שאינם Admin** — יש לפתוח מחדש employer-scoped read permissions ו-role policies.

---

## שלב 1 — תפעול (חובה)

- [ ] **Backup / snapshot** ל-DB קיים לפני כל migration
- [ ] הרצת **`db/migration.sql`** על SQL Server (לא `create-db.sql`) — נוצר ע"י `Create-FinalDeploy.ps1`
- [ ] וידוא migrations: `NormalizeAllUsersToAdmin`, `CanonicalizeStoredAcademicYears` (או סקרipts ב-`server/Scripts/`)
- [ ] הגדרת secrets על השרver (לא ב-ZIP):
  - `ConnectionStrings__DefaultConnection`
  - `Jwt__Key`, `Jwt__Issuer`, `Jwt__Audience`
  - `AllowedOrigins` (אם רלוונטי)
- [ ] bootstrap **Admin** ב-production (לא `SeedAdminPassword` מ-dev)
- [ ] SQL Server **לא** חשוף ל-1433 באינטרנט
- [ ] הרצת `Create-FinalDeploy.ps1` → בדיקת ZIP (`backend/`, `wwwroot/`, `db/migration.sql`)
- [ ] smoke test אחרי עלייה: login, מעסיק, עובד, employment, דוח אחד
- [ ] spot-check: שורות `שנת_לימודים` שלא נרמלו אוטומטית (ערכים לא מזוהים) — תיקון ידני

---

## שלב 2 — מומלץ מאוד (איכות נתונים)

### 2.1 AddEmployee — עובד בלי employment
**בעיה:** עובד נוצר, employment נכשל → רשומה יתומה ב-DB.

- [ ] Backend: endpoint אטומי `POST /employees/with-employment` **או**
- [ ] Frontend: הודעה ברורה + קישור לעמוד עובד לתיקון

### 2.2 מחיקה / סטטוס פעיל — scope מעסיק
**בעיה:** `DeleteAsync` ו-`SetManualActiveStatusAsync` בודקים employment לפי `EmployeeId` בלבד.

- [ ] סינון לפי `employee.EmployerId` ב-`EmployeeService`

### 2.3 ייבוא מוני — validation parity עם API
**מצב:** partial row save בייבוא עובדים **תוקן** (ולידציה לפני שינוי tracked entities).  
**נותר:** mother benefit / validation מלא משותף ל-API.

- [ ] shared validator ל-API + bulk import
- [ ] בדיקות parity

---

## שלב 3 — אחרי עלייה / לא blocker

| נושא | תיאור קצר |
|------|-----------|
| Race conditions | concurrent create → 500 במקום 409 |
| דוח השוואה | TZ/מספר עובד כפול → `.First()` שרירותי |
| ייבוא מעסיקים | ללא transaction באמצע קובץ |
| Frontend pagination | עמוד ריק אחרי מחיקה |
| Frontend race | `EmployeeDetails` load ללא abort |
| Unsaved edits | modal דוח שנתי |
| `/settings` | **לא קיים** — רק אם נדרש מוצרית |
| employer-scoped read | רק אם בעתיד יוחזרו roles שאינם Admin |

---

## Checklist מהיר — "מוכן לפרוד?"

```
[ ] backup + migration.sql (Users Admin + AcademicYear canonical)
[ ] secrets על השרver
[ ] smoke test
[ ] בייבוא — עמודת חפ כשיש שמות מעסיק כפולים
```

---

## מה **כבר** מוכן (לא לחזור עליו)

- כל המשתמשים Admin (שרת + migration)
- ייבוא עובדים: שורה שגויה לא שומרת Employee/Employment (`BulkImportPartialRowSaveTests`)
- שנת לימודים: `TryValidateAndCanonicalize` + `CanonicalForComparison` בדוחות; migration `CanonicalizeStoredAcademicYears`
- ייבוא: ח.פ. + שם ייחודי + validation שנת לימודים
- חסימת שינוי `EmployerId` לעובד
- `slots: null` → 400
- כתיבה/מחיקה/ייבוא — Admin בלבד (`AdminWrite`)
- soft delete + שחזור עובד/מעסיק (מתועד)
- README פריסה + `ProductionCriticalFixesTests`

---

*עדכון: מאי 2026*
