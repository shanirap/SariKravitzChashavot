-- Apply InstitutionType column if migration was not run via EF (one-time fix)
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = N'סמלי_מוסד_מעסיקים' AND COLUMN_NAME = N'סוג_מוסד')
BEGIN
    ALTER TABLE [סמלי_מוסד_מעסיקים]
    ADD [סוג_מוסד] nvarchar(20) NOT NULL
        CONSTRAINT DF_EmployerInstitutionSymbol_InstitutionType DEFAULT (N'אחר');
END

IF NOT EXISTS (
    SELECT 1 FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260525120000_AddInstitutionTypeToEmployerInstitutionSymbols')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260525120000_AddInstitutionTypeToEmployerInstitutionSymbols', N'8.0.4');
END
