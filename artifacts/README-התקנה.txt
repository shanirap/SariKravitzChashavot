===================================================
  מערכת שכר — הוראות התקנה לאיש המחשבים
  גרסה: אוקטובר 2026
===================================================

מה בחבילה
----------
  backend\          — קבצי המערכת (ASP.NET Core + React)
  db\migration.sql  — סקריפט DB (הרצה חד-פעמית / מצטבר)
  README-התקנה.txt  — המסמך הזה


דרישות מוקדמות
---------------
  • Windows Server עם IIS מותקן
  • .NET 8 Runtime (Hosting Bundle):
    https://dotnet.microsoft.com/en-us/download/dotnet/8.0
    (הורידו "ASP.NET Core Runtime 8.x — Hosting Bundle")
  • SQL Server (כל גרסה — Express מספיק)
  • הרשאות Administrator בשרת


שלב 1 — הכנת מסד הנתונים
--------------------------
1. פתחו SQL Server Management Studio (SSMS).
2. אם זו התקנה ראשונה — צרו DB חדש, למשל: AccountingDb
3. פתחו את הקובץ:  db\migration.sql
4. הריצו אותו על ה-DB שיצרתם (F5 ב-SSMS).
   הסקריפט הוא idempotent — בטוח להריץ יותר מפעם אחת.


שלב 2 — הגדרת IIS
------------------
1. פתחו IIS Manager.
2. צרו Application Pool חדש:
     שם: AccountingPool
     .NET CLR version: "No Managed Code"
     Pipeline: Integrated
3. צרו Website או Application חדשה:
     Physical path: תיקיית backend\ שחילצתם
     Application Pool: AccountingPool
     Port: לפי הגדרת הרשת (למשל 80 או 8080)


שלב 3 — קובץ הגדרות (חובה!)
------------------------------
צרו קובץ בשם:  appsettings.Production.json
במיקום:        backend\appsettings.Production.json

תוכן הקובץ (מלאו את הערכים בהתאם לסביבה שלכם):

{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\SQLEXPRESS;Database=AccountingDb;Integrated Security=True;TrustServerCertificate=True;"
  },
  "AllowedHosts": "your-server-hostname",
  "AllowedOrigins": ["http://your-server-hostname"],
  "Jwt": {
    "Key": "הכניסו-כאן-מחרוזת-אקראית-של-לפחות-32-תווים",
    "Issuer": "AccountingProject",
    "Audience": "AccountingProjectUsers",
    "ExpirationMinutes": 480
  }
}

הערות:
  • Jwt:Key — חייב להיות סודי וייחודי. צרו מחרוזת אקראית (לפחות 32 תווים).
    דוגמה (אל תשתמשו בדוגמה הזאת!):  xK9#mP2$nL7@qR4^wY1&vB8*cF5!hT3
  • AllowedHosts — שם הדומיין / IP של השרת. לדוגמה: "192.168.1.100"
  • AllowedOrigins — כתובת ה-URL שממנה גולשים. לדוגמה: "http://192.168.1.100"
  • ConnectionStrings — שרת SQL ושם ה-DB שיצרתם בשלב 1.


שלב 4 — יצירת משתמש Admin ראשון
----------------------------------
בפעם הראשונה בלבד — הריצו מהתיקייה backend\ :

  AccountingProject.exe --bootstrap-admin

המערכת תשאל שם משתמש וסיסמה ותיצור חשבון Admin.
אחרי יצירת המשתמש — הפעילו מחדש את האפליקציה ב-IIS.


שלב 5 — בדיקת תקינות (Smoke Test)
------------------------------------
פתחו דפדפן וגשו לכתובת המערכת.
בדקו:
  [ ] דף Login מוצג
  [ ] כניסה עם המשתמש שיצרתם
  [ ] רשימת מעסיקים מוצגת
  [ ] ניווט לפרטי מעסיק ועובד עובד
  [ ] הנפקת דוח בסיסי


עדכון גרסה (הפעם הבאה)
------------------------
1. גיבוי DB לפני הכל.
2. הריצו db\migration.sql — בטוח לריצה חוזרת.
3. החליפו את תכולת backend\ בקבצים החדשים.
   שימו לב: אל תמחקו את appsettings.Production.json שיצרתם!
4. Restart ל-Application Pool ב-IIS.


שאלות? פנו לשרי קרביץ.
===================================================
