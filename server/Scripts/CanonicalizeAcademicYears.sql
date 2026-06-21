-- Canonicalize numeric academic-year aliases to Hebrew labels (e.g. 5786 / 2026 -> תשפ"ו).
-- Run after backup. Idempotent for already-canonical values.
-- Values that do not match known numeric aliases are left unchanged for manual review.

DECLARE @Tables TABLE (Name sysname);
INSERT INTO @Tables (Name) VALUES
    (N'נתוני_העסקה'),
    (N'קלט_עוקץ_חודשי_אצווה'),
    (N'קלט_עוקץ_חודשי_שורה'),
    (N'דריסות_דוח_השוואה_שנתי');

DECLARE @Table sysname;
DECLARE table_cursor CURSOR LOCAL FAST_FORWARD FOR SELECT Name FROM @Tables;
OPEN table_cursor;
FETCH NEXT FROM table_cursor INTO @Table;

WHILE @@FETCH_STATUS = 0
BEGIN
    DECLARE @Sql nvarchar(max) = N'
UPDATE [' + @Table + N']
SET [שנת_לימודים] = LTRIM(RTRIM([שנת_לימודים]));

UPDATE [' + @Table + N']
SET [שנת_לימודים] = CASE [שנת_לימודים]
  WHEN N''2000'' THEN N''תשס'''''''
  WHEN N''2001'' THEN N''תשס''''א'''
  WHEN N''2025'' THEN N''תשפ''''ה'''
  WHEN N''2026'' THEN N''תשפ''''ו'''
  WHEN N''5785'' THEN N''תשפ''''ה'''
  WHEN N''5786'' THEN N''תשפ''''ו'''
  ELSE [שנת_לימודים]
END
WHERE [שנת_לימודים] IN (N''2000'', N''2001'', N''2025'', N''2026'', N''5785'', N''5786'');
';
    EXEC sp_executesql @Sql;
    FETCH NEXT FROM table_cursor INTO @Table;
END

CLOSE table_cursor;
DEALLOCATE table_cursor;

-- NOTE: Full numeric range conversion is applied by EF migration CanonicalizeStoredAcademicYears.
-- After running migration.sql, spot-check rows where LEN(RTRIM([שנת_לימודים])) > 0
-- and TryValidateAndCanonicalize would still fail in the application.
