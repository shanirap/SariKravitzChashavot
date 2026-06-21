IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418212408_BaselineStabilityUpgrade'
)
BEGIN
    CREATE TABLE [מעסיקים] (
        [מזהה_מעסיק] int NOT NULL IDENTITY,
        [שם_מעסיק] nvarchar(450) NOT NULL,
        [חפ] nvarchar(450) NULL,
        [מספר_שכר] nvarchar(max) NULL,
        [מספר_עוקץ] nvarchar(max) NULL,
        [CreatedAtUtc] datetimeoffset NOT NULL,
        [UpdatedAtUtc] datetimeoffset NOT NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAtUtc] datetimeoffset NULL,
        CONSTRAINT [PK_מעסיקים] PRIMARY KEY ([מזהה_מעסיק])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418212408_BaselineStabilityUpgrade'
)
BEGIN
    CREATE TABLE [עובדים] (
        [מזהה_עובד] int NOT NULL IDENTITY,
        [מספר_עובד] int NULL,
        [תז] nvarchar(450) NOT NULL,
        [שם_פרטי] nvarchar(max) NULL,
        [שם_משפחה] nvarchar(max) NULL,
        [תאריך_לידה] date NULL,
        [מין] nvarchar(max) NULL,
        [טל] nvarchar(max) NULL,
        [תאריך_לידה_ילד_1] date NULL,
        [תאריך_לידה_ילד_2] date NULL,
        [תאריך_לידה_ילד_3] date NULL,
        [תאריך_לידה_ילד_4] date NULL,
        [תאריך_לידה_ילד_5] date NULL,
        [תאריך_לידה_ילד_6] date NULL,
        [תאריך_לידה_ילד_7] date NULL,
        [תאריך_לידה_ילד_8] date NULL,
        [תאריך_לידה_ילד_9] date NULL,
        [תאריך_לידה_ילד_10] date NULL,
        [CreatedAtUtc] datetimeoffset NOT NULL,
        [UpdatedAtUtc] datetimeoffset NOT NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAtUtc] datetimeoffset NULL,
        CONSTRAINT [PK_עובדים] PRIMARY KEY ([מזהה_עובד])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418212408_BaselineStabilityUpgrade'
)
BEGIN
    CREATE TABLE [AuditLogs] (
        [Id] bigint NOT NULL IDENTITY,
        [EntityName] nvarchar(100) NOT NULL,
        [Action] nvarchar(50) NOT NULL,
        [EntityKey] nvarchar(200) NULL,
        [ChangesJson] nvarchar(max) NULL,
        [ChangedBy] nvarchar(100) NOT NULL,
        [ChangedAtUtc] datetimeoffset NOT NULL,
        CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418212408_BaselineStabilityUpgrade'
)
BEGIN
    CREATE TABLE [נתוני_העסקה] (
        [מזהה_נתון_העסקה] int NOT NULL IDENTITY,
        [מזהה_עובד] int NOT NULL,
        [מזהה_מעסיק] int NOT NULL,
        [חודש_שכר] int NOT NULL,
        [שנת_שכר] int NOT NULL,
        [סמל_מוסד] nvarchar(max) NOT NULL,
        [שם_דרגה] nvarchar(max) NULL,
        [דרגה] nvarchar(max) NULL,
        [תפקיד] nvarchar(max) NULL,
        [ותק] nvarchar(max) NULL,
        [שעות_תקן_1] decimal(18,2) NULL,
        [שעות_בפועל_1] decimal(18,2) NULL,
        [שעות_ד1_2] decimal(18,2) NULL,
        [שעות_תקן_3] decimal(18,2) NULL,
        [שעות_תקן_2] decimal(18,2) NULL,
        [שעות_בפועל_2] decimal(18,2) NULL,
        [שעות_ד2_2] decimal(18,2) NULL,
        [שעות_גמלא_2] decimal(18,2) NULL,
        [קרן_השתלמות_סכום] decimal(18,2) NULL,
        [קרן_השתלמות_אחוז] decimal(18,2) NULL,
        [פנסיה_סכום] decimal(18,2) NULL,
        [סוג_משרה] nvarchar(max) NULL,
        [הכפלה_כללית_באחוז] decimal(18,2) NULL,
        [גמול_חינוך_כיתה] decimal(18,2) NULL,
        [גמול_הכשרה_ומקצוע] decimal(18,2) NULL,
        [כפל_תואר] decimal(18,2) NULL,
        [גמולי_השתלמות] decimal(18,2) NULL,
        [שעות_גיל] decimal(18,2) NULL,
        [אחוז_תוספת_אם] decimal(18,2) NULL,
        [CreatedAtUtc] datetimeoffset NOT NULL,
        [UpdatedAtUtc] datetimeoffset NOT NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAtUtc] datetimeoffset NULL,
        CONSTRAINT [PK_נתוני_העסקה] PRIMARY KEY ([מזהה_נתון_העסקה]),
        CONSTRAINT [FK_נתוני_העסקה_מעסיקים_מזהה_מעסיק] FOREIGN KEY ([מזהה_מעסיק]) REFERENCES [מעסיקים] ([מזהה_מעסיק]) ON DELETE NO ACTION,
        CONSTRAINT [FK_נתוני_העסקה_עובדים_מזהה_עובד] FOREIGN KEY ([מזהה_עובד]) REFERENCES [עובדים] ([מזהה_עובד]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418212408_BaselineStabilityUpgrade'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_מעסיקים_חפ] ON [מעסיקים] ([חפ]) WHERE [חפ] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418212408_BaselineStabilityUpgrade'
)
BEGIN
    CREATE INDEX [IX_מעסיקים_שם_מעסיק] ON [מעסיקים] ([שם_מעסיק]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418212408_BaselineStabilityUpgrade'
)
BEGIN
    CREATE INDEX [IX_נתוני_העסקה_מזהה_מעסיק_שנת_שכר_חודש_שכר] ON [נתוני_העסקה] ([מזהה_מעסיק], [שנת_שכר], [חודש_שכר]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418212408_BaselineStabilityUpgrade'
)
BEGIN
    CREATE INDEX [IX_נתוני_העסקה_מזהה_עובד_מזהה_מעסיק_שנת_שכר_חודש_שכר] ON [נתוני_העסקה] ([מזהה_עובד], [מזהה_מעסיק], [שנת_שכר], [חודש_שכר]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418212408_BaselineStabilityUpgrade'
)
BEGIN
    CREATE INDEX [IX_עובדים_מספר_עובד] ON [עובדים] ([מספר_עובד]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418212408_BaselineStabilityUpgrade'
)
BEGIN
    CREATE UNIQUE INDEX [IX_עובדים_תז] ON [עובדים] ([תז]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418212408_BaselineStabilityUpgrade'
)
BEGIN
    CREATE INDEX [IX_AuditLogs_ChangedAtUtc] ON [AuditLogs] ([ChangedAtUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260418212408_BaselineStabilityUpgrade'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260418212408_BaselineStabilityUpgrade', N'8.0.4');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420150411_RenameEmploymentDataColumnsToExcelFormat'
)
BEGIN
    EXEC sp_rename N'[נתוני_העסקה].[שעות_תקן_3]', N'שעות_משרה_3_דרוג_1', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420150411_RenameEmploymentDataColumnsToExcelFormat'
)
BEGIN
    EXEC sp_rename N'[נתוני_העסקה].[שעות_תקן_2]', N'שעות_משרה_1_דרוג_2', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420150411_RenameEmploymentDataColumnsToExcelFormat'
)
BEGIN
    EXEC sp_rename N'[נתוני_העסקה].[שעות_תקן_1]', N'שעות_משרה_1_דרוג_1', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420150411_RenameEmploymentDataColumnsToExcelFormat'
)
BEGIN
    EXEC sp_rename N'[נתוני_העסקה].[שעות_ד2_2]', N'שעות_משרה_2_דרוג_2', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420150411_RenameEmploymentDataColumnsToExcelFormat'
)
BEGIN
    EXEC sp_rename N'[נתוני_העסקה].[שעות_ד1_2]', N'שעות_משרה_2_דרוג_1', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420150411_RenameEmploymentDataColumnsToExcelFormat'
)
BEGIN
    EXEC sp_rename N'[נתוני_העסקה].[שעות_גמלא_2]', N'שעות_משרה_3_דרוג_2', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420150411_RenameEmploymentDataColumnsToExcelFormat'
)
BEGIN
    EXEC sp_rename N'[נתוני_העסקה].[שעות_בפועל_2]', N'מתוך_שעות_משרה_1_דרוג_2', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420150411_RenameEmploymentDataColumnsToExcelFormat'
)
BEGIN
    EXEC sp_rename N'[נתוני_העסקה].[שעות_בפועל_1]', N'מתוך_שעות_משרה_1_דרוג_1', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420150411_RenameEmploymentDataColumnsToExcelFormat'
)
BEGIN
    EXEC sp_rename N'[נתוני_העסקה].[שנת_שכר]', N'שנה', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420150411_RenameEmploymentDataColumnsToExcelFormat'
)
BEGIN
    EXEC sp_rename N'[נתוני_העסקה].[שם_דרגה]', N'שם_הדירוג', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420150411_RenameEmploymentDataColumnsToExcelFormat'
)
BEGIN
    EXEC sp_rename N'[נתוני_העסקה].[חודש_שכר]', N'חודש', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420150411_RenameEmploymentDataColumnsToExcelFormat'
)
BEGIN
    EXEC sp_rename N'[נתוני_העסקה].[דרגה]', N'דירוג', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420150411_RenameEmploymentDataColumnsToExcelFormat'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260420150411_RenameEmploymentDataColumnsToExcelFormat', N'8.0.4');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420171110_FixAuditLogIdToInt'
)
BEGIN
    ALTER TABLE [AuditLogs] DROP CONSTRAINT [PK_AuditLogs];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420171110_FixAuditLogIdToInt'
)
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AuditLogs]') AND [c].[name] = N'Id');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [AuditLogs] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [AuditLogs] ALTER COLUMN [Id] int NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420171110_FixAuditLogIdToInt'
)
BEGIN
    ALTER TABLE [AuditLogs] ADD CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420171110_FixAuditLogIdToInt'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260420171110_FixAuditLogIdToInt', N'8.0.4');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420223559_FixUniqueIndexExcludeSoftDeleted'
)
BEGIN
    DROP INDEX [IX_מעסיקים_חפ] ON [מעסיקים];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420223559_FixUniqueIndexExcludeSoftDeleted'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_מעסיקים_חפ] ON [מעסיקים] ([חפ]) WHERE [חפ] IS NOT NULL AND [IsDeleted] = 0');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420223559_FixUniqueIndexExcludeSoftDeleted'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260420223559_FixUniqueIndexExcludeSoftDeleted', N'8.0.4');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260425175834_EmploymentDataRestructure'
)
BEGIN

    DECLARE @rid int = OBJECT_ID(N'[נתוני_העסקה]', N'U');
    IF @rid IS NOT NULL
    BEGIN
      DECLARE @dropFk nvarchar(max) = N'';
      SELECT @dropFk = @dropFk + N'ALTER TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(fk.parent_object_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(10)
      FROM sys.foreign_keys fk
      WHERE fk.referenced_object_id = @rid;
      IF (@dropFk <> N'') EXEC sp_executesql @dropFk;
    END
    IF OBJECT_ID(N'[נתוני_העסקה_מקטע]', N'U') IS NOT NULL DROP TABLE [נתוני_העסקה_מקטע];
    IF OBJECT_ID(N'[נתוני_העסקה]', N'U') IS NOT NULL DROP TABLE [נתוני_העסקה];

END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260425175834_EmploymentDataRestructure'
)
BEGIN
    CREATE TABLE [נתוני_העסקה] (
        [מזהה_נתון_העסקה] int NOT NULL IDENTITY,
        [מזהה_עובד] int NOT NULL,
        [מזהה_מעסיק] int NOT NULL,
        [שנת_לימודים] int NOT NULL,
        [דרגה1_סהכ] decimal(18,2) NULL,
        [דרגה1_אחוז_משרה] decimal(18,2) NULL,
        [דרגה1_קרן_השתלמות_אחוז] decimal(18,2) NULL,
        [דרגה1_שעות_גיל] decimal(18,2) NULL,
        [דרגה1_אחוז_הטבה_אם] decimal(18,2) NULL,
        [דרגה2_סהכ] decimal(18,2) NULL,
        [דרגה2_אחוז_משרה] decimal(18,2) NULL,
        [דרגה2_קרן_השתלמות_אחוז] decimal(18,2) NULL,
        [דרגה2_שעות_גיל] decimal(18,2) NULL,
        [דרגה2_אחוז_הטבה_אם] decimal(18,2) NULL,
        [CreatedAtUtc] datetimeoffset NOT NULL,
        [UpdatedAtUtc] datetimeoffset NOT NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAtUtc] datetimeoffset NULL,
        CONSTRAINT [PK_נתוני_העסקה] PRIMARY KEY ([מזהה_נתון_העסקה]),
        CONSTRAINT [FK_נתוני_העסקה_מעסיקים_מזהה_מעסיק] FOREIGN KEY ([מזהה_מעסיק]) REFERENCES [מעסיקים] ([מזהה_מעסיק]) ON DELETE NO ACTION,
        CONSTRAINT [FK_נתוני_העסקה_עובדים_מזהה_עובד] FOREIGN KEY ([מזהה_עובד]) REFERENCES [עובדים] ([מזהה_עובד]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260425175834_EmploymentDataRestructure'
)
BEGIN
    CREATE TABLE [נתוני_העסקה_מקטע] (
        [מזהה_מקטע] int NOT NULL IDENTITY,
        [מזהה_נתון_העסקה] int NOT NULL,
        [רמת_דרגה] tinyint NOT NULL,
        [אינדקס_מקטע] tinyint NOT NULL,
        [שם_הדירוג] nvarchar(max) NULL,
        [דרגה] nvarchar(max) NULL,
        [תפקיד] nvarchar(max) NULL,
        [ותק] nvarchar(max) NULL,
        [סמל_מוסד] nvarchar(max) NULL,
        [שבוע_שעות] decimal(18,2) NULL,
        [בסיס_משרה] decimal(18,2) NULL,
        CONSTRAINT [PK_נתוני_העסקה_מקטע] PRIMARY KEY ([מזהה_מקטע]),
        CONSTRAINT [FK_נתוני_העסקה_מקטע_נתוני_העסקה_מזהה_נתון_העסקה] FOREIGN KEY ([מזהה_נתון_העסקה]) REFERENCES [נתוני_העסקה] ([מזהה_נתון_העסקה]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260425175834_EmploymentDataRestructure'
)
BEGIN
    CREATE INDEX [IX_נתוני_העסקה_מזהה_מעסיק] ON [נתוני_העסקה] ([מזהה_מעסיק]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260425175834_EmploymentDataRestructure'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_נתוני_העסקה_מזהה_עובד_מזהה_מעסיק_שנת_לימודים] ON [נתוני_העסקה] ([מזהה_עובד], [מזהה_מעסיק], [שנת_לימודים]) WHERE [IsDeleted] = 0');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260425175834_EmploymentDataRestructure'
)
BEGIN
    CREATE UNIQUE INDEX [IX_נתוני_העסקה_מקטע_מזהה_נתון_העסקה_רמת_דרגה_אינדקס_מקטע] ON [נתוני_העסקה_מקטע] ([מזהה_נתון_העסקה], [רמת_דרגה], [אינדקס_מקטע]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260425175834_EmploymentDataRestructure'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260425175834_EmploymentDataRestructure', N'8.0.4');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260425184909_RenameEmployerPayrollNumberToBeneficiarySymbol'
)
BEGIN
    EXEC sp_rename N'[מעסיקים].[מספר_שכר]', N'סמל_מוטב', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260425184909_RenameEmployerPayrollNumberToBeneficiarySymbol'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260425184909_RenameEmployerPayrollNumberToBeneficiarySymbol', N'8.0.4');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260425190419_AddEmployerInstitutionSymbols'
)
BEGIN
    CREATE TABLE [סמלי_מוסד_מעסיקים] (
        [מזהה_סמל_מוסד_מעסיק] int NOT NULL IDENTITY,
        [מעסיק] nvarchar(450) NOT NULL,
        [סמל_מוטב] nvarchar(450) NOT NULL,
        [סמל_מוסד] nvarchar(450) NOT NULL,
        [שם_סמל_מוסד] nvarchar(max) NULL,
        CONSTRAINT [PK_סמלי_מוסד_מעסיקים] PRIMARY KEY ([מזהה_סמל_מוסד_מעסיק])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260425190419_AddEmployerInstitutionSymbols'
)
BEGIN
    CREATE UNIQUE INDEX [IX_סמלי_מוסד_מעסיקים_מעסיק_סמל_מוטב_סמל_מוסד] ON [סמלי_מוסד_מעסיקים] ([מעסיק], [סמל_מוטב], [סמל_מוסד]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260425190419_AddEmployerInstitutionSymbols'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260425190419_AddEmployerInstitutionSymbols', N'8.0.4');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260425202720_AddEmployerIdToEmployees'
)
BEGIN

    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_עובדים_תז' AND object_id = OBJECT_ID(N'[עובדים]'))
        DROP INDEX [IX_עובדים_תז] ON [עובדים];

END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260425202720_AddEmployerIdToEmployees'
)
BEGIN
    ALTER TABLE [עובדים] ADD [מזהה_מעסיק] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260425202720_AddEmployerIdToEmployees'
)
BEGIN

    UPDATE e
    SET [מזהה_מעסיק] = x.[מזהה_מעסיק]
    FROM [עובדים] e
    OUTER APPLY (
        SELECT TOP (1) ed.[מזהה_מעסיק]
        FROM [נתוני_העסקה] ed
        WHERE ed.[מזהה_עובד] = e.[מזהה_עובד]
        ORDER BY ed.[UpdatedAtUtc] DESC, ed.[מזהה_נתון_העסקה] DESC
    ) x
    WHERE e.[מזהה_מעסיק] IS NULL AND x.[מזהה_מעסיק] IS NOT NULL;

    UPDATE e
    SET [מזהה_מעסיק] = (SELECT TOP (1) [מזהה_מעסיק] FROM [מעסיקים] ORDER BY [מזהה_מעסיק])
    FROM [עובדים] e
    WHERE e.[מזהה_מעסיק] IS NULL;

    IF EXISTS (SELECT 1 FROM [עובדים] WHERE [מזהה_מעסיק] IS NULL)
        THROW 51000, N'לא ניתן לשייך עובדים קיימים למעסיק כי לא קיים מעסיק במערכת.', 1;

END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260425202720_AddEmployerIdToEmployees'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[עובדים]') AND [c].[name] = N'מזהה_מעסיק');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [עובדים] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [עובדים] ALTER COLUMN [מזהה_מעסיק] int NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260425202720_AddEmployerIdToEmployees'
)
BEGIN
    CREATE UNIQUE INDEX [IX_עובדים_מזהה_מעסיק_תז] ON [עובדים] ([מזהה_מעסיק], [תז]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260425202720_AddEmployerIdToEmployees'
)
BEGIN
    ALTER TABLE [עובדים] ADD CONSTRAINT [FK_עובדים_מעסיקים_מזהה_מעסיק] FOREIGN KEY ([מזהה_מעסיק]) REFERENCES [מעסיקים] ([מזהה_מעסיק]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260425202720_AddEmployerIdToEmployees'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260425202720_AddEmployerIdToEmployees', N'8.0.4');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260425225350_RenameEmploymentHebrewFields'
)
BEGIN
    EXEC sp_rename N'[נתוני_העסקה_מקטע].[שבוע_שעות]', N'שעות_שבועיות', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260425225350_RenameEmploymentHebrewFields'
)
BEGIN
    EXEC sp_rename N'[נתוני_העסקה].[דרגה2_אחוז_הטבה_אם]', N'דרגה2_אחוז_תוספת_אם', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260425225350_RenameEmploymentHebrewFields'
)
BEGIN
    EXEC sp_rename N'[נתוני_העסקה].[דרגה1_אחוז_הטבה_אם]', N'דרגה1_אחוז_תוספת_אם', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260425225350_RenameEmploymentHebrewFields'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260425225350_RenameEmploymentHebrewFields', N'8.0.4');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427213705_EmploymentBandRankFieldsOnHeader'
)
BEGIN
    ALTER TABLE [נתוני_העסקה] ADD [דרגה1_דרגה] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427213705_EmploymentBandRankFieldsOnHeader'
)
BEGIN
    ALTER TABLE [נתוני_העסקה] ADD [דרגה1_ותק] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427213705_EmploymentBandRankFieldsOnHeader'
)
BEGIN
    ALTER TABLE [נתוני_העסקה] ADD [דרגה1_שם_הדירוג] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427213705_EmploymentBandRankFieldsOnHeader'
)
BEGIN
    ALTER TABLE [נתוני_העסקה] ADD [דרגה1_תפקיד] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427213705_EmploymentBandRankFieldsOnHeader'
)
BEGIN
    ALTER TABLE [נתוני_העסקה] ADD [דרגה2_דרגה] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427213705_EmploymentBandRankFieldsOnHeader'
)
BEGIN
    ALTER TABLE [נתוני_העסקה] ADD [דרגה2_ותק] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427213705_EmploymentBandRankFieldsOnHeader'
)
BEGIN
    ALTER TABLE [נתוני_העסקה] ADD [דרגה2_שם_הדירוג] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427213705_EmploymentBandRankFieldsOnHeader'
)
BEGIN
    ALTER TABLE [נתוני_העסקה] ADD [דרגה2_תפקיד] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427213705_EmploymentBandRankFieldsOnHeader'
)
BEGIN

    UPDATE ed SET
      [דרגה1_שם_הדירוג] = s.[שם_הדירוג],
      [דרגה1_דרגה] = s.[דרגה],
      [דרגה1_תפקיד] = s.[תפקיד],
      [דרגה1_ותק] = s.[ותק]
    FROM [נתוני_העסקה] ed
    INNER JOIN [נתוני_העסקה_מקטע] s ON s.[מזהה_נתון_העסקה] = ed.[מזהה_נתון_העסקה] AND s.[רמת_דרגה] = 1 AND s.[אינדקס_מקטע] = 1;

    UPDATE ed SET
      [דרגה2_שם_הדירוג] = s.[שם_הדירוג],
      [דרגה2_דרגה] = s.[דרגה],
      [דרגה2_תפקיד] = s.[תפקיד],
      [דרגה2_ותק] = s.[ותק]
    FROM [נתוני_העסקה] ed
    INNER JOIN [נתוני_העסקה_מקטע] s ON s.[מזהה_נתון_העסקה] = ed.[מזהה_נתון_העסקה] AND s.[רמת_דרגה] = 2 AND s.[אינדקס_מקטע] = 1;

END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427213705_EmploymentBandRankFieldsOnHeader'
)
BEGIN
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[נתוני_העסקה_מקטע]') AND [c].[name] = N'דרגה');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [נתוני_העסקה_מקטע] DROP CONSTRAINT [' + @var2 + '];');
    ALTER TABLE [נתוני_העסקה_מקטע] DROP COLUMN [דרגה];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427213705_EmploymentBandRankFieldsOnHeader'
)
BEGIN
    DECLARE @var3 sysname;
    SELECT @var3 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[נתוני_העסקה_מקטע]') AND [c].[name] = N'ותק');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [נתוני_העסקה_מקטע] DROP CONSTRAINT [' + @var3 + '];');
    ALTER TABLE [נתוני_העסקה_מקטע] DROP COLUMN [ותק];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427213705_EmploymentBandRankFieldsOnHeader'
)
BEGIN
    DECLARE @var4 sysname;
    SELECT @var4 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[נתוני_העסקה_מקטע]') AND [c].[name] = N'שם_הדירוג');
    IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [נתוני_העסקה_מקטע] DROP CONSTRAINT [' + @var4 + '];');
    ALTER TABLE [נתוני_העסקה_מקטע] DROP COLUMN [שם_הדירוג];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427213705_EmploymentBandRankFieldsOnHeader'
)
BEGIN
    DECLARE @var5 sysname;
    SELECT @var5 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[נתוני_העסקה_מקטע]') AND [c].[name] = N'תפקיד');
    IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [נתוני_העסקה_מקטע] DROP CONSTRAINT [' + @var5 + '];');
    ALTER TABLE [נתוני_העסקה_מקטע] DROP COLUMN [תפקיד];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427213705_EmploymentBandRankFieldsOnHeader'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260427213705_EmploymentBandRankFieldsOnHeader', N'8.0.4');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427221425_HebrewAcademicYear'
)
BEGIN
    DROP INDEX [IX_נתוני_העסקה_מזהה_עובד_מזהה_מעסיק_שנת_לימודים] ON [נתוני_העסקה];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427221425_HebrewAcademicYear'
)
BEGIN
    DECLARE @var6 sysname;
    SELECT @var6 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[נתוני_העסקה]') AND [c].[name] = N'שנת_לימודים');
    IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [נתוני_העסקה] DROP CONSTRAINT [' + @var6 + '];');
    ALTER TABLE [נתוני_העסקה] ALTER COLUMN [שנת_לימודים] nvarchar(20) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427221425_HebrewAcademicYear'
)
BEGIN

    UPDATE [נתוני_העסקה]
    SET [שנת_לימודים] = CASE [שנת_לימודים]
      WHEN N'2000' THEN N'תש"ס'
      WHEN N'2001' THEN N'תשס"א'
      WHEN N'2002' THEN N'תשס"ב'
      WHEN N'2003' THEN N'תשס"ג'
      WHEN N'2004' THEN N'תשס"ד'
      WHEN N'2005' THEN N'תשס"ה'
      WHEN N'2006' THEN N'תשס"ו'
      WHEN N'2007' THEN N'תשס"ז'
      WHEN N'2008' THEN N'תשס"ח'
      WHEN N'2009' THEN N'תשס"ט'
      WHEN N'2010' THEN N'תש"ע'
      WHEN N'2011' THEN N'תשע"א'
      WHEN N'2012' THEN N'תשע"ב'
      WHEN N'2013' THEN N'תשע"ג'
      WHEN N'2014' THEN N'תשע"ד'
      WHEN N'2015' THEN N'תשע"ה'
      WHEN N'2016' THEN N'תשע"ו'
      WHEN N'2017' THEN N'תשע"ז'
      WHEN N'2018' THEN N'תשע"ח'
      WHEN N'2019' THEN N'תשע"ט'
      WHEN N'2020' THEN N'תש"פ'
      WHEN N'2021' THEN N'תשפ"א'
      WHEN N'2022' THEN N'תשפ"ב'
      WHEN N'2023' THEN N'תשפ"ג'
      WHEN N'2024' THEN N'תשפ"ד'
      WHEN N'2025' THEN N'תשפ"ה'
      WHEN N'2026' THEN N'תשפ"ו'
      WHEN N'2027' THEN N'תשפ"ז'
      WHEN N'2028' THEN N'תשפ"ח'
      WHEN N'2029' THEN N'תשפ"ט'
      WHEN N'2030' THEN N'תש"צ'
      WHEN N'2031' THEN N'תשצ"א'
      WHEN N'2032' THEN N'תשצ"ב'
      WHEN N'2033' THEN N'תשצ"ג'
      WHEN N'2034' THEN N'תשצ"ד'
      WHEN N'2035' THEN N'תשצ"ה'
      WHEN N'2036' THEN N'תשצ"ו'
      WHEN N'2037' THEN N'תשצ"ז'
      WHEN N'2038' THEN N'תשצ"ח'
      WHEN N'2039' THEN N'תשצ"ט'
      WHEN N'2040' THEN N'ת"ת'
      WHEN N'2041' THEN N'תת"א'
      WHEN N'2042' THEN N'תת"ב'
      WHEN N'2043' THEN N'תת"ג'
      WHEN N'2044' THEN N'תת"ד'
      WHEN N'2045' THEN N'תת"ה'
      WHEN N'2046' THEN N'תת"ו'
      WHEN N'2047' THEN N'תת"ז'
      WHEN N'2048' THEN N'תת"ח'
      WHEN N'2049' THEN N'תת"ט'
      WHEN N'2050' THEN N'תת"י'
      WHEN N'2051' THEN N'תתי"א'
      WHEN N'2052' THEN N'תתי"ב'
      WHEN N'2053' THEN N'תתי"ג'
      WHEN N'2054' THEN N'תתי"ד'
      WHEN N'2055' THEN N'תתט"ו'
      WHEN N'2056' THEN N'תתט"ז'
      WHEN N'2057' THEN N'תתי"ז'
      WHEN N'2058' THEN N'תתי"ח'
      WHEN N'2059' THEN N'תתי"ט'
      WHEN N'2060' THEN N'תת"כ'
      WHEN N'2061' THEN N'תתכ"א'
      WHEN N'2062' THEN N'תתכ"ב'
      WHEN N'2063' THEN N'תתכ"ג'
      WHEN N'2064' THEN N'תתכ"ד'
      WHEN N'2065' THEN N'תתכ"ה'
      WHEN N'2066' THEN N'תתכ"ו'
      WHEN N'2067' THEN N'תתכ"ז'
      WHEN N'2068' THEN N'תתכ"ח'
      WHEN N'2069' THEN N'תתכ"ט'
      WHEN N'2070' THEN N'תת"ל'
      WHEN N'2071' THEN N'תתל"א'
      WHEN N'2072' THEN N'תתל"ב'
      WHEN N'2073' THEN N'תתל"ג'
      WHEN N'2074' THEN N'תתל"ד'
      WHEN N'2075' THEN N'תתל"ה'
      WHEN N'2076' THEN N'תתל"ו'
      WHEN N'2077' THEN N'תתל"ז'
      WHEN N'2078' THEN N'תתל"ח'
      WHEN N'2079' THEN N'תתל"ט'
      WHEN N'2080' THEN N'תת"מ'
      WHEN N'2081' THEN N'תתמ"א'
      WHEN N'2082' THEN N'תתמ"ב'
      WHEN N'2083' THEN N'תתמ"ג'
      WHEN N'2084' THEN N'תתמ"ד'
      WHEN N'2085' THEN N'תתמ"ה'
      WHEN N'2086' THEN N'תתמ"ו'
      WHEN N'2087' THEN N'תתמ"ז'
      WHEN N'2088' THEN N'תתמ"ח'
      WHEN N'2089' THEN N'תתמ"ט'
      WHEN N'2090' THEN N'תת"נ'
      WHEN N'2091' THEN N'תתנ"א'
      WHEN N'2092' THEN N'תתנ"ב'
      WHEN N'2093' THEN N'תתנ"ג'
      WHEN N'2094' THEN N'תתנ"ד'
      WHEN N'2095' THEN N'תתנ"ה'
      WHEN N'2096' THEN N'תתנ"ו'
      WHEN N'2097' THEN N'תתנ"ז'
      WHEN N'2098' THEN N'תתנ"ח'
      WHEN N'2099' THEN N'תתנ"ט'
      WHEN N'2100' THEN N'תת"ס'
      WHEN N'2101' THEN N'תתס"א'
      WHEN N'2102' THEN N'תתס"ב'
      WHEN N'2103' THEN N'תתס"ג'
      WHEN N'2104' THEN N'תתס"ד'
      WHEN N'2105' THEN N'תתס"ה'
      WHEN N'2106' THEN N'תתס"ו'
      WHEN N'2107' THEN N'תתס"ז'
      WHEN N'2108' THEN N'תתס"ח'
      WHEN N'2109' THEN N'תתס"ט'
      WHEN N'2110' THEN N'תת"ע'
      WHEN N'2111' THEN N'תתע"א'
      WHEN N'2112' THEN N'תתע"ב'
      WHEN N'2113' THEN N'תתע"ג'
      WHEN N'2114' THEN N'תתע"ד'
      WHEN N'2115' THEN N'תתע"ה'
      WHEN N'2116' THEN N'תתע"ו'
      WHEN N'2117' THEN N'תתע"ז'
      WHEN N'2118' THEN N'תתע"ח'
      WHEN N'2119' THEN N'תתע"ט'
      WHEN N'2120' THEN N'תת"פ'
      WHEN N'2121' THEN N'תתפ"א'
      WHEN N'2122' THEN N'תתפ"ב'
      WHEN N'2123' THEN N'תתפ"ג'
      WHEN N'2124' THEN N'תתפ"ד'
      WHEN N'2125' THEN N'תתפ"ה'
      WHEN N'2126' THEN N'תתפ"ו'
      WHEN N'2127' THEN N'תתפ"ז'
      WHEN N'2128' THEN N'תתפ"ח'
      WHEN N'2129' THEN N'תתפ"ט'
      WHEN N'2130' THEN N'תת"צ'
      WHEN N'2131' THEN N'תתצ"א'
      WHEN N'2132' THEN N'תתצ"ב'
      WHEN N'2133' THEN N'תתצ"ג'
      WHEN N'2134' THEN N'תתצ"ד'
      WHEN N'2135' THEN N'תתצ"ה'
      WHEN N'2136' THEN N'תתצ"ו'
      WHEN N'2137' THEN N'תתצ"ז'
      WHEN N'2138' THEN N'תתצ"ח'
      WHEN N'2139' THEN N'תתצ"ט'
      WHEN N'2140' THEN N'תת"ק'
      WHEN N'2141' THEN N'תתק"א'
      WHEN N'2142' THEN N'תתק"ב'
      WHEN N'2143' THEN N'תתק"ג'
      WHEN N'2144' THEN N'תתק"ד'
      WHEN N'2145' THEN N'תתק"ה'
      WHEN N'2146' THEN N'תתק"ו'
      WHEN N'2147' THEN N'תתק"ז'
      WHEN N'2148' THEN N'תתק"ח'
      WHEN N'2149' THEN N'תתק"ט'
      WHEN N'2150' THEN N'תתק"י'
      WHEN N'2151' THEN N'תתקי"א'
      WHEN N'2152' THEN N'תתקי"ב'
      WHEN N'2153' THEN N'תתקי"ג'
      WHEN N'2154' THEN N'תתקי"ד'
      WHEN N'2155' THEN N'תתקט"ו'
      WHEN N'2156' THEN N'תתקט"ז'
      WHEN N'2157' THEN N'תתקי"ז'
      WHEN N'2158' THEN N'תתקי"ח'
      WHEN N'2159' THEN N'תתקי"ט'
      WHEN N'2160' THEN N'תתק"כ'
      WHEN N'2161' THEN N'תתקכ"א'
      WHEN N'2162' THEN N'תתקכ"ב'
      WHEN N'2163' THEN N'תתקכ"ג'
      WHEN N'2164' THEN N'תתקכ"ד'
      WHEN N'2165' THEN N'תתקכ"ה'
      WHEN N'2166' THEN N'תתקכ"ו'
      WHEN N'2167' THEN N'תתקכ"ז'
      WHEN N'2168' THEN N'תתקכ"ח'
      WHEN N'2169' THEN N'תתקכ"ט'
      WHEN N'2170' THEN N'תתק"ל'
      WHEN N'2171' THEN N'תתקל"א'
      WHEN N'2172' THEN N'תתקל"ב'
      WHEN N'2173' THEN N'תתקל"ג'
      WHEN N'2174' THEN N'תתקל"ד'
      WHEN N'2175' THEN N'תתקל"ה'
      WHEN N'2176' THEN N'תתקל"ו'
      WHEN N'2177' THEN N'תתקל"ז'
      WHEN N'2178' THEN N'תתקל"ח'
      WHEN N'2179' THEN N'תתקל"ט'
      WHEN N'2180' THEN N'תתק"מ'
      WHEN N'2181' THEN N'תתקמ"א'
      WHEN N'2182' THEN N'תתקמ"ב'
      WHEN N'2183' THEN N'תתקמ"ג'
      WHEN N'2184' THEN N'תתקמ"ד'
      WHEN N'2185' THEN N'תתקמ"ה'
      WHEN N'2186' THEN N'תתקמ"ו'
      WHEN N'2187' THEN N'תתקמ"ז'
      WHEN N'2188' THEN N'תתקמ"ח'
      WHEN N'2189' THEN N'תתקמ"ט'
      WHEN N'2190' THEN N'תתק"נ'
      WHEN N'2191' THEN N'תתקנ"א'
      WHEN N'2192' THEN N'תתקנ"ב'
      WHEN N'2193' THEN N'תתקנ"ג'
      WHEN N'2194' THEN N'תתקנ"ד'
      WHEN N'2195' THEN N'תתקנ"ה'
      WHEN N'2196' THEN N'תתקנ"ו'
      WHEN N'2197' THEN N'תתקנ"ז'
      WHEN N'2198' THEN N'תתקנ"ח'
      WHEN N'2199' THEN N'תתקנ"ט'
      WHEN N'2200' THEN N'תתק"ס'
      ELSE [שנת_לימודים]
    END;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427221425_HebrewAcademicYear'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_נתוני_העסקה_מזהה_עובד_מזהה_מעסיק_שנת_לימודים] ON [נתוני_העסקה] ([מזהה_עובד], [מזהה_מעסיק], [שנת_לימודים]) WHERE [IsDeleted] = 0');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427221425_HebrewAcademicYear'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260427221425_HebrewAcademicYear', N'8.0.4');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429190448_EmployerInstitutionSymbolUseEmployerId'
)
BEGIN
    ALTER TABLE [סמלי_מוסד_מעסיקים] ADD [מזהה_מעסיק] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429190448_EmployerInstitutionSymbolUseEmployerId'
)
BEGIN
    UPDATE s
    SET s.[מזהה_מעסיק] = e.[מזהה_מעסיק]
    FROM [סמלי_מוסד_מעסיקים] s
    INNER JOIN [מעסיקים] e
        ON e.[שם_מעסיק] = s.[מעסיק]
       AND ISNULL(e.[סמל_מוטב], N'') = ISNULL(s.[סמל_מוטב], N'');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429190448_EmployerInstitutionSymbolUseEmployerId'
)
BEGIN
    IF EXISTS (
        SELECT 1
        FROM [סמלי_מוסד_מעסיקים]
        WHERE [מזהה_מעסיק] IS NULL
    )
    BEGIN
        THROW 50001, N'לא ניתן להשלים מעבר ל-EmployerId: קיימים סמלי מוסד ללא התאמה למעסיק.', 1;
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429190448_EmployerInstitutionSymbolUseEmployerId'
)
BEGIN
    DECLARE @var7 sysname;
    SELECT @var7 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[סמלי_מוסד_מעסיקים]') AND [c].[name] = N'מזהה_מעסיק');
    IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [סמלי_מוסד_מעסיקים] DROP CONSTRAINT [' + @var7 + '];');
    ALTER TABLE [סמלי_מוסד_מעסיקים] ALTER COLUMN [מזהה_מעסיק] int NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429190448_EmployerInstitutionSymbolUseEmployerId'
)
BEGIN
    DROP INDEX [IX_סמלי_מוסד_מעסיקים_מעסיק_סמל_מוטב_סמל_מוסד] ON [סמלי_מוסד_מעסיקים];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429190448_EmployerInstitutionSymbolUseEmployerId'
)
BEGIN
    DECLARE @var8 sysname;
    SELECT @var8 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[סמלי_מוסד_מעסיקים]') AND [c].[name] = N'מעסיק');
    IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [סמלי_מוסד_מעסיקים] DROP CONSTRAINT [' + @var8 + '];');
    ALTER TABLE [סמלי_מוסד_מעסיקים] DROP COLUMN [מעסיק];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429190448_EmployerInstitutionSymbolUseEmployerId'
)
BEGIN
    DECLARE @var9 sysname;
    SELECT @var9 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[סמלי_מוסד_מעסיקים]') AND [c].[name] = N'סמל_מוטב');
    IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [סמלי_מוסד_מעסיקים] DROP CONSTRAINT [' + @var9 + '];');
    ALTER TABLE [סמלי_מוסד_מעסיקים] DROP COLUMN [סמל_מוטב];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429190448_EmployerInstitutionSymbolUseEmployerId'
)
BEGIN
    CREATE UNIQUE INDEX [IX_סמלי_מוסד_מעסיקים_מזהה_מעסיק_סמל_מוסד] ON [סמלי_מוסד_מעסיקים] ([מזהה_מעסיק], [סמל_מוסד]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429190448_EmployerInstitutionSymbolUseEmployerId'
)
BEGIN
    ALTER TABLE [סמלי_מוסד_מעסיקים] ADD CONSTRAINT [FK_סמלי_מוסד_מעסיקים_מעסיקים_מזהה_מעסיק] FOREIGN KEY ([מזהה_מעסיק]) REFERENCES [מעסיקים] ([מזהה_מעסיק]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429190448_EmployerInstitutionSymbolUseEmployerId'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260429190448_EmployerInstitutionSymbolUseEmployerId', N'8.0.4');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260430121321_AddEmployerIdToEmployerInstitutionSymbols'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260430121321_AddEmployerIdToEmployerInstitutionSymbols', N'8.0.4');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260430180343_AddEmployeeManualActiveStatus'
)
BEGIN
    ALTER TABLE [עובדים] ADD [סטטוס_פעילות_ידני] bit NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260430180343_AddEmployeeManualActiveStatus'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260430180343_AddEmployeeManualActiveStatus', N'8.0.4');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260504191851_AddUsersTable'
)
BEGIN
    CREATE TABLE [Users] (
        [Id] int NOT NULL IDENTITY,
        [Username] nvarchar(128) NOT NULL,
        [PasswordHash] nvarchar(500) NOT NULL,
        [Role] nvarchar(64) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260504191851_AddUsersTable'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Users_Username] ON [Users] ([Username]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260504191851_AddUsersTable'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260504191851_AddUsersTable', N'8.0.4');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260504192339_AddUsersAuthentication'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260504192339_AddUsersAuthentication', N'8.0.4');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260524220913_AddEmploymentTrainingBenefitsAndDoubleDegree'
)
BEGIN
    ALTER TABLE [נתוני_העסקה] ADD [גמולי_השתלמות] decimal(18,2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260524220913_AddEmploymentTrainingBenefitsAndDoubleDegree'
)
BEGIN
    ALTER TABLE [נתוני_העסקה] ADD [כפל_תואר] decimal(18,2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260524220913_AddEmploymentTrainingBenefitsAndDoubleDegree'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260524220913_AddEmploymentTrainingBenefitsAndDoubleDegree', N'8.0.4');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260524223123_CleanupEmptyEmploymentDataSlots'
)
BEGIN
    IF COL_LENGTH(N'נתוני_העסקה_מקטע', N'מקטע_הורה_שעות_נוספות') IS NOT NULL
    BEGIN
        DELETE FROM [נתוני_העסקה_מקטע]
        WHERE [מקטע_הורה_שעות_נוספות] IS NULL
          AND ([סמל_מוסד] IS NULL OR LTRIM(RTRIM([סמל_מוסד])) = '')
          AND ([שעות_שבועיות] IS NULL OR [שעות_שבועיות] = 0);
    END;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260524223123_CleanupEmptyEmploymentDataSlots'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260524223123_CleanupEmptyEmploymentDataSlots', N'8.0.4');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260525054357_MoveTrainingBenefitsDoubleDegreeToPerGrade'
)
BEGIN
    EXEC sp_rename N'[נתוני_העסקה].[גמולי_השתלמות]', N'דרגה1_גמולי_השתלמות', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260525054357_MoveTrainingBenefitsDoubleDegreeToPerGrade'
)
BEGIN
    EXEC sp_rename N'[נתוני_העסקה].[כפל_תואר]', N'דרגה1_כפל_תואר', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260525054357_MoveTrainingBenefitsDoubleDegreeToPerGrade'
)
BEGIN
    ALTER TABLE [נתוני_העסקה] ADD [דרגה2_גמולי_השתלמות] decimal(18,2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260525054357_MoveTrainingBenefitsDoubleDegreeToPerGrade'
)
BEGIN
    ALTER TABLE [נתוני_העסקה] ADD [דרגה2_כפל_תואר] decimal(18,2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260525054357_MoveTrainingBenefitsDoubleDegreeToPerGrade'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260525054357_MoveTrainingBenefitsDoubleDegreeToPerGrade', N'8.0.4');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260526215142_AddPayrollMonthlyInputs'
)
BEGIN
    CREATE TABLE [קלט_עוקץ_חודשי_אצווה] (
        [מזהה_אצווה] int NOT NULL IDENTITY,
        [מזהה_מעסיק] int NOT NULL,
        [שנת_לימודים] nvarchar(20) NOT NULL,
        [חודש] int NOT NULL,
        [שנה_גרגוריאנית] int NOT NULL,
        [שם_קובץ_מקורי] nvarchar(500) NOT NULL,
        [הועלה_בתאריך] datetime2 NOT NULL,
        [הועלה_על_ידי] nvarchar(200) NULL,
        [מספר_שורות] int NOT NULL,
        [פעיל] bit NOT NULL,
        [נמחק] bit NOT NULL,
        [נמחק_בתאריך] datetime2 NULL,
        [נוצר_בתאריך] datetime2 NOT NULL,
        [עודכן_בתאריך] datetime2 NULL,
        CONSTRAINT [PK_קלט_עוקץ_חודשי_אצווה] PRIMARY KEY ([מזהה_אצווה]),
        CONSTRAINT [FK_קלט_עוקץ_חודשי_אצווה_מעסיקים_מזהה_מעסיק] FOREIGN KEY ([מזהה_מעסיק]) REFERENCES [מעסיקים] ([מזהה_מעסיק]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260526215142_AddPayrollMonthlyInputs'
)
BEGIN
    CREATE TABLE [קלט_עוקץ_חודשי_שורה] (
        [מזהה_שורה] int NOT NULL IDENTITY,
        [מזהה_אצווה] int NOT NULL,
        [מזהה_מעסיק] int NOT NULL,
        [שנת_לימודים] nvarchar(20) NOT NULL,
        [חודש] int NOT NULL,
        [שנה_גרגוריאנית] int NOT NULL,
        [מספר_שורה_באקסל] int NULL,
        [סמל_מוסד] nvarchar(50) NULL,
        [מספר_עובד_בעוקץ] nvarchar(50) NULL,
        [תז] nvarchar(20) NULL,
        [שם_מלא] nvarchar(200) NULL,
        [תפקיד] nvarchar(100) NULL,
        [דרגה] nvarchar(50) NULL,
        [ותק] decimal(18,2) NULL,
        [שעות_שבועיות] decimal(18,2) NULL,
        [בסיס_משרה] decimal(18,2) NULL,
        [אחוז_משרה] decimal(18,2) NULL,
        [שעות_גיל] decimal(18,2) NULL,
        [גמולי_השתלמות] decimal(18,2) NULL,
        [כפל_תואר] decimal(18,2) NULL,
        [קרן_השתלמות] decimal(18,2) NULL,
        [הכפלה_כללית] decimal(18,2) NULL,
        [נערך_ידנית] bit NOT NULL,
        [הערת_עריכה_ידנית] nvarchar(500) NULL,
        [תאים_גולמיים_json] nvarchar(max) NULL,
        [נמחק] bit NOT NULL,
        [נמחק_בתאריך] datetime2 NULL,
        [נוצר_בתאריך] datetime2 NOT NULL,
        [עודכן_בתאריך] datetime2 NULL,
        CONSTRAINT [PK_קלט_עוקץ_חודשי_שורה] PRIMARY KEY ([מזהה_שורה]),
        CONSTRAINT [FK_קלט_עוקץ_חודשי_שורה_קלט_עוקץ_חודשי_אצווה_מזהה_אצווה] FOREIGN KEY ([מזהה_אצווה]) REFERENCES [קלט_עוקץ_חודשי_אצווה] ([מזהה_אצווה]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260526215142_AddPayrollMonthlyInputs'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_קלט_עוקץ_חודשי_אצווה_מזהה_מעסיק_שנת_לימודים_חודש_שנה_גרגוריאנית] ON [קלט_עוקץ_חודשי_אצווה] ([מזהה_מעסיק], [שנת_לימודים], [חודש], [שנה_גרגוריאנית]) WHERE [פעיל] = 1 AND [נמחק] = 0');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260526215142_AddPayrollMonthlyInputs'
)
BEGIN
    CREATE INDEX [IX_קלט_עוקץ_חודשי_שורה_מזהה_אצווה] ON [קלט_עוקץ_חודשי_שורה] ([מזהה_אצווה]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260526215142_AddPayrollMonthlyInputs'
)
BEGIN
    CREATE INDEX [IX_קלט_עוקץ_חודשי_שורה_מזהה_מעסיק_שנת_לימודים_חודש_שנה_גרגוריאנית] ON [קלט_עוקץ_חודשי_שורה] ([מזהה_מעסיק], [שנת_לימודים], [חודש], [שנה_גרגוריאנית]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260526215142_AddPayrollMonthlyInputs'
)
BEGIN
    CREATE INDEX [IX_קלט_עוקץ_חודשי_שורה_מספר_עובד_בעוקץ] ON [קלט_עוקץ_חודשי_שורה] ([מספר_עובד_בעוקץ]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260526215142_AddPayrollMonthlyInputs'
)
BEGIN
    CREATE INDEX [IX_קלט_עוקץ_חודשי_שורה_סמל_מוסד] ON [קלט_עוקץ_חודשי_שורה] ([סמל_מוסד]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260526215142_AddPayrollMonthlyInputs'
)
BEGIN
    CREATE INDEX [IX_קלט_עוקץ_חודשי_שורה_תז] ON [קלט_עוקץ_חודשי_שורה] ([תז]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260526215142_AddPayrollMonthlyInputs'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260526215142_AddPayrollMonthlyInputs', N'8.0.4');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260606221234_AddAnnualComparisonReportRowOverrides'
)
BEGIN
    CREATE TABLE [דריסות_דוח_השוואה_שנתי] (
        [מזהה] int NOT NULL IDENTITY,
        [מזהה_מעסיק] int NOT NULL,
        [שנת_לימודים] nvarchar(20) NOT NULL,
        [מזהה_מקטע] int NOT NULL,
        [סמל_מוסד] nvarchar(50) NULL,
        [שם_מלא] nvarchar(200) NULL,
        [תפקיד] nvarchar(100) NULL,
        [סוג_משרה_מעוקץ] nvarchar(100) NULL,
        [דרגה] nvarchar(50) NULL,
        [ותק] nvarchar(50) NULL,
        [שעות_שבועיות] decimal(18,2) NULL,
        [בסיס_משרה] decimal(18,2) NULL,
        [אחוז_משרה] decimal(18,2) NULL,
        [הכפלה_כללית] decimal(18,2) NULL,
        [תאי_חודש_json] nvarchar(max) NULL,
        [נערך_ידנית] bit NOT NULL,
        [הערת_עריכה] nvarchar(500) NULL,
        [נוצר_בתאריך] datetime2 NOT NULL,
        [עודכן_בתאריך] datetime2 NULL,
        CONSTRAINT [PK_דריסות_דוח_השוואה_שנתי] PRIMARY KEY ([מזהה]),
        CONSTRAINT [FK_דריסות_דוח_השוואה_שנתי_מעסיקים_מזהה_מעסיק] FOREIGN KEY ([מזהה_מעסיק]) REFERENCES [מעסיקים] ([מזהה_מעסיק]) ON DELETE CASCADE,
        CONSTRAINT [FK_דריסות_דוח_השוואה_שנתי_נתוני_העסקה_מקטע_מזהה_מקטע] FOREIGN KEY ([מזהה_מקטע]) REFERENCES [נתוני_העסקה_מקטע] ([מזהה_מקטע]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260606221234_AddAnnualComparisonReportRowOverrides'
)
BEGIN
    CREATE UNIQUE INDEX [IX_דריסות_דוח_השוואה_שנתי_מזהה_מעסיק_שנת_לימודים_מזהה_מקטע] ON [דריסות_דוח_השוואה_שנתי] ([מזהה_מעסיק], [שנת_לימודים], [מזהה_מקטע]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260606221234_AddAnnualComparisonReportRowOverrides'
)
BEGIN
    CREATE INDEX [IX_דריסות_דוח_השוואה_שנתי_מזהה_מקטע] ON [דריסות_דוח_השוואה_שנתי] ([מזהה_מקטע]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260606221234_AddAnnualComparisonReportRowOverrides'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260606221234_AddAnnualComparisonReportRowOverrides', N'8.0.4');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260616232108_NormalizeAllUsersToAdmin'
)
BEGIN
    UPDATE [Users]
    SET [Role] = N'Admin'
    WHERE [Role] <> N'Admin';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260616232108_NormalizeAllUsersToAdmin'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260616232108_NormalizeAllUsersToAdmin', N'8.0.4');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260616232951_CanonicalizeStoredAcademicYears'
)
BEGIN
    UPDATE [נתוני_העסקה]
    SET [שנת_לימודים] = LTRIM(RTRIM([שנת_לימודים]));

    UPDATE [נתוני_העסקה]
    SET [שנת_לימודים] = CASE [שנת_לימודים]
      WHEN N'2000' THEN N'תש"ס'
      WHEN N'2001' THEN N'תשס"א'
      WHEN N'2002' THEN N'תשס"ב'
      WHEN N'2003' THEN N'תשס"ג'
      WHEN N'2004' THEN N'תשס"ד'
      WHEN N'2005' THEN N'תשס"ה'
      WHEN N'2006' THEN N'תשס"ו'
      WHEN N'2007' THEN N'תשס"ז'
      WHEN N'2008' THEN N'תשס"ח'
      WHEN N'2009' THEN N'תשס"ט'
      WHEN N'2010' THEN N'תש"ע'
      WHEN N'2011' THEN N'תשע"א'
      WHEN N'2012' THEN N'תשע"ב'
      WHEN N'2013' THEN N'תשע"ג'
      WHEN N'2014' THEN N'תשע"ד'
      WHEN N'2015' THEN N'תשע"ה'
      WHEN N'2016' THEN N'תשע"ו'
      WHEN N'2017' THEN N'תשע"ז'
      WHEN N'2018' THEN N'תשע"ח'
      WHEN N'2019' THEN N'תשע"ט'
      WHEN N'2020' THEN N'תש"פ'
      WHEN N'2021' THEN N'תשפ"א'
      WHEN N'2022' THEN N'תשפ"ב'
      WHEN N'2023' THEN N'תשפ"ג'
      WHEN N'2024' THEN N'תשפ"ד'
      WHEN N'2025' THEN N'תשפ"ה'
      WHEN N'2026' THEN N'תשפ"ו'
      WHEN N'2027' THEN N'תשפ"ז'
      WHEN N'2028' THEN N'תשפ"ח'
      WHEN N'2029' THEN N'תשפ"ט'
      WHEN N'2030' THEN N'תש"צ'
      WHEN N'2031' THEN N'תשצ"א'
      WHEN N'2032' THEN N'תשצ"ב'
      WHEN N'2033' THEN N'תשצ"ג'
      WHEN N'2034' THEN N'תשצ"ד'
      WHEN N'2035' THEN N'תשצ"ה'
      WHEN N'2036' THEN N'תשצ"ו'
      WHEN N'2037' THEN N'תשצ"ז'
      WHEN N'2038' THEN N'תשצ"ח'
      WHEN N'2039' THEN N'תשצ"ט'
      WHEN N'2040' THEN N'ת"ת'
      WHEN N'2041' THEN N'תת"א'
      WHEN N'2042' THEN N'תת"ב'
      WHEN N'2043' THEN N'תת"ג'
      WHEN N'2044' THEN N'תת"ד'
      WHEN N'2045' THEN N'תת"ה'
      WHEN N'2046' THEN N'תת"ו'
      WHEN N'2047' THEN N'תת"ז'
      WHEN N'2048' THEN N'תת"ח'
      WHEN N'2049' THEN N'תת"ט'
      WHEN N'2050' THEN N'תת"י'
      WHEN N'2051' THEN N'תתי"א'
      WHEN N'2052' THEN N'תתי"ב'
      WHEN N'2053' THEN N'תתי"ג'
      WHEN N'2054' THEN N'תתי"ד'
      WHEN N'2055' THEN N'תתט"ו'
      WHEN N'2056' THEN N'תתט"ז'
      WHEN N'2057' THEN N'תתי"ז'
      WHEN N'2058' THEN N'תתי"ח'
      WHEN N'2059' THEN N'תתי"ט'
      WHEN N'2060' THEN N'תת"כ'
      WHEN N'2061' THEN N'תתכ"א'
      WHEN N'2062' THEN N'תתכ"ב'
      WHEN N'2063' THEN N'תתכ"ג'
      WHEN N'2064' THEN N'תתכ"ד'
      WHEN N'2065' THEN N'תתכ"ה'
      WHEN N'2066' THEN N'תתכ"ו'
      WHEN N'2067' THEN N'תתכ"ז'
      WHEN N'2068' THEN N'תתכ"ח'
      WHEN N'2069' THEN N'תתכ"ט'
      WHEN N'2070' THEN N'תת"ל'
      WHEN N'2071' THEN N'תתל"א'
      WHEN N'2072' THEN N'תתל"ב'
      WHEN N'2073' THEN N'תתל"ג'
      WHEN N'2074' THEN N'תתל"ד'
      WHEN N'2075' THEN N'תתל"ה'
      WHEN N'2076' THEN N'תתל"ו'
      WHEN N'2077' THEN N'תתל"ז'
      WHEN N'2078' THEN N'תתל"ח'
      WHEN N'2079' THEN N'תתל"ט'
      WHEN N'2080' THEN N'תת"מ'
      WHEN N'2081' THEN N'תתמ"א'
      WHEN N'2082' THEN N'תתמ"ב'
      WHEN N'2083' THEN N'תתמ"ג'
      WHEN N'2084' THEN N'תתמ"ד'
      WHEN N'2085' THEN N'תתמ"ה'
      WHEN N'2086' THEN N'תתמ"ו'
      WHEN N'2087' THEN N'תתמ"ז'
      WHEN N'2088' THEN N'תתמ"ח'
      WHEN N'2089' THEN N'תתמ"ט'
      WHEN N'2090' THEN N'תת"נ'
      WHEN N'2091' THEN N'תתנ"א'
      WHEN N'2092' THEN N'תתנ"ב'
      WHEN N'2093' THEN N'תתנ"ג'
      WHEN N'2094' THEN N'תתנ"ד'
      WHEN N'2095' THEN N'תתנ"ה'
      WHEN N'2096' THEN N'תתנ"ו'
      WHEN N'2097' THEN N'תתנ"ז'
      WHEN N'2098' THEN N'תתנ"ח'
      WHEN N'2099' THEN N'תתנ"ט'
      WHEN N'2100' THEN N'תת"ס'
      WHEN N'2101' THEN N'תתס"א'
      WHEN N'2102' THEN N'תתס"ב'
      WHEN N'2103' THEN N'תתס"ג'
      WHEN N'2104' THEN N'תתס"ד'
      WHEN N'2105' THEN N'תתס"ה'
      WHEN N'2106' THEN N'תתס"ו'
      WHEN N'2107' THEN N'תתס"ז'
      WHEN N'2108' THEN N'תתס"ח'
      WHEN N'2109' THEN N'תתס"ט'
      WHEN N'2110' THEN N'תת"ע'
      WHEN N'2111' THEN N'תתע"א'
      WHEN N'2112' THEN N'תתע"ב'
      WHEN N'2113' THEN N'תתע"ג'
      WHEN N'2114' THEN N'תתע"ד'
      WHEN N'2115' THEN N'תתע"ה'
      WHEN N'2116' THEN N'תתע"ו'
      WHEN N'2117' THEN N'תתע"ז'
      WHEN N'2118' THEN N'תתע"ח'
      WHEN N'2119' THEN N'תתע"ט'
      WHEN N'2120' THEN N'תת"פ'
      WHEN N'2121' THEN N'תתפ"א'
      WHEN N'2122' THEN N'תתפ"ב'
      WHEN N'2123' THEN N'תתפ"ג'
      WHEN N'2124' THEN N'תתפ"ד'
      WHEN N'2125' THEN N'תתפ"ה'
      WHEN N'2126' THEN N'תתפ"ו'
      WHEN N'2127' THEN N'תתפ"ז'
      WHEN N'2128' THEN N'תתפ"ח'
      WHEN N'2129' THEN N'תתפ"ט'
      WHEN N'2130' THEN N'תת"צ'
      WHEN N'2131' THEN N'תתצ"א'
      WHEN N'2132' THEN N'תתצ"ב'
      WHEN N'2133' THEN N'תתצ"ג'
      WHEN N'2134' THEN N'תתצ"ד'
      WHEN N'2135' THEN N'תתצ"ה'
      WHEN N'2136' THEN N'תתצ"ו'
      WHEN N'2137' THEN N'תתצ"ז'
      WHEN N'2138' THEN N'תתצ"ח'
      WHEN N'2139' THEN N'תתצ"ט'
      WHEN N'2140' THEN N'תת"ק'
      WHEN N'2141' THEN N'תתק"א'
      WHEN N'2142' THEN N'תתק"ב'
      WHEN N'2143' THEN N'תתק"ג'
      WHEN N'2144' THEN N'תתק"ד'
      WHEN N'2145' THEN N'תתק"ה'
      WHEN N'2146' THEN N'תתק"ו'
      WHEN N'2147' THEN N'תתק"ז'
      WHEN N'2148' THEN N'תתק"ח'
      WHEN N'2149' THEN N'תתק"ט'
      WHEN N'2150' THEN N'תתק"י'
      WHEN N'2151' THEN N'תתקי"א'
      WHEN N'2152' THEN N'תתקי"ב'
      WHEN N'2153' THEN N'תתקי"ג'
      WHEN N'2154' THEN N'תתקי"ד'
      WHEN N'2155' THEN N'תתקט"ו'
      WHEN N'2156' THEN N'תתקט"ז'
      WHEN N'2157' THEN N'תתקי"ז'
      WHEN N'2158' THEN N'תתקי"ח'
      WHEN N'2159' THEN N'תתקי"ט'
      WHEN N'2160' THEN N'תתק"כ'
      WHEN N'2161' THEN N'תתקכ"א'
      WHEN N'2162' THEN N'תתקכ"ב'
      WHEN N'2163' THEN N'תתקכ"ג'
      WHEN N'2164' THEN N'תתקכ"ד'
      WHEN N'2165' THEN N'תתקכ"ה'
      WHEN N'2166' THEN N'תתקכ"ו'
      WHEN N'2167' THEN N'תתקכ"ז'
      WHEN N'2168' THEN N'תתקכ"ח'
      WHEN N'2169' THEN N'תתקכ"ט'
      WHEN N'2170' THEN N'תתק"ל'
      WHEN N'2171' THEN N'תתקל"א'
      WHEN N'2172' THEN N'תתקל"ב'
      WHEN N'2173' THEN N'תתקל"ג'
      WHEN N'2174' THEN N'תתקל"ד'
      WHEN N'2175' THEN N'תתקל"ה'
      WHEN N'2176' THEN N'תתקל"ו'
      WHEN N'2177' THEN N'תתקל"ז'
      WHEN N'2178' THEN N'תתקל"ח'
      WHEN N'2179' THEN N'תתקל"ט'
      WHEN N'2180' THEN N'תתק"מ'
      WHEN N'2181' THEN N'תתקמ"א'
      WHEN N'2182' THEN N'תתקמ"ב'
      WHEN N'2183' THEN N'תתקמ"ג'
      WHEN N'2184' THEN N'תתקמ"ד'
      WHEN N'2185' THEN N'תתקמ"ה'
      WHEN N'2186' THEN N'תתקמ"ו'
      WHEN N'2187' THEN N'תתקמ"ז'
      WHEN N'2188' THEN N'תתקמ"ח'
      WHEN N'2189' THEN N'תתקמ"ט'
      WHEN N'2190' THEN N'תתק"נ'
      WHEN N'2191' THEN N'תתקנ"א'
      WHEN N'2192' THEN N'תתקנ"ב'
      WHEN N'2193' THEN N'תתקנ"ג'
      WHEN N'2194' THEN N'תתקנ"ד'
      WHEN N'2195' THEN N'תתקנ"ה'
      WHEN N'2196' THEN N'תתקנ"ו'
      WHEN N'2197' THEN N'תתקנ"ז'
      WHEN N'2198' THEN N'תתקנ"ח'
      WHEN N'2199' THEN N'תתקנ"ט'
      WHEN N'2200' THEN N'תתק"ס'
      WHEN N'5001' THEN N'א'''
      WHEN N'5002' THEN N'ב'''
      WHEN N'5003' THEN N'ג'''
      WHEN N'5004' THEN N'ד'''
      WHEN N'5005' THEN N'ה'''
      WHEN N'5006' THEN N'ו'''
      WHEN N'5007' THEN N'ז'''
      WHEN N'5008' THEN N'ח'''
      WHEN N'5009' THEN N'ט'''
      WHEN N'5010' THEN N'י'''
      WHEN N'5011' THEN N'י"א'
      WHEN N'5012' THEN N'י"ב'
      WHEN N'5013' THEN N'י"ג'
      WHEN N'5014' THEN N'י"ד'
      WHEN N'5015' THEN N'ט"ו'
      WHEN N'5016' THEN N'ט"ז'
      WHEN N'5017' THEN N'י"ז'
      WHEN N'5018' THEN N'י"ח'
      WHEN N'5019' THEN N'י"ט'
      WHEN N'5020' THEN N'כ'''
      WHEN N'5021' THEN N'כ"א'
      WHEN N'5022' THEN N'כ"ב'
      WHEN N'5023' THEN N'כ"ג'
      WHEN N'5024' THEN N'כ"ד'
      WHEN N'5025' THEN N'כ"ה'
      WHEN N'5026' THEN N'כ"ו'
      WHEN N'5027' THEN N'כ"ז'
      WHEN N'5028' THEN N'כ"ח'
      WHEN N'5029' THEN N'כ"ט'
      WHEN N'5030' THEN N'ל'''
      WHEN N'5031' THEN N'ל"א'
      WHEN N'5032' THEN N'ל"ב'
      WHEN N'5033' THEN N'ל"ג'
      WHEN N'5034' THEN N'ל"ד'
      WHEN N'5035' THEN N'ל"ה'
      WHEN N'5036' THEN N'ל"ו'
      WHEN N'5037' THEN N'ל"ז'
      WHEN N'5038' THEN N'ל"ח'
      WHEN N'5039' THEN N'ל"ט'
      WHEN N'5040' THEN N'מ'''
      WHEN N'5041' THEN N'מ"א'
      WHEN N'5042' THEN N'מ"ב'
      WHEN N'5043' THEN N'מ"ג'
      WHEN N'5044' THEN N'מ"ד'
      WHEN N'5045' THEN N'מ"ה'
      WHEN N'5046' THEN N'מ"ו'
      WHEN N'5047' THEN N'מ"ז'
      WHEN N'5048' THEN N'מ"ח'
      WHEN N'5049' THEN N'מ"ט'
      WHEN N'5050' THEN N'נ'''
      WHEN N'5051' THEN N'נ"א'
      WHEN N'5052' THEN N'נ"ב'
      WHEN N'5053' THEN N'נ"ג'
      WHEN N'5054' THEN N'נ"ד'
      WHEN N'5055' THEN N'נ"ה'
      WHEN N'5056' THEN N'נ"ו'
      WHEN N'5057' THEN N'נ"ז'
      WHEN N'5058' THEN N'נ"ח'
      WHEN N'5059' THEN N'נ"ט'
      WHEN N'5060' THEN N'ס'''
      WHEN N'5061' THEN N'ס"א'
      WHEN N'5062' THEN N'ס"ב'
      WHEN N'5063' THEN N'ס"ג'
      WHEN N'5064' THEN N'ס"ד'
      WHEN N'5065' THEN N'ס"ה'
      WHEN N'5066' THEN N'ס"ו'
      WHEN N'5067' THEN N'ס"ז'
      WHEN N'5068' THEN N'ס"ח'
      WHEN N'5069' THEN N'ס"ט'
      WHEN N'5070' THEN N'ע'''
      WHEN N'5071' THEN N'ע"א'
      WHEN N'5072' THEN N'ע"ב'
      WHEN N'5073' THEN N'ע"ג'
      WHEN N'5074' THEN N'ע"ד'
      WHEN N'5075' THEN N'ע"ה'
      WHEN N'5076' THEN N'ע"ו'
      WHEN N'5077' THEN N'ע"ז'
      WHEN N'5078' THEN N'ע"ח'
      WHEN N'5079' THEN N'ע"ט'
      WHEN N'5080' THEN N'פ'''
      WHEN N'5081' THEN N'פ"א'
      WHEN N'5082' THEN N'פ"ב'
      WHEN N'5083' THEN N'פ"ג'
      WHEN N'5084' THEN N'פ"ד'
      WHEN N'5085' THEN N'פ"ה'
      WHEN N'5086' THEN N'פ"ו'
      WHEN N'5087' THEN N'פ"ז'
      WHEN N'5088' THEN N'פ"ח'
      WHEN N'5089' THEN N'פ"ט'
      WHEN N'5090' THEN N'צ'''
      WHEN N'5091' THEN N'צ"א'
      WHEN N'5092' THEN N'צ"ב'
      WHEN N'5093' THEN N'צ"ג'
      WHEN N'5094' THEN N'צ"ד'
      WHEN N'5095' THEN N'צ"ה'
      WHEN N'5096' THEN N'צ"ו'
      WHEN N'5097' THEN N'צ"ז'
      WHEN N'5098' THEN N'צ"ח'
      WHEN N'5099' THEN N'צ"ט'
      WHEN N'5100' THEN N'ק'''
      WHEN N'5101' THEN N'ק"א'
      WHEN N'5102' THEN N'ק"ב'
      WHEN N'5103' THEN N'ק"ג'
      WHEN N'5104' THEN N'ק"ד'
      WHEN N'5105' THEN N'ק"ה'
      WHEN N'5106' THEN N'ק"ו'
      WHEN N'5107' THEN N'ק"ז'
      WHEN N'5108' THEN N'ק"ח'
      WHEN N'5109' THEN N'ק"ט'
      WHEN N'5110' THEN N'ק"י'
      WHEN N'5111' THEN N'קי"א'
      WHEN N'5112' THEN N'קי"ב'
      WHEN N'5113' THEN N'קי"ג'
      WHEN N'5114' THEN N'קי"ד'
      WHEN N'5115' THEN N'קט"ו'
      WHEN N'5116' THEN N'קט"ז'
      WHEN N'5117' THEN N'קי"ז'
      WHEN N'5118' THEN N'קי"ח'
      WHEN N'5119' THEN N'קי"ט'
      WHEN N'5120' THEN N'ק"כ'
      WHEN N'5121' THEN N'קכ"א'
      WHEN N'5122' THEN N'קכ"ב'
      WHEN N'5123' THEN N'קכ"ג'
      WHEN N'5124' THEN N'קכ"ד'
      WHEN N'5125' THEN N'קכ"ה'
      WHEN N'5126' THEN N'קכ"ו'
      WHEN N'5127' THEN N'קכ"ז'
      WHEN N'5128' THEN N'קכ"ח'
      WHEN N'5129' THEN N'קכ"ט'
      WHEN N'5130' THEN N'ק"ל'
      WHEN N'5131' THEN N'קל"א'
      WHEN N'5132' THEN N'קל"ב'
      WHEN N'5133' THEN N'קל"ג'
      WHEN N'5134' THEN N'קל"ד'
      WHEN N'5135' THEN N'קל"ה'
      WHEN N'5136' THEN N'קל"ו'
      WHEN N'5137' THEN N'קל"ז'
      WHEN N'5138' THEN N'קל"ח'
      WHEN N'5139' THEN N'קל"ט'
      WHEN N'5140' THEN N'ק"מ'
      WHEN N'5141' THEN N'קמ"א'
      WHEN N'5142' THEN N'קמ"ב'
      WHEN N'5143' THEN N'קמ"ג'
      WHEN N'5144' THEN N'קמ"ד'
      WHEN N'5145' THEN N'קמ"ה'
      WHEN N'5146' THEN N'קמ"ו'
      WHEN N'5147' THEN N'קמ"ז'
      WHEN N'5148' THEN N'קמ"ח'
      WHEN N'5149' THEN N'קמ"ט'
      WHEN N'5150' THEN N'ק"נ'
      WHEN N'5151' THEN N'קנ"א'
      WHEN N'5152' THEN N'קנ"ב'
      WHEN N'5153' THEN N'קנ"ג'
      WHEN N'5154' THEN N'קנ"ד'
      WHEN N'5155' THEN N'קנ"ה'
      WHEN N'5156' THEN N'קנ"ו'
      WHEN N'5157' THEN N'קנ"ז'
      WHEN N'5158' THEN N'קנ"ח'
      WHEN N'5159' THEN N'קנ"ט'
      WHEN N'5160' THEN N'ק"ס'
      WHEN N'5161' THEN N'קס"א'
      WHEN N'5162' THEN N'קס"ב'
      WHEN N'5163' THEN N'קס"ג'
      WHEN N'5164' THEN N'קס"ד'
      WHEN N'5165' THEN N'קס"ה'
      WHEN N'5166' THEN N'קס"ו'
      WHEN N'5167' THEN N'קס"ז'
      WHEN N'5168' THEN N'קס"ח'
      WHEN N'5169' THEN N'קס"ט'
      WHEN N'5170' THEN N'ק"ע'
      WHEN N'5171' THEN N'קע"א'
      WHEN N'5172' THEN N'קע"ב'
      WHEN N'5173' THEN N'קע"ג'
      WHEN N'5174' THEN N'קע"ד'
      WHEN N'5175' THEN N'קע"ה'
      WHEN N'5176' THEN N'קע"ו'
      WHEN N'5177' THEN N'קע"ז'
      WHEN N'5178' THEN N'קע"ח'
      WHEN N'5179' THEN N'קע"ט'
      WHEN N'5180' THEN N'ק"פ'
      WHEN N'5181' THEN N'קפ"א'
      WHEN N'5182' THEN N'קפ"ב'
      WHEN N'5183' THEN N'קפ"ג'
      WHEN N'5184' THEN N'קפ"ד'
      WHEN N'5185' THEN N'קפ"ה'
      WHEN N'5186' THEN N'קפ"ו'
      WHEN N'5187' THEN N'קפ"ז'
      WHEN N'5188' THEN N'קפ"ח'
      WHEN N'5189' THEN N'קפ"ט'
      WHEN N'5190' THEN N'ק"צ'
      WHEN N'5191' THEN N'קצ"א'
      WHEN N'5192' THEN N'קצ"ב'
      WHEN N'5193' THEN N'קצ"ג'
      WHEN N'5194' THEN N'קצ"ד'
      WHEN N'5195' THEN N'קצ"ה'
      WHEN N'5196' THEN N'קצ"ו'
      WHEN N'5197' THEN N'קצ"ז'
      WHEN N'5198' THEN N'קצ"ח'
      WHEN N'5199' THEN N'קצ"ט'
      WHEN N'5200' THEN N'ר'''
      WHEN N'5201' THEN N'ר"א'
      WHEN N'5202' THEN N'ר"ב'
      WHEN N'5203' THEN N'ר"ג'
      WHEN N'5204' THEN N'ר"ד'
      WHEN N'5205' THEN N'ר"ה'
      WHEN N'5206' THEN N'ר"ו'
      WHEN N'5207' THEN N'ר"ז'
      WHEN N'5208' THEN N'ר"ח'
      WHEN N'5209' THEN N'ר"ט'
      WHEN N'5210' THEN N'ר"י'
      WHEN N'5211' THEN N'רי"א'
      WHEN N'5212' THEN N'רי"ב'
      WHEN N'5213' THEN N'רי"ג'
      WHEN N'5214' THEN N'רי"ד'
      WHEN N'5215' THEN N'רט"ו'
      WHEN N'5216' THEN N'רט"ז'
      WHEN N'5217' THEN N'רי"ז'
      WHEN N'5218' THEN N'רי"ח'
      WHEN N'5219' THEN N'רי"ט'
      WHEN N'5220' THEN N'ר"כ'
      WHEN N'5221' THEN N'רכ"א'
      WHEN N'5222' THEN N'רכ"ב'
      WHEN N'5223' THEN N'רכ"ג'
      WHEN N'5224' THEN N'רכ"ד'
      WHEN N'5225' THEN N'רכ"ה'
      WHEN N'5226' THEN N'רכ"ו'
      WHEN N'5227' THEN N'רכ"ז'
      WHEN N'5228' THEN N'רכ"ח'
      WHEN N'5229' THEN N'רכ"ט'
      WHEN N'5230' THEN N'ר"ל'
      WHEN N'5231' THEN N'רל"א'
      WHEN N'5232' THEN N'רל"ב'
      WHEN N'5233' THEN N'רל"ג'
      WHEN N'5234' THEN N'רל"ד'
      WHEN N'5235' THEN N'רל"ה'
      WHEN N'5236' THEN N'רל"ו'
      WHEN N'5237' THEN N'רל"ז'
      WHEN N'5238' THEN N'רל"ח'
      WHEN N'5239' THEN N'רל"ט'
      WHEN N'5240' THEN N'ר"מ'
      WHEN N'5241' THEN N'רמ"א'
      WHEN N'5242' THEN N'רמ"ב'
      WHEN N'5243' THEN N'רמ"ג'
      WHEN N'5244' THEN N'רמ"ד'
      WHEN N'5245' THEN N'רמ"ה'
      WHEN N'5246' THEN N'רמ"ו'
      WHEN N'5247' THEN N'רמ"ז'
      WHEN N'5248' THEN N'רמ"ח'
      WHEN N'5249' THEN N'רמ"ט'
      WHEN N'5250' THEN N'ר"נ'
      WHEN N'5251' THEN N'רנ"א'
      WHEN N'5252' THEN N'רנ"ב'
      WHEN N'5253' THEN N'רנ"ג'
      WHEN N'5254' THEN N'רנ"ד'
      WHEN N'5255' THEN N'רנ"ה'
      WHEN N'5256' THEN N'רנ"ו'
      WHEN N'5257' THEN N'רנ"ז'
      WHEN N'5258' THEN N'רנ"ח'
      WHEN N'5259' THEN N'רנ"ט'
      WHEN N'5260' THEN N'ר"ס'
      WHEN N'5261' THEN N'רס"א'
      WHEN N'5262' THEN N'רס"ב'
      WHEN N'5263' THEN N'רס"ג'
      WHEN N'5264' THEN N'רס"ד'
      WHEN N'5265' THEN N'רס"ה'
      WHEN N'5266' THEN N'רס"ו'
      WHEN N'5267' THEN N'רס"ז'
      WHEN N'5268' THEN N'רס"ח'
      WHEN N'5269' THEN N'רס"ט'
      WHEN N'5270' THEN N'ר"ע'
      WHEN N'5271' THEN N'רע"א'
      WHEN N'5272' THEN N'רע"ב'
      WHEN N'5273' THEN N'רע"ג'
      WHEN N'5274' THEN N'רע"ד'
      WHEN N'5275' THEN N'רע"ה'
      WHEN N'5276' THEN N'רע"ו'
      WHEN N'5277' THEN N'רע"ז'
      WHEN N'5278' THEN N'רע"ח'
      WHEN N'5279' THEN N'רע"ט'
      WHEN N'5280' THEN N'ר"פ'
      WHEN N'5281' THEN N'רפ"א'
      WHEN N'5282' THEN N'רפ"ב'
      WHEN N'5283' THEN N'רפ"ג'
      WHEN N'5284' THEN N'רפ"ד'
      WHEN N'5285' THEN N'רפ"ה'
      WHEN N'5286' THEN N'רפ"ו'
      WHEN N'5287' THEN N'רפ"ז'
      WHEN N'5288' THEN N'רפ"ח'
      WHEN N'5289' THEN N'רפ"ט'
      WHEN N'5290' THEN N'ר"צ'
      WHEN N'5291' THEN N'רצ"א'
      WHEN N'5292' THEN N'רצ"ב'
      WHEN N'5293' THEN N'רצ"ג'
      WHEN N'5294' THEN N'רצ"ד'
      WHEN N'5295' THEN N'רצ"ה'
      WHEN N'5296' THEN N'רצ"ו'
      WHEN N'5297' THEN N'רצ"ז'
      WHEN N'5298' THEN N'רצ"ח'
      WHEN N'5299' THEN N'רצ"ט'
      WHEN N'5300' THEN N'ש'''
      WHEN N'5301' THEN N'ש"א'
      WHEN N'5302' THEN N'ש"ב'
      WHEN N'5303' THEN N'ש"ג'
      WHEN N'5304' THEN N'ש"ד'
      WHEN N'5305' THEN N'ש"ה'
      WHEN N'5306' THEN N'ש"ו'
      WHEN N'5307' THEN N'ש"ז'
      WHEN N'5308' THEN N'ש"ח'
      WHEN N'5309' THEN N'ש"ט'
      WHEN N'5310' THEN N'ש"י'
      WHEN N'5311' THEN N'שי"א'
      WHEN N'5312' THEN N'שי"ב'
      WHEN N'5313' THEN N'שי"ג'
      WHEN N'5314' THEN N'שי"ד'
      WHEN N'5315' THEN N'שט"ו'
      WHEN N'5316' THEN N'שט"ז'
      WHEN N'5317' THEN N'שי"ז'
      WHEN N'5318' THEN N'שי"ח'
      WHEN N'5319' THEN N'שי"ט'
      WHEN N'5320' THEN N'ש"כ'
      WHEN N'5321' THEN N'שכ"א'
      WHEN N'5322' THEN N'שכ"ב'
      WHEN N'5323' THEN N'שכ"ג'
      WHEN N'5324' THEN N'שכ"ד'
      WHEN N'5325' THEN N'שכ"ה'
      WHEN N'5326' THEN N'שכ"ו'
      WHEN N'5327' THEN N'שכ"ז'
      WHEN N'5328' THEN N'שכ"ח'
      WHEN N'5329' THEN N'שכ"ט'
      WHEN N'5330' THEN N'ש"ל'
      WHEN N'5331' THEN N'של"א'
      WHEN N'5332' THEN N'של"ב'
      WHEN N'5333' THEN N'של"ג'
      WHEN N'5334' THEN N'של"ד'
      WHEN N'5335' THEN N'של"ה'
      WHEN N'5336' THEN N'של"ו'
      WHEN N'5337' THEN N'של"ז'
      WHEN N'5338' THEN N'של"ח'
      WHEN N'5339' THEN N'של"ט'
      WHEN N'5340' THEN N'ש"מ'
      WHEN N'5341' THEN N'שמ"א'
      WHEN N'5342' THEN N'שמ"ב'
      WHEN N'5343' THEN N'שמ"ג'
      WHEN N'5344' THEN N'שמ"ד'
      WHEN N'5345' THEN N'שמ"ה'
      WHEN N'5346' THEN N'שמ"ו'
      WHEN N'5347' THEN N'שמ"ז'
      WHEN N'5348' THEN N'שמ"ח'
      WHEN N'5349' THEN N'שמ"ט'
      WHEN N'5350' THEN N'ש"נ'
      WHEN N'5351' THEN N'שנ"א'
      WHEN N'5352' THEN N'שנ"ב'
      WHEN N'5353' THEN N'שנ"ג'
      WHEN N'5354' THEN N'שנ"ד'
      WHEN N'5355' THEN N'שנ"ה'
      WHEN N'5356' THEN N'שנ"ו'
      WHEN N'5357' THEN N'שנ"ז'
      WHEN N'5358' THEN N'שנ"ח'
      WHEN N'5359' THEN N'שנ"ט'
      WHEN N'5360' THEN N'ש"ס'
      WHEN N'5361' THEN N'שס"א'
      WHEN N'5362' THEN N'שס"ב'
      WHEN N'5363' THEN N'שס"ג'
      WHEN N'5364' THEN N'שס"ד'
      WHEN N'5365' THEN N'שס"ה'
      WHEN N'5366' THEN N'שס"ו'
      WHEN N'5367' THEN N'שס"ז'
      WHEN N'5368' THEN N'שס"ח'
      WHEN N'5369' THEN N'שס"ט'
      WHEN N'5370' THEN N'ש"ע'
      WHEN N'5371' THEN N'שע"א'
      WHEN N'5372' THEN N'שע"ב'
      WHEN N'5373' THEN N'שע"ג'
      WHEN N'5374' THEN N'שע"ד'
      WHEN N'5375' THEN N'שע"ה'
      WHEN N'5376' THEN N'שע"ו'
      WHEN N'5377' THEN N'שע"ז'
      WHEN N'5378' THEN N'שע"ח'
      WHEN N'5379' THEN N'שע"ט'
      WHEN N'5380' THEN N'ש"פ'
      WHEN N'5381' THEN N'שפ"א'
      WHEN N'5382' THEN N'שפ"ב'
      WHEN N'5383' THEN N'שפ"ג'
      WHEN N'5384' THEN N'שפ"ד'
      WHEN N'5385' THEN N'שפ"ה'
      WHEN N'5386' THEN N'שפ"ו'
      WHEN N'5387' THEN N'שפ"ז'
      WHEN N'5388' THEN N'שפ"ח'
      WHEN N'5389' THEN N'שפ"ט'
      WHEN N'5390' THEN N'ש"צ'
      WHEN N'5391' THEN N'שצ"א'
      WHEN N'5392' THEN N'שצ"ב'
      WHEN N'5393' THEN N'שצ"ג'
      WHEN N'5394' THEN N'שצ"ד'
      WHEN N'5395' THEN N'שצ"ה'
      WHEN N'5396' THEN N'שצ"ו'
      WHEN N'5397' THEN N'שצ"ז'
      WHEN N'5398' THEN N'שצ"ח'
      WHEN N'5399' THEN N'שצ"ט'
      WHEN N'5400' THEN N'ת'''
      WHEN N'5401' THEN N'ת"א'
      WHEN N'5402' THEN N'ת"ב'
      WHEN N'5403' THEN N'ת"ג'
      WHEN N'5404' THEN N'ת"ד'
      WHEN N'5405' THEN N'ת"ה'
      WHEN N'5406' THEN N'ת"ו'
      WHEN N'5407' THEN N'ת"ז'
      WHEN N'5408' THEN N'ת"ח'
      WHEN N'5409' THEN N'ת"ט'
      WHEN N'5410' THEN N'ת"י'
      WHEN N'5411' THEN N'תי"א'
      WHEN N'5412' THEN N'תי"ב'
      WHEN N'5413' THEN N'תי"ג'
      WHEN N'5414' THEN N'תי"ד'
      WHEN N'5415' THEN N'תט"ו'
      WHEN N'5416' THEN N'תט"ז'
      WHEN N'5417' THEN N'תי"ז'
      WHEN N'5418' THEN N'תי"ח'
      WHEN N'5419' THEN N'תי"ט'
      WHEN N'5420' THEN N'ת"כ'
      WHEN N'5421' THEN N'תכ"א'
      WHEN N'5422' THEN N'תכ"ב'
      WHEN N'5423' THEN N'תכ"ג'
      WHEN N'5424' THEN N'תכ"ד'
      WHEN N'5425' THEN N'תכ"ה'
      WHEN N'5426' THEN N'תכ"ו'
      WHEN N'5427' THEN N'תכ"ז'
      WHEN N'5428' THEN N'תכ"ח'
      WHEN N'5429' THEN N'תכ"ט'
      WHEN N'5430' THEN N'ת"ל'
      WHEN N'5431' THEN N'תל"א'
      WHEN N'5432' THEN N'תל"ב'
      WHEN N'5433' THEN N'תל"ג'
      WHEN N'5434' THEN N'תל"ד'
      WHEN N'5435' THEN N'תל"ה'
      WHEN N'5436' THEN N'תל"ו'
      WHEN N'5437' THEN N'תל"ז'
      WHEN N'5438' THEN N'תל"ח'
      WHEN N'5439' THEN N'תל"ט'
      WHEN N'5440' THEN N'ת"מ'
      WHEN N'5441' THEN N'תמ"א'
      WHEN N'5442' THEN N'תמ"ב'
      WHEN N'5443' THEN N'תמ"ג'
      WHEN N'5444' THEN N'תמ"ד'
      WHEN N'5445' THEN N'תמ"ה'
      WHEN N'5446' THEN N'תמ"ו'
      WHEN N'5447' THEN N'תמ"ז'
      WHEN N'5448' THEN N'תמ"ח'
      WHEN N'5449' THEN N'תמ"ט'
      WHEN N'5450' THEN N'ת"נ'
      WHEN N'5451' THEN N'תנ"א'
      WHEN N'5452' THEN N'תנ"ב'
      WHEN N'5453' THEN N'תנ"ג'
      WHEN N'5454' THEN N'תנ"ד'
      WHEN N'5455' THEN N'תנ"ה'
      WHEN N'5456' THEN N'תנ"ו'
      WHEN N'5457' THEN N'תנ"ז'
      WHEN N'5458' THEN N'תנ"ח'
      WHEN N'5459' THEN N'תנ"ט'
      WHEN N'5460' THEN N'ת"ס'
      WHEN N'5461' THEN N'תס"א'
      WHEN N'5462' THEN N'תס"ב'
      WHEN N'5463' THEN N'תס"ג'
      WHEN N'5464' THEN N'תס"ד'
      WHEN N'5465' THEN N'תס"ה'
      WHEN N'5466' THEN N'תס"ו'
      WHEN N'5467' THEN N'תס"ז'
      WHEN N'5468' THEN N'תס"ח'
      WHEN N'5469' THEN N'תס"ט'
      WHEN N'5470' THEN N'ת"ע'
      WHEN N'5471' THEN N'תע"א'
      WHEN N'5472' THEN N'תע"ב'
      WHEN N'5473' THEN N'תע"ג'
      WHEN N'5474' THEN N'תע"ד'
      WHEN N'5475' THEN N'תע"ה'
      WHEN N'5476' THEN N'תע"ו'
      WHEN N'5477' THEN N'תע"ז'
      WHEN N'5478' THEN N'תע"ח'
      WHEN N'5479' THEN N'תע"ט'
      WHEN N'5480' THEN N'ת"פ'
      WHEN N'5481' THEN N'תפ"א'
      WHEN N'5482' THEN N'תפ"ב'
      WHEN N'5483' THEN N'תפ"ג'
      WHEN N'5484' THEN N'תפ"ד'
      WHEN N'5485' THEN N'תפ"ה'
      WHEN N'5486' THEN N'תפ"ו'
      WHEN N'5487' THEN N'תפ"ז'
      WHEN N'5488' THEN N'תפ"ח'
      WHEN N'5489' THEN N'תפ"ט'
      WHEN N'5490' THEN N'ת"צ'
      WHEN N'5491' THEN N'תצ"א'
      WHEN N'5492' THEN N'תצ"ב'
      WHEN N'5493' THEN N'תצ"ג'
      WHEN N'5494' THEN N'תצ"ד'
      WHEN N'5495' THEN N'תצ"ה'
      WHEN N'5496' THEN N'תצ"ו'
      WHEN N'5497' THEN N'תצ"ז'
      WHEN N'5498' THEN N'תצ"ח'
      WHEN N'5499' THEN N'תצ"ט'
      WHEN N'5500' THEN N'ת"ק'
      WHEN N'5501' THEN N'תק"א'
      WHEN N'5502' THEN N'תק"ב'
      WHEN N'5503' THEN N'תק"ג'
      WHEN N'5504' THEN N'תק"ד'
      WHEN N'5505' THEN N'תק"ה'
      WHEN N'5506' THEN N'תק"ו'
      WHEN N'5507' THEN N'תק"ז'
      WHEN N'5508' THEN N'תק"ח'
      WHEN N'5509' THEN N'תק"ט'
      WHEN N'5510' THEN N'תק"י'
      WHEN N'5511' THEN N'תקי"א'
      WHEN N'5512' THEN N'תקי"ב'
      WHEN N'5513' THEN N'תקי"ג'
      WHEN N'5514' THEN N'תקי"ד'
      WHEN N'5515' THEN N'תקט"ו'
      WHEN N'5516' THEN N'תקט"ז'
      WHEN N'5517' THEN N'תקי"ז'
      WHEN N'5518' THEN N'תקי"ח'
      WHEN N'5519' THEN N'תקי"ט'
      WHEN N'5520' THEN N'תק"כ'
      WHEN N'5521' THEN N'תקכ"א'
      WHEN N'5522' THEN N'תקכ"ב'
      WHEN N'5523' THEN N'תקכ"ג'
      WHEN N'5524' THEN N'תקכ"ד'
      WHEN N'5525' THEN N'תקכ"ה'
      WHEN N'5526' THEN N'תקכ"ו'
      WHEN N'5527' THEN N'תקכ"ז'
      WHEN N'5528' THEN N'תקכ"ח'
      WHEN N'5529' THEN N'תקכ"ט'
      WHEN N'5530' THEN N'תק"ל'
      WHEN N'5531' THEN N'תקל"א'
      WHEN N'5532' THEN N'תקל"ב'
      WHEN N'5533' THEN N'תקל"ג'
      WHEN N'5534' THEN N'תקל"ד'
      WHEN N'5535' THEN N'תקל"ה'
      WHEN N'5536' THEN N'תקל"ו'
      WHEN N'5537' THEN N'תקל"ז'
      WHEN N'5538' THEN N'תקל"ח'
      WHEN N'5539' THEN N'תקל"ט'
      WHEN N'5540' THEN N'תק"מ'
      WHEN N'5541' THEN N'תקמ"א'
      WHEN N'5542' THEN N'תקמ"ב'
      WHEN N'5543' THEN N'תקמ"ג'
      WHEN N'5544' THEN N'תקמ"ד'
      WHEN N'5545' THEN N'תקמ"ה'
      WHEN N'5546' THEN N'תקמ"ו'
      WHEN N'5547' THEN N'תקמ"ז'
      WHEN N'5548' THEN N'תקמ"ח'
      WHEN N'5549' THEN N'תקמ"ט'
      WHEN N'5550' THEN N'תק"נ'
      WHEN N'5551' THEN N'תקנ"א'
      WHEN N'5552' THEN N'תקנ"ב'
      WHEN N'5553' THEN N'תקנ"ג'
      WHEN N'5554' THEN N'תקנ"ד'
      WHEN N'5555' THEN N'תקנ"ה'
      WHEN N'5556' THEN N'תקנ"ו'
      WHEN N'5557' THEN N'תקנ"ז'
      WHEN N'5558' THEN N'תקנ"ח'
      WHEN N'5559' THEN N'תקנ"ט'
      WHEN N'5560' THEN N'תק"ס'
      WHEN N'5561' THEN N'תקס"א'
      WHEN N'5562' THEN N'תקס"ב'
      WHEN N'5563' THEN N'תקס"ג'
      WHEN N'5564' THEN N'תקס"ד'
      WHEN N'5565' THEN N'תקס"ה'
      WHEN N'5566' THEN N'תקס"ו'
      WHEN N'5567' THEN N'תקס"ז'
      WHEN N'5568' THEN N'תקס"ח'
      WHEN N'5569' THEN N'תקס"ט'
      WHEN N'5570' THEN N'תק"ע'
      WHEN N'5571' THEN N'תקע"א'
      WHEN N'5572' THEN N'תקע"ב'
      WHEN N'5573' THEN N'תקע"ג'
      WHEN N'5574' THEN N'תקע"ד'
      WHEN N'5575' THEN N'תקע"ה'
      WHEN N'5576' THEN N'תקע"ו'
      WHEN N'5577' THEN N'תקע"ז'
      WHEN N'5578' THEN N'תקע"ח'
      WHEN N'5579' THEN N'תקע"ט'
      WHEN N'5580' THEN N'תק"פ'
      WHEN N'5581' THEN N'תקפ"א'
      WHEN N'5582' THEN N'תקפ"ב'
      WHEN N'5583' THEN N'תקפ"ג'
      WHEN N'5584' THEN N'תקפ"ד'
      WHEN N'5585' THEN N'תקפ"ה'
      WHEN N'5586' THEN N'תקפ"ו'
      WHEN N'5587' THEN N'תקפ"ז'
      WHEN N'5588' THEN N'תקפ"ח'
      WHEN N'5589' THEN N'תקפ"ט'
      WHEN N'5590' THEN N'תק"צ'
      WHEN N'5591' THEN N'תקצ"א'
      WHEN N'5592' THEN N'תקצ"ב'
      WHEN N'5593' THEN N'תקצ"ג'
      WHEN N'5594' THEN N'תקצ"ד'
      WHEN N'5595' THEN N'תקצ"ה'
      WHEN N'5596' THEN N'תקצ"ו'
      WHEN N'5597' THEN N'תקצ"ז'
      WHEN N'5598' THEN N'תקצ"ח'
      WHEN N'5599' THEN N'תקצ"ט'
      WHEN N'5600' THEN N'ת"ר'
      WHEN N'5601' THEN N'תר"א'
      WHEN N'5602' THEN N'תר"ב'
      WHEN N'5603' THEN N'תר"ג'
      WHEN N'5604' THEN N'תר"ד'
      WHEN N'5605' THEN N'תר"ה'
      WHEN N'5606' THEN N'תר"ו'
      WHEN N'5607' THEN N'תר"ז'
      WHEN N'5608' THEN N'תר"ח'
      WHEN N'5609' THEN N'תר"ט'
      WHEN N'5610' THEN N'תר"י'
      WHEN N'5611' THEN N'תרי"א'
      WHEN N'5612' THEN N'תרי"ב'
      WHEN N'5613' THEN N'תרי"ג'
      WHEN N'5614' THEN N'תרי"ד'
      WHEN N'5615' THEN N'תרט"ו'
      WHEN N'5616' THEN N'תרט"ז'
      WHEN N'5617' THEN N'תרי"ז'
      WHEN N'5618' THEN N'תרי"ח'
      WHEN N'5619' THEN N'תרי"ט'
      WHEN N'5620' THEN N'תר"כ'
      WHEN N'5621' THEN N'תרכ"א'
      WHEN N'5622' THEN N'תרכ"ב'
      WHEN N'5623' THEN N'תרכ"ג'
      WHEN N'5624' THEN N'תרכ"ד'
      WHEN N'5625' THEN N'תרכ"ה'
      WHEN N'5626' THEN N'תרכ"ו'
      WHEN N'5627' THEN N'תרכ"ז'
      WHEN N'5628' THEN N'תרכ"ח'
      WHEN N'5629' THEN N'תרכ"ט'
      WHEN N'5630' THEN N'תר"ל'
      WHEN N'5631' THEN N'תרל"א'
      WHEN N'5632' THEN N'תרל"ב'
      WHEN N'5633' THEN N'תרל"ג'
      WHEN N'5634' THEN N'תרל"ד'
      WHEN N'5635' THEN N'תרל"ה'
      WHEN N'5636' THEN N'תרל"ו'
      WHEN N'5637' THEN N'תרל"ז'
      WHEN N'5638' THEN N'תרל"ח'
      WHEN N'5639' THEN N'תרל"ט'
      WHEN N'5640' THEN N'תר"מ'
      WHEN N'5641' THEN N'תרמ"א'
      WHEN N'5642' THEN N'תרמ"ב'
      WHEN N'5643' THEN N'תרמ"ג'
      WHEN N'5644' THEN N'תרמ"ד'
      WHEN N'5645' THEN N'תרמ"ה'
      WHEN N'5646' THEN N'תרמ"ו'
      WHEN N'5647' THEN N'תרמ"ז'
      WHEN N'5648' THEN N'תרמ"ח'
      WHEN N'5649' THEN N'תרמ"ט'
      WHEN N'5650' THEN N'תר"נ'
      WHEN N'5651' THEN N'תרנ"א'
      WHEN N'5652' THEN N'תרנ"ב'
      WHEN N'5653' THEN N'תרנ"ג'
      WHEN N'5654' THEN N'תרנ"ד'
      WHEN N'5655' THEN N'תרנ"ה'
      WHEN N'5656' THEN N'תרנ"ו'
      WHEN N'5657' THEN N'תרנ"ז'
      WHEN N'5658' THEN N'תרנ"ח'
      WHEN N'5659' THEN N'תרנ"ט'
      WHEN N'5660' THEN N'תר"ס'
      WHEN N'5661' THEN N'תרס"א'
      WHEN N'5662' THEN N'תרס"ב'
      WHEN N'5663' THEN N'תרס"ג'
      WHEN N'5664' THEN N'תרס"ד'
      WHEN N'5665' THEN N'תרס"ה'
      WHEN N'5666' THEN N'תרס"ו'
      WHEN N'5667' THEN N'תרס"ז'
      WHEN N'5668' THEN N'תרס"ח'
      WHEN N'5669' THEN N'תרס"ט'
      WHEN N'5670' THEN N'תר"ע'
      WHEN N'5671' THEN N'תרע"א'
      WHEN N'5672' THEN N'תרע"ב'
      WHEN N'5673' THEN N'תרע"ג'
      WHEN N'5674' THEN N'תרע"ד'
      WHEN N'5675' THEN N'תרע"ה'
      WHEN N'5676' THEN N'תרע"ו'
      WHEN N'5677' THEN N'תרע"ז'
      WHEN N'5678' THEN N'תרע"ח'
      WHEN N'5679' THEN N'תרע"ט'
      WHEN N'5680' THEN N'תר"פ'
      WHEN N'5681' THEN N'תרפ"א'
      WHEN N'5682' THEN N'תרפ"ב'
      WHEN N'5683' THEN N'תרפ"ג'
      WHEN N'5684' THEN N'תרפ"ד'
      WHEN N'5685' THEN N'תרפ"ה'
      WHEN N'5686' THEN N'תרפ"ו'
      WHEN N'5687' THEN N'תרפ"ז'
      WHEN N'5688' THEN N'תרפ"ח'
      WHEN N'5689' THEN N'תרפ"ט'
      WHEN N'5690' THEN N'תר"צ'
      WHEN N'5691' THEN N'תרצ"א'
      WHEN N'5692' THEN N'תרצ"ב'
      WHEN N'5693' THEN N'תרצ"ג'
      WHEN N'5694' THEN N'תרצ"ד'
      WHEN N'5695' THEN N'תרצ"ה'
      WHEN N'5696' THEN N'תרצ"ו'
      WHEN N'5697' THEN N'תרצ"ז'
      WHEN N'5698' THEN N'תרצ"ח'
      WHEN N'5699' THEN N'תרצ"ט'
      WHEN N'5700' THEN N'ת"ש'
      WHEN N'5701' THEN N'תש"א'
      WHEN N'5702' THEN N'תש"ב'
      WHEN N'5703' THEN N'תש"ג'
      WHEN N'5704' THEN N'תש"ד'
      WHEN N'5705' THEN N'תש"ה'
      WHEN N'5706' THEN N'תש"ו'
      WHEN N'5707' THEN N'תש"ז'
      WHEN N'5708' THEN N'תש"ח'
      WHEN N'5709' THEN N'תש"ט'
      WHEN N'5710' THEN N'תש"י'
      WHEN N'5711' THEN N'תשי"א'
      WHEN N'5712' THEN N'תשי"ב'
      WHEN N'5713' THEN N'תשי"ג'
      WHEN N'5714' THEN N'תשי"ד'
      WHEN N'5715' THEN N'תשט"ו'
      WHEN N'5716' THEN N'תשט"ז'
      WHEN N'5717' THEN N'תשי"ז'
      WHEN N'5718' THEN N'תשי"ח'
      WHEN N'5719' THEN N'תשי"ט'
      WHEN N'5720' THEN N'תש"כ'
      WHEN N'5721' THEN N'תשכ"א'
      WHEN N'5722' THEN N'תשכ"ב'
      WHEN N'5723' THEN N'תשכ"ג'
      WHEN N'5724' THEN N'תשכ"ד'
      WHEN N'5725' THEN N'תשכ"ה'
      WHEN N'5726' THEN N'תשכ"ו'
      WHEN N'5727' THEN N'תשכ"ז'
      WHEN N'5728' THEN N'תשכ"ח'
      WHEN N'5729' THEN N'תשכ"ט'
      WHEN N'5730' THEN N'תש"ל'
      WHEN N'5731' THEN N'תשל"א'
      WHEN N'5732' THEN N'תשל"ב'
      WHEN N'5733' THEN N'תשל"ג'
      WHEN N'5734' THEN N'תשל"ד'
      WHEN N'5735' THEN N'תשל"ה'
      WHEN N'5736' THEN N'תשל"ו'
      WHEN N'5737' THEN N'תשל"ז'
      WHEN N'5738' THEN N'תשל"ח'
      WHEN N'5739' THEN N'תשל"ט'
      WHEN N'5740' THEN N'תש"מ'
      WHEN N'5741' THEN N'תשמ"א'
      WHEN N'5742' THEN N'תשמ"ב'
      WHEN N'5743' THEN N'תשמ"ג'
      WHEN N'5744' THEN N'תשמ"ד'
      WHEN N'5745' THEN N'תשמ"ה'
      WHEN N'5746' THEN N'תשמ"ו'
      WHEN N'5747' THEN N'תשמ"ז'
      WHEN N'5748' THEN N'תשמ"ח'
      WHEN N'5749' THEN N'תשמ"ט'
      WHEN N'5750' THEN N'תש"נ'
      WHEN N'5751' THEN N'תשנ"א'
      WHEN N'5752' THEN N'תשנ"ב'
      WHEN N'5753' THEN N'תשנ"ג'
      WHEN N'5754' THEN N'תשנ"ד'
      WHEN N'5755' THEN N'תשנ"ה'
      WHEN N'5756' THEN N'תשנ"ו'
      WHEN N'5757' THEN N'תשנ"ז'
      WHEN N'5758' THEN N'תשנ"ח'
      WHEN N'5759' THEN N'תשנ"ט'
      WHEN N'5760' THEN N'תש"ס'
      WHEN N'5761' THEN N'תשס"א'
      WHEN N'5762' THEN N'תשס"ב'
      WHEN N'5763' THEN N'תשס"ג'
      WHEN N'5764' THEN N'תשס"ד'
      WHEN N'5765' THEN N'תשס"ה'
      WHEN N'5766' THEN N'תשס"ו'
      WHEN N'5767' THEN N'תשס"ז'
      WHEN N'5768' THEN N'תשס"ח'
      WHEN N'5769' THEN N'תשס"ט'
      WHEN N'5770' THEN N'תש"ע'
      WHEN N'5771' THEN N'תשע"א'
      WHEN N'5772' THEN N'תשע"ב'
      WHEN N'5773' THEN N'תשע"ג'
      WHEN N'5774' THEN N'תשע"ד'
      WHEN N'5775' THEN N'תשע"ה'
      WHEN N'5776' THEN N'תשע"ו'
      WHEN N'5777' THEN N'תשע"ז'
      WHEN N'5778' THEN N'תשע"ח'
      WHEN N'5779' THEN N'תשע"ט'
      WHEN N'5780' THEN N'תש"פ'
      WHEN N'5781' THEN N'תשפ"א'
      WHEN N'5782' THEN N'תשפ"ב'
      WHEN N'5783' THEN N'תשפ"ג'
      WHEN N'5784' THEN N'תשפ"ד'
      WHEN N'5785' THEN N'תשפ"ה'
      WHEN N'5786' THEN N'תשפ"ו'
      WHEN N'5787' THEN N'תשפ"ז'
      WHEN N'5788' THEN N'תשפ"ח'
      WHEN N'5789' THEN N'תשפ"ט'
      WHEN N'5790' THEN N'תש"צ'
      WHEN N'5791' THEN N'תשצ"א'
      WHEN N'5792' THEN N'תשצ"ב'
      WHEN N'5793' THEN N'תשצ"ג'
      WHEN N'5794' THEN N'תשצ"ד'
      WHEN N'5795' THEN N'תשצ"ה'
      WHEN N'5796' THEN N'תשצ"ו'
      WHEN N'5797' THEN N'תשצ"ז'
      WHEN N'5798' THEN N'תשצ"ח'
      WHEN N'5799' THEN N'תשצ"ט'
      WHEN N'5800' THEN N'ת"ת'
      WHEN N'5801' THEN N'תת"א'
      WHEN N'5802' THEN N'תת"ב'
      WHEN N'5803' THEN N'תת"ג'
      WHEN N'5804' THEN N'תת"ד'
      WHEN N'5805' THEN N'תת"ה'
      WHEN N'5806' THEN N'תת"ו'
      WHEN N'5807' THEN N'תת"ז'
      WHEN N'5808' THEN N'תת"ח'
      WHEN N'5809' THEN N'תת"ט'
      WHEN N'5810' THEN N'תת"י'
      WHEN N'5811' THEN N'תתי"א'
      WHEN N'5812' THEN N'תתי"ב'
      WHEN N'5813' THEN N'תתי"ג'
      WHEN N'5814' THEN N'תתי"ד'
      WHEN N'5815' THEN N'תתט"ו'
      WHEN N'5816' THEN N'תתט"ז'
      WHEN N'5817' THEN N'תתי"ז'
      WHEN N'5818' THEN N'תתי"ח'
      WHEN N'5819' THEN N'תתי"ט'
      WHEN N'5820' THEN N'תת"כ'
      WHEN N'5821' THEN N'תתכ"א'
      WHEN N'5822' THEN N'תתכ"ב'
      WHEN N'5823' THEN N'תתכ"ג'
      WHEN N'5824' THEN N'תתכ"ד'
      WHEN N'5825' THEN N'תתכ"ה'
      WHEN N'5826' THEN N'תתכ"ו'
      WHEN N'5827' THEN N'תתכ"ז'
      WHEN N'5828' THEN N'תתכ"ח'
      WHEN N'5829' THEN N'תתכ"ט'
      WHEN N'5830' THEN N'תת"ל'
      WHEN N'5831' THEN N'תתל"א'
      WHEN N'5832' THEN N'תתל"ב'
      WHEN N'5833' THEN N'תתל"ג'
      WHEN N'5834' THEN N'תתל"ד'
      WHEN N'5835' THEN N'תתל"ה'
      WHEN N'5836' THEN N'תתל"ו'
      WHEN N'5837' THEN N'תתל"ז'
      WHEN N'5838' THEN N'תתל"ח'
      WHEN N'5839' THEN N'תתל"ט'
      WHEN N'5840' THEN N'תת"מ'
      WHEN N'5841' THEN N'תתמ"א'
      WHEN N'5842' THEN N'תתמ"ב'
      WHEN N'5843' THEN N'תתמ"ג'
      WHEN N'5844' THEN N'תתמ"ד'
      WHEN N'5845' THEN N'תתמ"ה'
      WHEN N'5846' THEN N'תתמ"ו'
      WHEN N'5847' THEN N'תתמ"ז'
      WHEN N'5848' THEN N'תתמ"ח'
      WHEN N'5849' THEN N'תתמ"ט'
      WHEN N'5850' THEN N'תת"נ'
      WHEN N'5851' THEN N'תתנ"א'
      WHEN N'5852' THEN N'תתנ"ב'
      WHEN N'5853' THEN N'תתנ"ג'
      WHEN N'5854' THEN N'תתנ"ד'
      WHEN N'5855' THEN N'תתנ"ה'
      WHEN N'5856' THEN N'תתנ"ו'
      WHEN N'5857' THEN N'תתנ"ז'
      WHEN N'5858' THEN N'תתנ"ח'
      WHEN N'5859' THEN N'תתנ"ט'
      WHEN N'5860' THEN N'תת"ס'
      WHEN N'5861' THEN N'תתס"א'
      WHEN N'5862' THEN N'תתס"ב'
      WHEN N'5863' THEN N'תתס"ג'
      WHEN N'5864' THEN N'תתס"ד'
      WHEN N'5865' THEN N'תתס"ה'
      WHEN N'5866' THEN N'תתס"ו'
      WHEN N'5867' THEN N'תתס"ז'
      WHEN N'5868' THEN N'תתס"ח'
      WHEN N'5869' THEN N'תתס"ט'
      WHEN N'5870' THEN N'תת"ע'
      WHEN N'5871' THEN N'תתע"א'
      WHEN N'5872' THEN N'תתע"ב'
      WHEN N'5873' THEN N'תתע"ג'
      WHEN N'5874' THEN N'תתע"ד'
      WHEN N'5875' THEN N'תתע"ה'
      WHEN N'5876' THEN N'תתע"ו'
      WHEN N'5877' THEN N'תתע"ז'
      WHEN N'5878' THEN N'תתע"ח'
      WHEN N'5879' THEN N'תתע"ט'
      WHEN N'5880' THEN N'תת"פ'
      WHEN N'5881' THEN N'תתפ"א'
      WHEN N'5882' THEN N'תתפ"ב'
      WHEN N'5883' THEN N'תתפ"ג'
      WHEN N'5884' THEN N'תתפ"ד'
      WHEN N'5885' THEN N'תתפ"ה'
      WHEN N'5886' THEN N'תתפ"ו'
      WHEN N'5887' THEN N'תתפ"ז'
      WHEN N'5888' THEN N'תתפ"ח'
      WHEN N'5889' THEN N'תתפ"ט'
      WHEN N'5890' THEN N'תת"צ'
      WHEN N'5891' THEN N'תתצ"א'
      WHEN N'5892' THEN N'תתצ"ב'
      WHEN N'5893' THEN N'תתצ"ג'
      WHEN N'5894' THEN N'תתצ"ד'
      WHEN N'5895' THEN N'תתצ"ה'
      WHEN N'5896' THEN N'תתצ"ו'
      WHEN N'5897' THEN N'תתצ"ז'
      WHEN N'5898' THEN N'תתצ"ח'
      WHEN N'5899' THEN N'תתצ"ט'
      WHEN N'5900' THEN N'תת"ק'
      WHEN N'5901' THEN N'תתק"א'
      WHEN N'5902' THEN N'תתק"ב'
      WHEN N'5903' THEN N'תתק"ג'
      WHEN N'5904' THEN N'תתק"ד'
      WHEN N'5905' THEN N'תתק"ה'
      WHEN N'5906' THEN N'תתק"ו'
      WHEN N'5907' THEN N'תתק"ז'
      WHEN N'5908' THEN N'תתק"ח'
      WHEN N'5909' THEN N'תתק"ט'
      WHEN N'5910' THEN N'תתק"י'
      WHEN N'5911' THEN N'תתקי"א'
      WHEN N'5912' THEN N'תתקי"ב'
      WHEN N'5913' THEN N'תתקי"ג'
      WHEN N'5914' THEN N'תתקי"ד'
      WHEN N'5915' THEN N'תתקט"ו'
      WHEN N'5916' THEN N'תתקט"ז'
      WHEN N'5917' THEN N'תתקי"ז'
      WHEN N'5918' THEN N'תתקי"ח'
      WHEN N'5919' THEN N'תתקי"ט'
      WHEN N'5920' THEN N'תתק"כ'
      WHEN N'5921' THEN N'תתקכ"א'
      WHEN N'5922' THEN N'תתקכ"ב'
      WHEN N'5923' THEN N'תתקכ"ג'
      WHEN N'5924' THEN N'תתקכ"ד'
      WHEN N'5925' THEN N'תתקכ"ה'
      WHEN N'5926' THEN N'תתקכ"ו'
      WHEN N'5927' THEN N'תתקכ"ז'
      WHEN N'5928' THEN N'תתקכ"ח'
      WHEN N'5929' THEN N'תתקכ"ט'
      WHEN N'5930' THEN N'תתק"ל'
      WHEN N'5931' THEN N'תתקל"א'
      WHEN N'5932' THEN N'תתקל"ב'
      WHEN N'5933' THEN N'תתקל"ג'
      WHEN N'5934' THEN N'תתקל"ד'
      WHEN N'5935' THEN N'תתקל"ה'
      WHEN N'5936' THEN N'תתקל"ו'
      WHEN N'5937' THEN N'תתקל"ז'
      WHEN N'5938' THEN N'תתקל"ח'
      WHEN N'5939' THEN N'תתקל"ט'
      WHEN N'5940' THEN N'תתק"מ'
      WHEN N'5941' THEN N'תתקמ"א'
      WHEN N'5942' THEN N'תתקמ"ב'
      WHEN N'5943' THEN N'תתקמ"ג'
      WHEN N'5944' THEN N'תתקמ"ד'
      WHEN N'5945' THEN N'תתקמ"ה'
      WHEN N'5946' THEN N'תתקמ"ו'
      WHEN N'5947' THEN N'תתקמ"ז'
      WHEN N'5948' THEN N'תתקמ"ח'
      WHEN N'5949' THEN N'תתקמ"ט'
      WHEN N'5950' THEN N'תתק"נ'
      WHEN N'5951' THEN N'תתקנ"א'
      WHEN N'5952' THEN N'תתקנ"ב'
      WHEN N'5953' THEN N'תתקנ"ג'
      WHEN N'5954' THEN N'תתקנ"ד'
      WHEN N'5955' THEN N'תתקנ"ה'
      WHEN N'5956' THEN N'תתקנ"ו'
      WHEN N'5957' THEN N'תתקנ"ז'
      WHEN N'5958' THEN N'תתקנ"ח'
      WHEN N'5959' THEN N'תתקנ"ט'
      WHEN N'5960' THEN N'תתק"ס'
      WHEN N'5961' THEN N'תתקס"א'
      WHEN N'5962' THEN N'תתקס"ב'
      WHEN N'5963' THEN N'תתקס"ג'
      WHEN N'5964' THEN N'תתקס"ד'
      WHEN N'5965' THEN N'תתקס"ה'
      WHEN N'5966' THEN N'תתקס"ו'
      WHEN N'5967' THEN N'תתקס"ז'
      WHEN N'5968' THEN N'תתקס"ח'
      WHEN N'5969' THEN N'תתקס"ט'
      WHEN N'5970' THEN N'תתק"ע'
      WHEN N'5971' THEN N'תתקע"א'
      WHEN N'5972' THEN N'תתקע"ב'
      WHEN N'5973' THEN N'תתקע"ג'
      WHEN N'5974' THEN N'תתקע"ד'
      WHEN N'5975' THEN N'תתקע"ה'
      WHEN N'5976' THEN N'תתקע"ו'
      WHEN N'5977' THEN N'תתקע"ז'
      WHEN N'5978' THEN N'תתקע"ח'
      WHEN N'5979' THEN N'תתקע"ט'
      WHEN N'5980' THEN N'תתק"פ'
      WHEN N'5981' THEN N'תתקפ"א'
      WHEN N'5982' THEN N'תתקפ"ב'
      WHEN N'5983' THEN N'תתקפ"ג'
      WHEN N'5984' THEN N'תתקפ"ד'
      WHEN N'5985' THEN N'תתקפ"ה'
      WHEN N'5986' THEN N'תתקפ"ו'
      WHEN N'5987' THEN N'תתקפ"ז'
      WHEN N'5988' THEN N'תתקפ"ח'
      WHEN N'5989' THEN N'תתקפ"ט'
      WHEN N'5990' THEN N'תתק"צ'
      WHEN N'5991' THEN N'תתקצ"א'
      WHEN N'5992' THEN N'תתקצ"ב'
      WHEN N'5993' THEN N'תתקצ"ג'
      WHEN N'5994' THEN N'תתקצ"ד'
      WHEN N'5995' THEN N'תתקצ"ה'
      WHEN N'5996' THEN N'תתקצ"ו'
      WHEN N'5997' THEN N'תתקצ"ז'
      WHEN N'5998' THEN N'תתקצ"ח'
      WHEN N'5999' THEN N'תתקצ"ט'
      ELSE [שנת_לימודים]
    END;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260616232951_CanonicalizeStoredAcademicYears'
)
BEGIN
    UPDATE [קלט_עוקץ_חודשי_אצווה]
    SET [שנת_לימודים] = LTRIM(RTRIM([שנת_לימודים]));

    UPDATE [קלט_עוקץ_חודשי_אצווה]
    SET [שנת_לימודים] = CASE [שנת_לימודים]
      WHEN N'2000' THEN N'תש"ס'
      WHEN N'2001' THEN N'תשס"א'
      WHEN N'2002' THEN N'תשס"ב'
      WHEN N'2003' THEN N'תשס"ג'
      WHEN N'2004' THEN N'תשס"ד'
      WHEN N'2005' THEN N'תשס"ה'
      WHEN N'2006' THEN N'תשס"ו'
      WHEN N'2007' THEN N'תשס"ז'
      WHEN N'2008' THEN N'תשס"ח'
      WHEN N'2009' THEN N'תשס"ט'
      WHEN N'2010' THEN N'תש"ע'
      WHEN N'2011' THEN N'תשע"א'
      WHEN N'2012' THEN N'תשע"ב'
      WHEN N'2013' THEN N'תשע"ג'
      WHEN N'2014' THEN N'תשע"ד'
      WHEN N'2015' THEN N'תשע"ה'
      WHEN N'2016' THEN N'תשע"ו'
      WHEN N'2017' THEN N'תשע"ז'
      WHEN N'2018' THEN N'תשע"ח'
      WHEN N'2019' THEN N'תשע"ט'
      WHEN N'2020' THEN N'תש"פ'
      WHEN N'2021' THEN N'תשפ"א'
      WHEN N'2022' THEN N'תשפ"ב'
      WHEN N'2023' THEN N'תשפ"ג'
      WHEN N'2024' THEN N'תשפ"ד'
      WHEN N'2025' THEN N'תשפ"ה'
      WHEN N'2026' THEN N'תשפ"ו'
      WHEN N'2027' THEN N'תשפ"ז'
      WHEN N'2028' THEN N'תשפ"ח'
      WHEN N'2029' THEN N'תשפ"ט'
      WHEN N'2030' THEN N'תש"צ'
      WHEN N'2031' THEN N'תשצ"א'
      WHEN N'2032' THEN N'תשצ"ב'
      WHEN N'2033' THEN N'תשצ"ג'
      WHEN N'2034' THEN N'תשצ"ד'
      WHEN N'2035' THEN N'תשצ"ה'
      WHEN N'2036' THEN N'תשצ"ו'
      WHEN N'2037' THEN N'תשצ"ז'
      WHEN N'2038' THEN N'תשצ"ח'
      WHEN N'2039' THEN N'תשצ"ט'
      WHEN N'2040' THEN N'ת"ת'
      WHEN N'2041' THEN N'תת"א'
      WHEN N'2042' THEN N'תת"ב'
      WHEN N'2043' THEN N'תת"ג'
      WHEN N'2044' THEN N'תת"ד'
      WHEN N'2045' THEN N'תת"ה'
      WHEN N'2046' THEN N'תת"ו'
      WHEN N'2047' THEN N'תת"ז'
      WHEN N'2048' THEN N'תת"ח'
      WHEN N'2049' THEN N'תת"ט'
      WHEN N'2050' THEN N'תת"י'
      WHEN N'2051' THEN N'תתי"א'
      WHEN N'2052' THEN N'תתי"ב'
      WHEN N'2053' THEN N'תתי"ג'
      WHEN N'2054' THEN N'תתי"ד'
      WHEN N'2055' THEN N'תתט"ו'
      WHEN N'2056' THEN N'תתט"ז'
      WHEN N'2057' THEN N'תתי"ז'
      WHEN N'2058' THEN N'תתי"ח'
      WHEN N'2059' THEN N'תתי"ט'
      WHEN N'2060' THEN N'תת"כ'
      WHEN N'2061' THEN N'תתכ"א'
      WHEN N'2062' THEN N'תתכ"ב'
      WHEN N'2063' THEN N'תתכ"ג'
      WHEN N'2064' THEN N'תתכ"ד'
      WHEN N'2065' THEN N'תתכ"ה'
      WHEN N'2066' THEN N'תתכ"ו'
      WHEN N'2067' THEN N'תתכ"ז'
      WHEN N'2068' THEN N'תתכ"ח'
      WHEN N'2069' THEN N'תתכ"ט'
      WHEN N'2070' THEN N'תת"ל'
      WHEN N'2071' THEN N'תתל"א'
      WHEN N'2072' THEN N'תתל"ב'
      WHEN N'2073' THEN N'תתל"ג'
      WHEN N'2074' THEN N'תתל"ד'
      WHEN N'2075' THEN N'תתל"ה'
      WHEN N'2076' THEN N'תתל"ו'
      WHEN N'2077' THEN N'תתל"ז'
      WHEN N'2078' THEN N'תתל"ח'
      WHEN N'2079' THEN N'תתל"ט'
      WHEN N'2080' THEN N'תת"מ'
      WHEN N'2081' THEN N'תתמ"א'
      WHEN N'2082' THEN N'תתמ"ב'
      WHEN N'2083' THEN N'תתמ"ג'
      WHEN N'2084' THEN N'תתמ"ד'
      WHEN N'2085' THEN N'תתמ"ה'
      WHEN N'2086' THEN N'תתמ"ו'
      WHEN N'2087' THEN N'תתמ"ז'
      WHEN N'2088' THEN N'תתמ"ח'
      WHEN N'2089' THEN N'תתמ"ט'
      WHEN N'2090' THEN N'תת"נ'
      WHEN N'2091' THEN N'תתנ"א'
      WHEN N'2092' THEN N'תתנ"ב'
      WHEN N'2093' THEN N'תתנ"ג'
      WHEN N'2094' THEN N'תתנ"ד'
      WHEN N'2095' THEN N'תתנ"ה'
      WHEN N'2096' THEN N'תתנ"ו'
      WHEN N'2097' THEN N'תתנ"ז'
      WHEN N'2098' THEN N'תתנ"ח'
      WHEN N'2099' THEN N'תתנ"ט'
      WHEN N'2100' THEN N'תת"ס'
      WHEN N'2101' THEN N'תתס"א'
      WHEN N'2102' THEN N'תתס"ב'
      WHEN N'2103' THEN N'תתס"ג'
      WHEN N'2104' THEN N'תתס"ד'
      WHEN N'2105' THEN N'תתס"ה'
      WHEN N'2106' THEN N'תתס"ו'
      WHEN N'2107' THEN N'תתס"ז'
      WHEN N'2108' THEN N'תתס"ח'
      WHEN N'2109' THEN N'תתס"ט'
      WHEN N'2110' THEN N'תת"ע'
      WHEN N'2111' THEN N'תתע"א'
      WHEN N'2112' THEN N'תתע"ב'
      WHEN N'2113' THEN N'תתע"ג'
      WHEN N'2114' THEN N'תתע"ד'
      WHEN N'2115' THEN N'תתע"ה'
      WHEN N'2116' THEN N'תתע"ו'
      WHEN N'2117' THEN N'תתע"ז'
      WHEN N'2118' THEN N'תתע"ח'
      WHEN N'2119' THEN N'תתע"ט'
      WHEN N'2120' THEN N'תת"פ'
      WHEN N'2121' THEN N'תתפ"א'
      WHEN N'2122' THEN N'תתפ"ב'
      WHEN N'2123' THEN N'תתפ"ג'
      WHEN N'2124' THEN N'תתפ"ד'
      WHEN N'2125' THEN N'תתפ"ה'
      WHEN N'2126' THEN N'תתפ"ו'
      WHEN N'2127' THEN N'תתפ"ז'
      WHEN N'2128' THEN N'תתפ"ח'
      WHEN N'2129' THEN N'תתפ"ט'
      WHEN N'2130' THEN N'תת"צ'
      WHEN N'2131' THEN N'תתצ"א'
      WHEN N'2132' THEN N'תתצ"ב'
      WHEN N'2133' THEN N'תתצ"ג'
      WHEN N'2134' THEN N'תתצ"ד'
      WHEN N'2135' THEN N'תתצ"ה'
      WHEN N'2136' THEN N'תתצ"ו'
      WHEN N'2137' THEN N'תתצ"ז'
      WHEN N'2138' THEN N'תתצ"ח'
      WHEN N'2139' THEN N'תתצ"ט'
      WHEN N'2140' THEN N'תת"ק'
      WHEN N'2141' THEN N'תתק"א'
      WHEN N'2142' THEN N'תתק"ב'
      WHEN N'2143' THEN N'תתק"ג'
      WHEN N'2144' THEN N'תתק"ד'
      WHEN N'2145' THEN N'תתק"ה'
      WHEN N'2146' THEN N'תתק"ו'
      WHEN N'2147' THEN N'תתק"ז'
      WHEN N'2148' THEN N'תתק"ח'
      WHEN N'2149' THEN N'תתק"ט'
      WHEN N'2150' THEN N'תתק"י'
      WHEN N'2151' THEN N'תתקי"א'
      WHEN N'2152' THEN N'תתקי"ב'
      WHEN N'2153' THEN N'תתקי"ג'
      WHEN N'2154' THEN N'תתקי"ד'
      WHEN N'2155' THEN N'תתקט"ו'
      WHEN N'2156' THEN N'תתקט"ז'
      WHEN N'2157' THEN N'תתקי"ז'
      WHEN N'2158' THEN N'תתקי"ח'
      WHEN N'2159' THEN N'תתקי"ט'
      WHEN N'2160' THEN N'תתק"כ'
      WHEN N'2161' THEN N'תתקכ"א'
      WHEN N'2162' THEN N'תתקכ"ב'
      WHEN N'2163' THEN N'תתקכ"ג'
      WHEN N'2164' THEN N'תתקכ"ד'
      WHEN N'2165' THEN N'תתקכ"ה'
      WHEN N'2166' THEN N'תתקכ"ו'
      WHEN N'2167' THEN N'תתקכ"ז'
      WHEN N'2168' THEN N'תתקכ"ח'
      WHEN N'2169' THEN N'תתקכ"ט'
      WHEN N'2170' THEN N'תתק"ל'
      WHEN N'2171' THEN N'תתקל"א'
      WHEN N'2172' THEN N'תתקל"ב'
      WHEN N'2173' THEN N'תתקל"ג'
      WHEN N'2174' THEN N'תתקל"ד'
      WHEN N'2175' THEN N'תתקל"ה'
      WHEN N'2176' THEN N'תתקל"ו'
      WHEN N'2177' THEN N'תתקל"ז'
      WHEN N'2178' THEN N'תתקל"ח'
      WHEN N'2179' THEN N'תתקל"ט'
      WHEN N'2180' THEN N'תתק"מ'
      WHEN N'2181' THEN N'תתקמ"א'
      WHEN N'2182' THEN N'תתקמ"ב'
      WHEN N'2183' THEN N'תתקמ"ג'
      WHEN N'2184' THEN N'תתקמ"ד'
      WHEN N'2185' THEN N'תתקמ"ה'
      WHEN N'2186' THEN N'תתקמ"ו'
      WHEN N'2187' THEN N'תתקמ"ז'
      WHEN N'2188' THEN N'תתקמ"ח'
      WHEN N'2189' THEN N'תתקמ"ט'
      WHEN N'2190' THEN N'תתק"נ'
      WHEN N'2191' THEN N'תתקנ"א'
      WHEN N'2192' THEN N'תתקנ"ב'
      WHEN N'2193' THEN N'תתקנ"ג'
      WHEN N'2194' THEN N'תתקנ"ד'
      WHEN N'2195' THEN N'תתקנ"ה'
      WHEN N'2196' THEN N'תתקנ"ו'
      WHEN N'2197' THEN N'תתקנ"ז'
      WHEN N'2198' THEN N'תתקנ"ח'
      WHEN N'2199' THEN N'תתקנ"ט'
      WHEN N'2200' THEN N'תתק"ס'
      WHEN N'5001' THEN N'א'''
      WHEN N'5002' THEN N'ב'''
      WHEN N'5003' THEN N'ג'''
      WHEN N'5004' THEN N'ד'''
      WHEN N'5005' THEN N'ה'''
      WHEN N'5006' THEN N'ו'''
      WHEN N'5007' THEN N'ז'''
      WHEN N'5008' THEN N'ח'''
      WHEN N'5009' THEN N'ט'''
      WHEN N'5010' THEN N'י'''
      WHEN N'5011' THEN N'י"א'
      WHEN N'5012' THEN N'י"ב'
      WHEN N'5013' THEN N'י"ג'
      WHEN N'5014' THEN N'י"ד'
      WHEN N'5015' THEN N'ט"ו'
      WHEN N'5016' THEN N'ט"ז'
      WHEN N'5017' THEN N'י"ז'
      WHEN N'5018' THEN N'י"ח'
      WHEN N'5019' THEN N'י"ט'
      WHEN N'5020' THEN N'כ'''
      WHEN N'5021' THEN N'כ"א'
      WHEN N'5022' THEN N'כ"ב'
      WHEN N'5023' THEN N'כ"ג'
      WHEN N'5024' THEN N'כ"ד'
      WHEN N'5025' THEN N'כ"ה'
      WHEN N'5026' THEN N'כ"ו'
      WHEN N'5027' THEN N'כ"ז'
      WHEN N'5028' THEN N'כ"ח'
      WHEN N'5029' THEN N'כ"ט'
      WHEN N'5030' THEN N'ל'''
      WHEN N'5031' THEN N'ל"א'
      WHEN N'5032' THEN N'ל"ב'
      WHEN N'5033' THEN N'ל"ג'
      WHEN N'5034' THEN N'ל"ד'
      WHEN N'5035' THEN N'ל"ה'
      WHEN N'5036' THEN N'ל"ו'
      WHEN N'5037' THEN N'ל"ז'
      WHEN N'5038' THEN N'ל"ח'
      WHEN N'5039' THEN N'ל"ט'
      WHEN N'5040' THEN N'מ'''
      WHEN N'5041' THEN N'מ"א'
      WHEN N'5042' THEN N'מ"ב'
      WHEN N'5043' THEN N'מ"ג'
      WHEN N'5044' THEN N'מ"ד'
      WHEN N'5045' THEN N'מ"ה'
      WHEN N'5046' THEN N'מ"ו'
      WHEN N'5047' THEN N'מ"ז'
      WHEN N'5048' THEN N'מ"ח'
      WHEN N'5049' THEN N'מ"ט'
      WHEN N'5050' THEN N'נ'''
      WHEN N'5051' THEN N'נ"א'
      WHEN N'5052' THEN N'נ"ב'
      WHEN N'5053' THEN N'נ"ג'
      WHEN N'5054' THEN N'נ"ד'
      WHEN N'5055' THEN N'נ"ה'
      WHEN N'5056' THEN N'נ"ו'
      WHEN N'5057' THEN N'נ"ז'
      WHEN N'5058' THEN N'נ"ח'
      WHEN N'5059' THEN N'נ"ט'
      WHEN N'5060' THEN N'ס'''
      WHEN N'5061' THEN N'ס"א'
      WHEN N'5062' THEN N'ס"ב'
      WHEN N'5063' THEN N'ס"ג'
      WHEN N'5064' THEN N'ס"ד'
      WHEN N'5065' THEN N'ס"ה'
      WHEN N'5066' THEN N'ס"ו'
      WHEN N'5067' THEN N'ס"ז'
      WHEN N'5068' THEN N'ס"ח'
      WHEN N'5069' THEN N'ס"ט'
      WHEN N'5070' THEN N'ע'''
      WHEN N'5071' THEN N'ע"א'
      WHEN N'5072' THEN N'ע"ב'
      WHEN N'5073' THEN N'ע"ג'
      WHEN N'5074' THEN N'ע"ד'
      WHEN N'5075' THEN N'ע"ה'
      WHEN N'5076' THEN N'ע"ו'
      WHEN N'5077' THEN N'ע"ז'
      WHEN N'5078' THEN N'ע"ח'
      WHEN N'5079' THEN N'ע"ט'
      WHEN N'5080' THEN N'פ'''
      WHEN N'5081' THEN N'פ"א'
      WHEN N'5082' THEN N'פ"ב'
      WHEN N'5083' THEN N'פ"ג'
      WHEN N'5084' THEN N'פ"ד'
      WHEN N'5085' THEN N'פ"ה'
      WHEN N'5086' THEN N'פ"ו'
      WHEN N'5087' THEN N'פ"ז'
      WHEN N'5088' THEN N'פ"ח'
      WHEN N'5089' THEN N'פ"ט'
      WHEN N'5090' THEN N'צ'''
      WHEN N'5091' THEN N'צ"א'
      WHEN N'5092' THEN N'צ"ב'
      WHEN N'5093' THEN N'צ"ג'
      WHEN N'5094' THEN N'צ"ד'
      WHEN N'5095' THEN N'צ"ה'
      WHEN N'5096' THEN N'צ"ו'
      WHEN N'5097' THEN N'צ"ז'
      WHEN N'5098' THEN N'צ"ח'
      WHEN N'5099' THEN N'צ"ט'
      WHEN N'5100' THEN N'ק'''
      WHEN N'5101' THEN N'ק"א'
      WHEN N'5102' THEN N'ק"ב'
      WHEN N'5103' THEN N'ק"ג'
      WHEN N'5104' THEN N'ק"ד'
      WHEN N'5105' THEN N'ק"ה'
      WHEN N'5106' THEN N'ק"ו'
      WHEN N'5107' THEN N'ק"ז'
      WHEN N'5108' THEN N'ק"ח'
      WHEN N'5109' THEN N'ק"ט'
      WHEN N'5110' THEN N'ק"י'
      WHEN N'5111' THEN N'קי"א'
      WHEN N'5112' THEN N'קי"ב'
      WHEN N'5113' THEN N'קי"ג'
      WHEN N'5114' THEN N'קי"ד'
      WHEN N'5115' THEN N'קט"ו'
      WHEN N'5116' THEN N'קט"ז'
      WHEN N'5117' THEN N'קי"ז'
      WHEN N'5118' THEN N'קי"ח'
      WHEN N'5119' THEN N'קי"ט'
      WHEN N'5120' THEN N'ק"כ'
      WHEN N'5121' THEN N'קכ"א'
      WHEN N'5122' THEN N'קכ"ב'
      WHEN N'5123' THEN N'קכ"ג'
      WHEN N'5124' THEN N'קכ"ד'
      WHEN N'5125' THEN N'קכ"ה'
      WHEN N'5126' THEN N'קכ"ו'
      WHEN N'5127' THEN N'קכ"ז'
      WHEN N'5128' THEN N'קכ"ח'
      WHEN N'5129' THEN N'קכ"ט'
      WHEN N'5130' THEN N'ק"ל'
      WHEN N'5131' THEN N'קל"א'
      WHEN N'5132' THEN N'קל"ב'
      WHEN N'5133' THEN N'קל"ג'
      WHEN N'5134' THEN N'קל"ד'
      WHEN N'5135' THEN N'קל"ה'
      WHEN N'5136' THEN N'קל"ו'
      WHEN N'5137' THEN N'קל"ז'
      WHEN N'5138' THEN N'קל"ח'
      WHEN N'5139' THEN N'קל"ט'
      WHEN N'5140' THEN N'ק"מ'
      WHEN N'5141' THEN N'קמ"א'
      WHEN N'5142' THEN N'קמ"ב'
      WHEN N'5143' THEN N'קמ"ג'
      WHEN N'5144' THEN N'קמ"ד'
      WHEN N'5145' THEN N'קמ"ה'
      WHEN N'5146' THEN N'קמ"ו'
      WHEN N'5147' THEN N'קמ"ז'
      WHEN N'5148' THEN N'קמ"ח'
      WHEN N'5149' THEN N'קמ"ט'
      WHEN N'5150' THEN N'ק"נ'
      WHEN N'5151' THEN N'קנ"א'
      WHEN N'5152' THEN N'קנ"ב'
      WHEN N'5153' THEN N'קנ"ג'
      WHEN N'5154' THEN N'קנ"ד'
      WHEN N'5155' THEN N'קנ"ה'
      WHEN N'5156' THEN N'קנ"ו'
      WHEN N'5157' THEN N'קנ"ז'
      WHEN N'5158' THEN N'קנ"ח'
      WHEN N'5159' THEN N'קנ"ט'
      WHEN N'5160' THEN N'ק"ס'
      WHEN N'5161' THEN N'קס"א'
      WHEN N'5162' THEN N'קס"ב'
      WHEN N'5163' THEN N'קס"ג'
      WHEN N'5164' THEN N'קס"ד'
      WHEN N'5165' THEN N'קס"ה'
      WHEN N'5166' THEN N'קס"ו'
      WHEN N'5167' THEN N'קס"ז'
      WHEN N'5168' THEN N'קס"ח'
      WHEN N'5169' THEN N'קס"ט'
      WHEN N'5170' THEN N'ק"ע'
      WHEN N'5171' THEN N'קע"א'
      WHEN N'5172' THEN N'קע"ב'
      WHEN N'5173' THEN N'קע"ג'
      WHEN N'5174' THEN N'קע"ד'
      WHEN N'5175' THEN N'קע"ה'
      WHEN N'5176' THEN N'קע"ו'
      WHEN N'5177' THEN N'קע"ז'
      WHEN N'5178' THEN N'קע"ח'
      WHEN N'5179' THEN N'קע"ט'
      WHEN N'5180' THEN N'ק"פ'
      WHEN N'5181' THEN N'קפ"א'
      WHEN N'5182' THEN N'קפ"ב'
      WHEN N'5183' THEN N'קפ"ג'
      WHEN N'5184' THEN N'קפ"ד'
      WHEN N'5185' THEN N'קפ"ה'
      WHEN N'5186' THEN N'קפ"ו'
      WHEN N'5187' THEN N'קפ"ז'
      WHEN N'5188' THEN N'קפ"ח'
      WHEN N'5189' THEN N'קפ"ט'
      WHEN N'5190' THEN N'ק"צ'
      WHEN N'5191' THEN N'קצ"א'
      WHEN N'5192' THEN N'קצ"ב'
      WHEN N'5193' THEN N'קצ"ג'
      WHEN N'5194' THEN N'קצ"ד'
      WHEN N'5195' THEN N'קצ"ה'
      WHEN N'5196' THEN N'קצ"ו'
      WHEN N'5197' THEN N'קצ"ז'
      WHEN N'5198' THEN N'קצ"ח'
      WHEN N'5199' THEN N'קצ"ט'
      WHEN N'5200' THEN N'ר'''
      WHEN N'5201' THEN N'ר"א'
      WHEN N'5202' THEN N'ר"ב'
      WHEN N'5203' THEN N'ר"ג'
      WHEN N'5204' THEN N'ר"ד'
      WHEN N'5205' THEN N'ר"ה'
      WHEN N'5206' THEN N'ר"ו'
      WHEN N'5207' THEN N'ר"ז'
      WHEN N'5208' THEN N'ר"ח'
      WHEN N'5209' THEN N'ר"ט'
      WHEN N'5210' THEN N'ר"י'
      WHEN N'5211' THEN N'רי"א'
      WHEN N'5212' THEN N'רי"ב'
      WHEN N'5213' THEN N'רי"ג'
      WHEN N'5214' THEN N'רי"ד'
      WHEN N'5215' THEN N'רט"ו'
      WHEN N'5216' THEN N'רט"ז'
      WHEN N'5217' THEN N'רי"ז'
      WHEN N'5218' THEN N'רי"ח'
      WHEN N'5219' THEN N'רי"ט'
      WHEN N'5220' THEN N'ר"כ'
      WHEN N'5221' THEN N'רכ"א'
      WHEN N'5222' THEN N'רכ"ב'
      WHEN N'5223' THEN N'רכ"ג'
      WHEN N'5224' THEN N'רכ"ד'
      WHEN N'5225' THEN N'רכ"ה'
      WHEN N'5226' THEN N'רכ"ו'
      WHEN N'5227' THEN N'רכ"ז'
      WHEN N'5228' THEN N'רכ"ח'
      WHEN N'5229' THEN N'רכ"ט'
      WHEN N'5230' THEN N'ר"ל'
      WHEN N'5231' THEN N'רל"א'
      WHEN N'5232' THEN N'רל"ב'
      WHEN N'5233' THEN N'רל"ג'
      WHEN N'5234' THEN N'רל"ד'
      WHEN N'5235' THEN N'רל"ה'
      WHEN N'5236' THEN N'רל"ו'
      WHEN N'5237' THEN N'רל"ז'
      WHEN N'5238' THEN N'רל"ח'
      WHEN N'5239' THEN N'רל"ט'
      WHEN N'5240' THEN N'ר"מ'
      WHEN N'5241' THEN N'רמ"א'
      WHEN N'5242' THEN N'רמ"ב'
      WHEN N'5243' THEN N'רמ"ג'
      WHEN N'5244' THEN N'רמ"ד'
      WHEN N'5245' THEN N'רמ"ה'
      WHEN N'5246' THEN N'רמ"ו'
      WHEN N'5247' THEN N'רמ"ז'
      WHEN N'5248' THEN N'רמ"ח'
      WHEN N'5249' THEN N'רמ"ט'
      WHEN N'5250' THEN N'ר"נ'
      WHEN N'5251' THEN N'רנ"א'
      WHEN N'5252' THEN N'רנ"ב'
      WHEN N'5253' THEN N'רנ"ג'
      WHEN N'5254' THEN N'רנ"ד'
      WHEN N'5255' THEN N'רנ"ה'
      WHEN N'5256' THEN N'רנ"ו'
      WHEN N'5257' THEN N'רנ"ז'
      WHEN N'5258' THEN N'רנ"ח'
      WHEN N'5259' THEN N'רנ"ט'
      WHEN N'5260' THEN N'ר"ס'
      WHEN N'5261' THEN N'רס"א'
      WHEN N'5262' THEN N'רס"ב'
      WHEN N'5263' THEN N'רס"ג'
      WHEN N'5264' THEN N'רס"ד'
      WHEN N'5265' THEN N'רס"ה'
      WHEN N'5266' THEN N'רס"ו'
      WHEN N'5267' THEN N'רס"ז'
      WHEN N'5268' THEN N'רס"ח'
      WHEN N'5269' THEN N'רס"ט'
      WHEN N'5270' THEN N'ר"ע'
      WHEN N'5271' THEN N'רע"א'
      WHEN N'5272' THEN N'רע"ב'
      WHEN N'5273' THEN N'רע"ג'
      WHEN N'5274' THEN N'רע"ד'
      WHEN N'5275' THEN N'רע"ה'
      WHEN N'5276' THEN N'רע"ו'
      WHEN N'5277' THEN N'רע"ז'
      WHEN N'5278' THEN N'רע"ח'
      WHEN N'5279' THEN N'רע"ט'
      WHEN N'5280' THEN N'ר"פ'
      WHEN N'5281' THEN N'רפ"א'
      WHEN N'5282' THEN N'רפ"ב'
      WHEN N'5283' THEN N'רפ"ג'
      WHEN N'5284' THEN N'רפ"ד'
      WHEN N'5285' THEN N'רפ"ה'
      WHEN N'5286' THEN N'רפ"ו'
      WHEN N'5287' THEN N'רפ"ז'
      WHEN N'5288' THEN N'רפ"ח'
      WHEN N'5289' THEN N'רפ"ט'
      WHEN N'5290' THEN N'ר"צ'
      WHEN N'5291' THEN N'רצ"א'
      WHEN N'5292' THEN N'רצ"ב'
      WHEN N'5293' THEN N'רצ"ג'
      WHEN N'5294' THEN N'רצ"ד'
      WHEN N'5295' THEN N'רצ"ה'
      WHEN N'5296' THEN N'רצ"ו'
      WHEN N'5297' THEN N'רצ"ז'
      WHEN N'5298' THEN N'רצ"ח'
      WHEN N'5299' THEN N'רצ"ט'
      WHEN N'5300' THEN N'ש'''
      WHEN N'5301' THEN N'ש"א'
      WHEN N'5302' THEN N'ש"ב'
      WHEN N'5303' THEN N'ש"ג'
      WHEN N'5304' THEN N'ש"ד'
      WHEN N'5305' THEN N'ש"ה'
      WHEN N'5306' THEN N'ש"ו'
      WHEN N'5307' THEN N'ש"ז'
      WHEN N'5308' THEN N'ש"ח'
      WHEN N'5309' THEN N'ש"ט'
      WHEN N'5310' THEN N'ש"י'
      WHEN N'5311' THEN N'שי"א'
      WHEN N'5312' THEN N'שי"ב'
      WHEN N'5313' THEN N'שי"ג'
      WHEN N'5314' THEN N'שי"ד'
      WHEN N'5315' THEN N'שט"ו'
      WHEN N'5316' THEN N'שט"ז'
      WHEN N'5317' THEN N'שי"ז'
      WHEN N'5318' THEN N'שי"ח'
      WHEN N'5319' THEN N'שי"ט'
      WHEN N'5320' THEN N'ש"כ'
      WHEN N'5321' THEN N'שכ"א'
      WHEN N'5322' THEN N'שכ"ב'
      WHEN N'5323' THEN N'שכ"ג'
      WHEN N'5324' THEN N'שכ"ד'
      WHEN N'5325' THEN N'שכ"ה'
      WHEN N'5326' THEN N'שכ"ו'
      WHEN N'5327' THEN N'שכ"ז'
      WHEN N'5328' THEN N'שכ"ח'
      WHEN N'5329' THEN N'שכ"ט'
      WHEN N'5330' THEN N'ש"ל'
      WHEN N'5331' THEN N'של"א'
      WHEN N'5332' THEN N'של"ב'
      WHEN N'5333' THEN N'של"ג'
      WHEN N'5334' THEN N'של"ד'
      WHEN N'5335' THEN N'של"ה'
      WHEN N'5336' THEN N'של"ו'
      WHEN N'5337' THEN N'של"ז'
      WHEN N'5338' THEN N'של"ח'
      WHEN N'5339' THEN N'של"ט'
      WHEN N'5340' THEN N'ש"מ'
      WHEN N'5341' THEN N'שמ"א'
      WHEN N'5342' THEN N'שמ"ב'
      WHEN N'5343' THEN N'שמ"ג'
      WHEN N'5344' THEN N'שמ"ד'
      WHEN N'5345' THEN N'שמ"ה'
      WHEN N'5346' THEN N'שמ"ו'
      WHEN N'5347' THEN N'שמ"ז'
      WHEN N'5348' THEN N'שמ"ח'
      WHEN N'5349' THEN N'שמ"ט'
      WHEN N'5350' THEN N'ש"נ'
      WHEN N'5351' THEN N'שנ"א'
      WHEN N'5352' THEN N'שנ"ב'
      WHEN N'5353' THEN N'שנ"ג'
      WHEN N'5354' THEN N'שנ"ד'
      WHEN N'5355' THEN N'שנ"ה'
      WHEN N'5356' THEN N'שנ"ו'
      WHEN N'5357' THEN N'שנ"ז'
      WHEN N'5358' THEN N'שנ"ח'
      WHEN N'5359' THEN N'שנ"ט'
      WHEN N'5360' THEN N'ש"ס'
      WHEN N'5361' THEN N'שס"א'
      WHEN N'5362' THEN N'שס"ב'
      WHEN N'5363' THEN N'שס"ג'
      WHEN N'5364' THEN N'שס"ד'
      WHEN N'5365' THEN N'שס"ה'
      WHEN N'5366' THEN N'שס"ו'
      WHEN N'5367' THEN N'שס"ז'
      WHEN N'5368' THEN N'שס"ח'
      WHEN N'5369' THEN N'שס"ט'
      WHEN N'5370' THEN N'ש"ע'
      WHEN N'5371' THEN N'שע"א'
      WHEN N'5372' THEN N'שע"ב'
      WHEN N'5373' THEN N'שע"ג'
      WHEN N'5374' THEN N'שע"ד'
      WHEN N'5375' THEN N'שע"ה'
      WHEN N'5376' THEN N'שע"ו'
      WHEN N'5377' THEN N'שע"ז'
      WHEN N'5378' THEN N'שע"ח'
      WHEN N'5379' THEN N'שע"ט'
      WHEN N'5380' THEN N'ש"פ'
      WHEN N'5381' THEN N'שפ"א'
      WHEN N'5382' THEN N'שפ"ב'
      WHEN N'5383' THEN N'שפ"ג'
      WHEN N'5384' THEN N'שפ"ד'
      WHEN N'5385' THEN N'שפ"ה'
      WHEN N'5386' THEN N'שפ"ו'
      WHEN N'5387' THEN N'שפ"ז'
      WHEN N'5388' THEN N'שפ"ח'
      WHEN N'5389' THEN N'שפ"ט'
      WHEN N'5390' THEN N'ש"צ'
      WHEN N'5391' THEN N'שצ"א'
      WHEN N'5392' THEN N'שצ"ב'
      WHEN N'5393' THEN N'שצ"ג'
      WHEN N'5394' THEN N'שצ"ד'
      WHEN N'5395' THEN N'שצ"ה'
      WHEN N'5396' THEN N'שצ"ו'
      WHEN N'5397' THEN N'שצ"ז'
      WHEN N'5398' THEN N'שצ"ח'
      WHEN N'5399' THEN N'שצ"ט'
      WHEN N'5400' THEN N'ת'''
      WHEN N'5401' THEN N'ת"א'
      WHEN N'5402' THEN N'ת"ב'
      WHEN N'5403' THEN N'ת"ג'
      WHEN N'5404' THEN N'ת"ד'
      WHEN N'5405' THEN N'ת"ה'
      WHEN N'5406' THEN N'ת"ו'
      WHEN N'5407' THEN N'ת"ז'
      WHEN N'5408' THEN N'ת"ח'
      WHEN N'5409' THEN N'ת"ט'
      WHEN N'5410' THEN N'ת"י'
      WHEN N'5411' THEN N'תי"א'
      WHEN N'5412' THEN N'תי"ב'
      WHEN N'5413' THEN N'תי"ג'
      WHEN N'5414' THEN N'תי"ד'
      WHEN N'5415' THEN N'תט"ו'
      WHEN N'5416' THEN N'תט"ז'
      WHEN N'5417' THEN N'תי"ז'
      WHEN N'5418' THEN N'תי"ח'
      WHEN N'5419' THEN N'תי"ט'
      WHEN N'5420' THEN N'ת"כ'
      WHEN N'5421' THEN N'תכ"א'
      WHEN N'5422' THEN N'תכ"ב'
      WHEN N'5423' THEN N'תכ"ג'
      WHEN N'5424' THEN N'תכ"ד'
      WHEN N'5425' THEN N'תכ"ה'
      WHEN N'5426' THEN N'תכ"ו'
      WHEN N'5427' THEN N'תכ"ז'
      WHEN N'5428' THEN N'תכ"ח'
      WHEN N'5429' THEN N'תכ"ט'
      WHEN N'5430' THEN N'ת"ל'
      WHEN N'5431' THEN N'תל"א'
      WHEN N'5432' THEN N'תל"ב'
      WHEN N'5433' THEN N'תל"ג'
      WHEN N'5434' THEN N'תל"ד'
      WHEN N'5435' THEN N'תל"ה'
      WHEN N'5436' THEN N'תל"ו'
      WHEN N'5437' THEN N'תל"ז'
      WHEN N'5438' THEN N'תל"ח'
      WHEN N'5439' THEN N'תל"ט'
      WHEN N'5440' THEN N'ת"מ'
      WHEN N'5441' THEN N'תמ"א'
      WHEN N'5442' THEN N'תמ"ב'
      WHEN N'5443' THEN N'תמ"ג'
      WHEN N'5444' THEN N'תמ"ד'
      WHEN N'5445' THEN N'תמ"ה'
      WHEN N'5446' THEN N'תמ"ו'
      WHEN N'5447' THEN N'תמ"ז'
      WHEN N'5448' THEN N'תמ"ח'
      WHEN N'5449' THEN N'תמ"ט'
      WHEN N'5450' THEN N'ת"נ'
      WHEN N'5451' THEN N'תנ"א'
      WHEN N'5452' THEN N'תנ"ב'
      WHEN N'5453' THEN N'תנ"ג'
      WHEN N'5454' THEN N'תנ"ד'
      WHEN N'5455' THEN N'תנ"ה'
      WHEN N'5456' THEN N'תנ"ו'
      WHEN N'5457' THEN N'תנ"ז'
      WHEN N'5458' THEN N'תנ"ח'
      WHEN N'5459' THEN N'תנ"ט'
      WHEN N'5460' THEN N'ת"ס'
      WHEN N'5461' THEN N'תס"א'
      WHEN N'5462' THEN N'תס"ב'
      WHEN N'5463' THEN N'תס"ג'
      WHEN N'5464' THEN N'תס"ד'
      WHEN N'5465' THEN N'תס"ה'
      WHEN N'5466' THEN N'תס"ו'
      WHEN N'5467' THEN N'תס"ז'
      WHEN N'5468' THEN N'תס"ח'
      WHEN N'5469' THEN N'תס"ט'
      WHEN N'5470' THEN N'ת"ע'
      WHEN N'5471' THEN N'תע"א'
      WHEN N'5472' THEN N'תע"ב'
      WHEN N'5473' THEN N'תע"ג'
      WHEN N'5474' THEN N'תע"ד'
      WHEN N'5475' THEN N'תע"ה'
      WHEN N'5476' THEN N'תע"ו'
      WHEN N'5477' THEN N'תע"ז'
      WHEN N'5478' THEN N'תע"ח'
      WHEN N'5479' THEN N'תע"ט'
      WHEN N'5480' THEN N'ת"פ'
      WHEN N'5481' THEN N'תפ"א'
      WHEN N'5482' THEN N'תפ"ב'
      WHEN N'5483' THEN N'תפ"ג'
      WHEN N'5484' THEN N'תפ"ד'
      WHEN N'5485' THEN N'תפ"ה'
      WHEN N'5486' THEN N'תפ"ו'
      WHEN N'5487' THEN N'תפ"ז'
      WHEN N'5488' THEN N'תפ"ח'
      WHEN N'5489' THEN N'תפ"ט'
      WHEN N'5490' THEN N'ת"צ'
      WHEN N'5491' THEN N'תצ"א'
      WHEN N'5492' THEN N'תצ"ב'
      WHEN N'5493' THEN N'תצ"ג'
      WHEN N'5494' THEN N'תצ"ד'
      WHEN N'5495' THEN N'תצ"ה'
      WHEN N'5496' THEN N'תצ"ו'
      WHEN N'5497' THEN N'תצ"ז'
      WHEN N'5498' THEN N'תצ"ח'
      WHEN N'5499' THEN N'תצ"ט'
      WHEN N'5500' THEN N'ת"ק'
      WHEN N'5501' THEN N'תק"א'
      WHEN N'5502' THEN N'תק"ב'
      WHEN N'5503' THEN N'תק"ג'
      WHEN N'5504' THEN N'תק"ד'
      WHEN N'5505' THEN N'תק"ה'
      WHEN N'5506' THEN N'תק"ו'
      WHEN N'5507' THEN N'תק"ז'
      WHEN N'5508' THEN N'תק"ח'
      WHEN N'5509' THEN N'תק"ט'
      WHEN N'5510' THEN N'תק"י'
      WHEN N'5511' THEN N'תקי"א'
      WHEN N'5512' THEN N'תקי"ב'
      WHEN N'5513' THEN N'תקי"ג'
      WHEN N'5514' THEN N'תקי"ד'
      WHEN N'5515' THEN N'תקט"ו'
      WHEN N'5516' THEN N'תקט"ז'
      WHEN N'5517' THEN N'תקי"ז'
      WHEN N'5518' THEN N'תקי"ח'
      WHEN N'5519' THEN N'תקי"ט'
      WHEN N'5520' THEN N'תק"כ'
      WHEN N'5521' THEN N'תקכ"א'
      WHEN N'5522' THEN N'תקכ"ב'
      WHEN N'5523' THEN N'תקכ"ג'
      WHEN N'5524' THEN N'תקכ"ד'
      WHEN N'5525' THEN N'תקכ"ה'
      WHEN N'5526' THEN N'תקכ"ו'
      WHEN N'5527' THEN N'תקכ"ז'
      WHEN N'5528' THEN N'תקכ"ח'
      WHEN N'5529' THEN N'תקכ"ט'
      WHEN N'5530' THEN N'תק"ל'
      WHEN N'5531' THEN N'תקל"א'
      WHEN N'5532' THEN N'תקל"ב'
      WHEN N'5533' THEN N'תקל"ג'
      WHEN N'5534' THEN N'תקל"ד'
      WHEN N'5535' THEN N'תקל"ה'
      WHEN N'5536' THEN N'תקל"ו'
      WHEN N'5537' THEN N'תקל"ז'
      WHEN N'5538' THEN N'תקל"ח'
      WHEN N'5539' THEN N'תקל"ט'
      WHEN N'5540' THEN N'תק"מ'
      WHEN N'5541' THEN N'תקמ"א'
      WHEN N'5542' THEN N'תקמ"ב'
      WHEN N'5543' THEN N'תקמ"ג'
      WHEN N'5544' THEN N'תקמ"ד'
      WHEN N'5545' THEN N'תקמ"ה'
      WHEN N'5546' THEN N'תקמ"ו'
      WHEN N'5547' THEN N'תקמ"ז'
      WHEN N'5548' THEN N'תקמ"ח'
      WHEN N'5549' THEN N'תקמ"ט'
      WHEN N'5550' THEN N'תק"נ'
      WHEN N'5551' THEN N'תקנ"א'
      WHEN N'5552' THEN N'תקנ"ב'
      WHEN N'5553' THEN N'תקנ"ג'
      WHEN N'5554' THEN N'תקנ"ד'
      WHEN N'5555' THEN N'תקנ"ה'
      WHEN N'5556' THEN N'תקנ"ו'
      WHEN N'5557' THEN N'תקנ"ז'
      WHEN N'5558' THEN N'תקנ"ח'
      WHEN N'5559' THEN N'תקנ"ט'
      WHEN N'5560' THEN N'תק"ס'
      WHEN N'5561' THEN N'תקס"א'
      WHEN N'5562' THEN N'תקס"ב'
      WHEN N'5563' THEN N'תקס"ג'
      WHEN N'5564' THEN N'תקס"ד'
      WHEN N'5565' THEN N'תקס"ה'
      WHEN N'5566' THEN N'תקס"ו'
      WHEN N'5567' THEN N'תקס"ז'
      WHEN N'5568' THEN N'תקס"ח'
      WHEN N'5569' THEN N'תקס"ט'
      WHEN N'5570' THEN N'תק"ע'
      WHEN N'5571' THEN N'תקע"א'
      WHEN N'5572' THEN N'תקע"ב'
      WHEN N'5573' THEN N'תקע"ג'
      WHEN N'5574' THEN N'תקע"ד'
      WHEN N'5575' THEN N'תקע"ה'
      WHEN N'5576' THEN N'תקע"ו'
      WHEN N'5577' THEN N'תקע"ז'
      WHEN N'5578' THEN N'תקע"ח'
      WHEN N'5579' THEN N'תקע"ט'
      WHEN N'5580' THEN N'תק"פ'
      WHEN N'5581' THEN N'תקפ"א'
      WHEN N'5582' THEN N'תקפ"ב'
      WHEN N'5583' THEN N'תקפ"ג'
      WHEN N'5584' THEN N'תקפ"ד'
      WHEN N'5585' THEN N'תקפ"ה'
      WHEN N'5586' THEN N'תקפ"ו'
      WHEN N'5587' THEN N'תקפ"ז'
      WHEN N'5588' THEN N'תקפ"ח'
      WHEN N'5589' THEN N'תקפ"ט'
      WHEN N'5590' THEN N'תק"צ'
      WHEN N'5591' THEN N'תקצ"א'
      WHEN N'5592' THEN N'תקצ"ב'
      WHEN N'5593' THEN N'תקצ"ג'
      WHEN N'5594' THEN N'תקצ"ד'
      WHEN N'5595' THEN N'תקצ"ה'
      WHEN N'5596' THEN N'תקצ"ו'
      WHEN N'5597' THEN N'תקצ"ז'
      WHEN N'5598' THEN N'תקצ"ח'
      WHEN N'5599' THEN N'תקצ"ט'
      WHEN N'5600' THEN N'ת"ר'
      WHEN N'5601' THEN N'תר"א'
      WHEN N'5602' THEN N'תר"ב'
      WHEN N'5603' THEN N'תר"ג'
      WHEN N'5604' THEN N'תר"ד'
      WHEN N'5605' THEN N'תר"ה'
      WHEN N'5606' THEN N'תר"ו'
      WHEN N'5607' THEN N'תר"ז'
      WHEN N'5608' THEN N'תר"ח'
      WHEN N'5609' THEN N'תר"ט'
      WHEN N'5610' THEN N'תר"י'
      WHEN N'5611' THEN N'תרי"א'
      WHEN N'5612' THEN N'תרי"ב'
      WHEN N'5613' THEN N'תרי"ג'
      WHEN N'5614' THEN N'תרי"ד'
      WHEN N'5615' THEN N'תרט"ו'
      WHEN N'5616' THEN N'תרט"ז'
      WHEN N'5617' THEN N'תרי"ז'
      WHEN N'5618' THEN N'תרי"ח'
      WHEN N'5619' THEN N'תרי"ט'
      WHEN N'5620' THEN N'תר"כ'
      WHEN N'5621' THEN N'תרכ"א'
      WHEN N'5622' THEN N'תרכ"ב'
      WHEN N'5623' THEN N'תרכ"ג'
      WHEN N'5624' THEN N'תרכ"ד'
      WHEN N'5625' THEN N'תרכ"ה'
      WHEN N'5626' THEN N'תרכ"ו'
      WHEN N'5627' THEN N'תרכ"ז'
      WHEN N'5628' THEN N'תרכ"ח'
      WHEN N'5629' THEN N'תרכ"ט'
      WHEN N'5630' THEN N'תר"ל'
      WHEN N'5631' THEN N'תרל"א'
      WHEN N'5632' THEN N'תרל"ב'
      WHEN N'5633' THEN N'תרל"ג'
      WHEN N'5634' THEN N'תרל"ד'
      WHEN N'5635' THEN N'תרל"ה'
      WHEN N'5636' THEN N'תרל"ו'
      WHEN N'5637' THEN N'תרל"ז'
      WHEN N'5638' THEN N'תרל"ח'
      WHEN N'5639' THEN N'תרל"ט'
      WHEN N'5640' THEN N'תר"מ'
      WHEN N'5641' THEN N'תרמ"א'
      WHEN N'5642' THEN N'תרמ"ב'
      WHEN N'5643' THEN N'תרמ"ג'
      WHEN N'5644' THEN N'תרמ"ד'
      WHEN N'5645' THEN N'תרמ"ה'
      WHEN N'5646' THEN N'תרמ"ו'
      WHEN N'5647' THEN N'תרמ"ז'
      WHEN N'5648' THEN N'תרמ"ח'
      WHEN N'5649' THEN N'תרמ"ט'
      WHEN N'5650' THEN N'תר"נ'
      WHEN N'5651' THEN N'תרנ"א'
      WHEN N'5652' THEN N'תרנ"ב'
      WHEN N'5653' THEN N'תרנ"ג'
      WHEN N'5654' THEN N'תרנ"ד'
      WHEN N'5655' THEN N'תרנ"ה'
      WHEN N'5656' THEN N'תרנ"ו'
      WHEN N'5657' THEN N'תרנ"ז'
      WHEN N'5658' THEN N'תרנ"ח'
      WHEN N'5659' THEN N'תרנ"ט'
      WHEN N'5660' THEN N'תר"ס'
      WHEN N'5661' THEN N'תרס"א'
      WHEN N'5662' THEN N'תרס"ב'
      WHEN N'5663' THEN N'תרס"ג'
      WHEN N'5664' THEN N'תרס"ד'
      WHEN N'5665' THEN N'תרס"ה'
      WHEN N'5666' THEN N'תרס"ו'
      WHEN N'5667' THEN N'תרס"ז'
      WHEN N'5668' THEN N'תרס"ח'
      WHEN N'5669' THEN N'תרס"ט'
      WHEN N'5670' THEN N'תר"ע'
      WHEN N'5671' THEN N'תרע"א'
      WHEN N'5672' THEN N'תרע"ב'
      WHEN N'5673' THEN N'תרע"ג'
      WHEN N'5674' THEN N'תרע"ד'
      WHEN N'5675' THEN N'תרע"ה'
      WHEN N'5676' THEN N'תרע"ו'
      WHEN N'5677' THEN N'תרע"ז'
      WHEN N'5678' THEN N'תרע"ח'
      WHEN N'5679' THEN N'תרע"ט'
      WHEN N'5680' THEN N'תר"פ'
      WHEN N'5681' THEN N'תרפ"א'
      WHEN N'5682' THEN N'תרפ"ב'
      WHEN N'5683' THEN N'תרפ"ג'
      WHEN N'5684' THEN N'תרפ"ד'
      WHEN N'5685' THEN N'תרפ"ה'
      WHEN N'5686' THEN N'תרפ"ו'
      WHEN N'5687' THEN N'תרפ"ז'
      WHEN N'5688' THEN N'תרפ"ח'
      WHEN N'5689' THEN N'תרפ"ט'
      WHEN N'5690' THEN N'תר"צ'
      WHEN N'5691' THEN N'תרצ"א'
      WHEN N'5692' THEN N'תרצ"ב'
      WHEN N'5693' THEN N'תרצ"ג'
      WHEN N'5694' THEN N'תרצ"ד'
      WHEN N'5695' THEN N'תרצ"ה'
      WHEN N'5696' THEN N'תרצ"ו'
      WHEN N'5697' THEN N'תרצ"ז'
      WHEN N'5698' THEN N'תרצ"ח'
      WHEN N'5699' THEN N'תרצ"ט'
      WHEN N'5700' THEN N'ת"ש'
      WHEN N'5701' THEN N'תש"א'
      WHEN N'5702' THEN N'תש"ב'
      WHEN N'5703' THEN N'תש"ג'
      WHEN N'5704' THEN N'תש"ד'
      WHEN N'5705' THEN N'תש"ה'
      WHEN N'5706' THEN N'תש"ו'
      WHEN N'5707' THEN N'תש"ז'
      WHEN N'5708' THEN N'תש"ח'
      WHEN N'5709' THEN N'תש"ט'
      WHEN N'5710' THEN N'תש"י'
      WHEN N'5711' THEN N'תשי"א'
      WHEN N'5712' THEN N'תשי"ב'
      WHEN N'5713' THEN N'תשי"ג'
      WHEN N'5714' THEN N'תשי"ד'
      WHEN N'5715' THEN N'תשט"ו'
      WHEN N'5716' THEN N'תשט"ז'
      WHEN N'5717' THEN N'תשי"ז'
      WHEN N'5718' THEN N'תשי"ח'
      WHEN N'5719' THEN N'תשי"ט'
      WHEN N'5720' THEN N'תש"כ'
      WHEN N'5721' THEN N'תשכ"א'
      WHEN N'5722' THEN N'תשכ"ב'
      WHEN N'5723' THEN N'תשכ"ג'
      WHEN N'5724' THEN N'תשכ"ד'
      WHEN N'5725' THEN N'תשכ"ה'
      WHEN N'5726' THEN N'תשכ"ו'
      WHEN N'5727' THEN N'תשכ"ז'
      WHEN N'5728' THEN N'תשכ"ח'
      WHEN N'5729' THEN N'תשכ"ט'
      WHEN N'5730' THEN N'תש"ל'
      WHEN N'5731' THEN N'תשל"א'
      WHEN N'5732' THEN N'תשל"ב'
      WHEN N'5733' THEN N'תשל"ג'
      WHEN N'5734' THEN N'תשל"ד'
      WHEN N'5735' THEN N'תשל"ה'
      WHEN N'5736' THEN N'תשל"ו'
      WHEN N'5737' THEN N'תשל"ז'
      WHEN N'5738' THEN N'תשל"ח'
      WHEN N'5739' THEN N'תשל"ט'
      WHEN N'5740' THEN N'תש"מ'
      WHEN N'5741' THEN N'תשמ"א'
      WHEN N'5742' THEN N'תשמ"ב'
      WHEN N'5743' THEN N'תשמ"ג'
      WHEN N'5744' THEN N'תשמ"ד'
      WHEN N'5745' THEN N'תשמ"ה'
      WHEN N'5746' THEN N'תשמ"ו'
      WHEN N'5747' THEN N'תשמ"ז'
      WHEN N'5748' THEN N'תשמ"ח'
      WHEN N'5749' THEN N'תשמ"ט'
      WHEN N'5750' THEN N'תש"נ'
      WHEN N'5751' THEN N'תשנ"א'
      WHEN N'5752' THEN N'תשנ"ב'
      WHEN N'5753' THEN N'תשנ"ג'
      WHEN N'5754' THEN N'תשנ"ד'
      WHEN N'5755' THEN N'תשנ"ה'
      WHEN N'5756' THEN N'תשנ"ו'
      WHEN N'5757' THEN N'תשנ"ז'
      WHEN N'5758' THEN N'תשנ"ח'
      WHEN N'5759' THEN N'תשנ"ט'
      WHEN N'5760' THEN N'תש"ס'
      WHEN N'5761' THEN N'תשס"א'
      WHEN N'5762' THEN N'תשס"ב'
      WHEN N'5763' THEN N'תשס"ג'
      WHEN N'5764' THEN N'תשס"ד'
      WHEN N'5765' THEN N'תשס"ה'
      WHEN N'5766' THEN N'תשס"ו'
      WHEN N'5767' THEN N'תשס"ז'
      WHEN N'5768' THEN N'תשס"ח'
      WHEN N'5769' THEN N'תשס"ט'
      WHEN N'5770' THEN N'תש"ע'
      WHEN N'5771' THEN N'תשע"א'
      WHEN N'5772' THEN N'תשע"ב'
      WHEN N'5773' THEN N'תשע"ג'
      WHEN N'5774' THEN N'תשע"ד'
      WHEN N'5775' THEN N'תשע"ה'
      WHEN N'5776' THEN N'תשע"ו'
      WHEN N'5777' THEN N'תשע"ז'
      WHEN N'5778' THEN N'תשע"ח'
      WHEN N'5779' THEN N'תשע"ט'
      WHEN N'5780' THEN N'תש"פ'
      WHEN N'5781' THEN N'תשפ"א'
      WHEN N'5782' THEN N'תשפ"ב'
      WHEN N'5783' THEN N'תשפ"ג'
      WHEN N'5784' THEN N'תשפ"ד'
      WHEN N'5785' THEN N'תשפ"ה'
      WHEN N'5786' THEN N'תשפ"ו'
      WHEN N'5787' THEN N'תשפ"ז'
      WHEN N'5788' THEN N'תשפ"ח'
      WHEN N'5789' THEN N'תשפ"ט'
      WHEN N'5790' THEN N'תש"צ'
      WHEN N'5791' THEN N'תשצ"א'
      WHEN N'5792' THEN N'תשצ"ב'
      WHEN N'5793' THEN N'תשצ"ג'
      WHEN N'5794' THEN N'תשצ"ד'
      WHEN N'5795' THEN N'תשצ"ה'
      WHEN N'5796' THEN N'תשצ"ו'
      WHEN N'5797' THEN N'תשצ"ז'
      WHEN N'5798' THEN N'תשצ"ח'
      WHEN N'5799' THEN N'תשצ"ט'
      WHEN N'5800' THEN N'ת"ת'
      WHEN N'5801' THEN N'תת"א'
      WHEN N'5802' THEN N'תת"ב'
      WHEN N'5803' THEN N'תת"ג'
      WHEN N'5804' THEN N'תת"ד'
      WHEN N'5805' THEN N'תת"ה'
      WHEN N'5806' THEN N'תת"ו'
      WHEN N'5807' THEN N'תת"ז'
      WHEN N'5808' THEN N'תת"ח'
      WHEN N'5809' THEN N'תת"ט'
      WHEN N'5810' THEN N'תת"י'
      WHEN N'5811' THEN N'תתי"א'
      WHEN N'5812' THEN N'תתי"ב'
      WHEN N'5813' THEN N'תתי"ג'
      WHEN N'5814' THEN N'תתי"ד'
      WHEN N'5815' THEN N'תתט"ו'
      WHEN N'5816' THEN N'תתט"ז'
      WHEN N'5817' THEN N'תתי"ז'
      WHEN N'5818' THEN N'תתי"ח'
      WHEN N'5819' THEN N'תתי"ט'
      WHEN N'5820' THEN N'תת"כ'
      WHEN N'5821' THEN N'תתכ"א'
      WHEN N'5822' THEN N'תתכ"ב'
      WHEN N'5823' THEN N'תתכ"ג'
      WHEN N'5824' THEN N'תתכ"ד'
      WHEN N'5825' THEN N'תתכ"ה'
      WHEN N'5826' THEN N'תתכ"ו'
      WHEN N'5827' THEN N'תתכ"ז'
      WHEN N'5828' THEN N'תתכ"ח'
      WHEN N'5829' THEN N'תתכ"ט'
      WHEN N'5830' THEN N'תת"ל'
      WHEN N'5831' THEN N'תתל"א'
      WHEN N'5832' THEN N'תתל"ב'
      WHEN N'5833' THEN N'תתל"ג'
      WHEN N'5834' THEN N'תתל"ד'
      WHEN N'5835' THEN N'תתל"ה'
      WHEN N'5836' THEN N'תתל"ו'
      WHEN N'5837' THEN N'תתל"ז'
      WHEN N'5838' THEN N'תתל"ח'
      WHEN N'5839' THEN N'תתל"ט'
      WHEN N'5840' THEN N'תת"מ'
      WHEN N'5841' THEN N'תתמ"א'
      WHEN N'5842' THEN N'תתמ"ב'
      WHEN N'5843' THEN N'תתמ"ג'
      WHEN N'5844' THEN N'תתמ"ד'
      WHEN N'5845' THEN N'תתמ"ה'
      WHEN N'5846' THEN N'תתמ"ו'
      WHEN N'5847' THEN N'תתמ"ז'
      WHEN N'5848' THEN N'תתמ"ח'
      WHEN N'5849' THEN N'תתמ"ט'
      WHEN N'5850' THEN N'תת"נ'
      WHEN N'5851' THEN N'תתנ"א'
      WHEN N'5852' THEN N'תתנ"ב'
      WHEN N'5853' THEN N'תתנ"ג'
      WHEN N'5854' THEN N'תתנ"ד'
      WHEN N'5855' THEN N'תתנ"ה'
      WHEN N'5856' THEN N'תתנ"ו'
      WHEN N'5857' THEN N'תתנ"ז'
      WHEN N'5858' THEN N'תתנ"ח'
      WHEN N'5859' THEN N'תתנ"ט'
      WHEN N'5860' THEN N'תת"ס'
      WHEN N'5861' THEN N'תתס"א'
      WHEN N'5862' THEN N'תתס"ב'
      WHEN N'5863' THEN N'תתס"ג'
      WHEN N'5864' THEN N'תתס"ד'
      WHEN N'5865' THEN N'תתס"ה'
      WHEN N'5866' THEN N'תתס"ו'
      WHEN N'5867' THEN N'תתס"ז'
      WHEN N'5868' THEN N'תתס"ח'
      WHEN N'5869' THEN N'תתס"ט'
      WHEN N'5870' THEN N'תת"ע'
      WHEN N'5871' THEN N'תתע"א'
      WHEN N'5872' THEN N'תתע"ב'
      WHEN N'5873' THEN N'תתע"ג'
      WHEN N'5874' THEN N'תתע"ד'
      WHEN N'5875' THEN N'תתע"ה'
      WHEN N'5876' THEN N'תתע"ו'
      WHEN N'5877' THEN N'תתע"ז'
      WHEN N'5878' THEN N'תתע"ח'
      WHEN N'5879' THEN N'תתע"ט'
      WHEN N'5880' THEN N'תת"פ'
      WHEN N'5881' THEN N'תתפ"א'
      WHEN N'5882' THEN N'תתפ"ב'
      WHEN N'5883' THEN N'תתפ"ג'
      WHEN N'5884' THEN N'תתפ"ד'
      WHEN N'5885' THEN N'תתפ"ה'
      WHEN N'5886' THEN N'תתפ"ו'
      WHEN N'5887' THEN N'תתפ"ז'
      WHEN N'5888' THEN N'תתפ"ח'
      WHEN N'5889' THEN N'תתפ"ט'
      WHEN N'5890' THEN N'תת"צ'
      WHEN N'5891' THEN N'תתצ"א'
      WHEN N'5892' THEN N'תתצ"ב'
      WHEN N'5893' THEN N'תתצ"ג'
      WHEN N'5894' THEN N'תתצ"ד'
      WHEN N'5895' THEN N'תתצ"ה'
      WHEN N'5896' THEN N'תתצ"ו'
      WHEN N'5897' THEN N'תתצ"ז'
      WHEN N'5898' THEN N'תתצ"ח'
      WHEN N'5899' THEN N'תתצ"ט'
      WHEN N'5900' THEN N'תת"ק'
      WHEN N'5901' THEN N'תתק"א'
      WHEN N'5902' THEN N'תתק"ב'
      WHEN N'5903' THEN N'תתק"ג'
      WHEN N'5904' THEN N'תתק"ד'
      WHEN N'5905' THEN N'תתק"ה'
      WHEN N'5906' THEN N'תתק"ו'
      WHEN N'5907' THEN N'תתק"ז'
      WHEN N'5908' THEN N'תתק"ח'
      WHEN N'5909' THEN N'תתק"ט'
      WHEN N'5910' THEN N'תתק"י'
      WHEN N'5911' THEN N'תתקי"א'
      WHEN N'5912' THEN N'תתקי"ב'
      WHEN N'5913' THEN N'תתקי"ג'
      WHEN N'5914' THEN N'תתקי"ד'
      WHEN N'5915' THEN N'תתקט"ו'
      WHEN N'5916' THEN N'תתקט"ז'
      WHEN N'5917' THEN N'תתקי"ז'
      WHEN N'5918' THEN N'תתקי"ח'
      WHEN N'5919' THEN N'תתקי"ט'
      WHEN N'5920' THEN N'תתק"כ'
      WHEN N'5921' THEN N'תתקכ"א'
      WHEN N'5922' THEN N'תתקכ"ב'
      WHEN N'5923' THEN N'תתקכ"ג'
      WHEN N'5924' THEN N'תתקכ"ד'
      WHEN N'5925' THEN N'תתקכ"ה'
      WHEN N'5926' THEN N'תתקכ"ו'
      WHEN N'5927' THEN N'תתקכ"ז'
      WHEN N'5928' THEN N'תתקכ"ח'
      WHEN N'5929' THEN N'תתקכ"ט'
      WHEN N'5930' THEN N'תתק"ל'
      WHEN N'5931' THEN N'תתקל"א'
      WHEN N'5932' THEN N'תתקל"ב'
      WHEN N'5933' THEN N'תתקל"ג'
      WHEN N'5934' THEN N'תתקל"ד'
      WHEN N'5935' THEN N'תתקל"ה'
      WHEN N'5936' THEN N'תתקל"ו'
      WHEN N'5937' THEN N'תתקל"ז'
      WHEN N'5938' THEN N'תתקל"ח'
      WHEN N'5939' THEN N'תתקל"ט'
      WHEN N'5940' THEN N'תתק"מ'
      WHEN N'5941' THEN N'תתקמ"א'
      WHEN N'5942' THEN N'תתקמ"ב'
      WHEN N'5943' THEN N'תתקמ"ג'
      WHEN N'5944' THEN N'תתקמ"ד'
      WHEN N'5945' THEN N'תתקמ"ה'
      WHEN N'5946' THEN N'תתקמ"ו'
      WHEN N'5947' THEN N'תתקמ"ז'
      WHEN N'5948' THEN N'תתקמ"ח'
      WHEN N'5949' THEN N'תתקמ"ט'
      WHEN N'5950' THEN N'תתק"נ'
      WHEN N'5951' THEN N'תתקנ"א'
      WHEN N'5952' THEN N'תתקנ"ב'
      WHEN N'5953' THEN N'תתקנ"ג'
      WHEN N'5954' THEN N'תתקנ"ד'
      WHEN N'5955' THEN N'תתקנ"ה'
      WHEN N'5956' THEN N'תתקנ"ו'
      WHEN N'5957' THEN N'תתקנ"ז'
      WHEN N'5958' THEN N'תתקנ"ח'
      WHEN N'5959' THEN N'תתקנ"ט'
      WHEN N'5960' THEN N'תתק"ס'
      WHEN N'5961' THEN N'תתקס"א'
      WHEN N'5962' THEN N'תתקס"ב'
      WHEN N'5963' THEN N'תתקס"ג'
      WHEN N'5964' THEN N'תתקס"ד'
      WHEN N'5965' THEN N'תתקס"ה'
      WHEN N'5966' THEN N'תתקס"ו'
      WHEN N'5967' THEN N'תתקס"ז'
      WHEN N'5968' THEN N'תתקס"ח'
      WHEN N'5969' THEN N'תתקס"ט'
      WHEN N'5970' THEN N'תתק"ע'
      WHEN N'5971' THEN N'תתקע"א'
      WHEN N'5972' THEN N'תתקע"ב'
      WHEN N'5973' THEN N'תתקע"ג'
      WHEN N'5974' THEN N'תתקע"ד'
      WHEN N'5975' THEN N'תתקע"ה'
      WHEN N'5976' THEN N'תתקע"ו'
      WHEN N'5977' THEN N'תתקע"ז'
      WHEN N'5978' THEN N'תתקע"ח'
      WHEN N'5979' THEN N'תתקע"ט'
      WHEN N'5980' THEN N'תתק"פ'
      WHEN N'5981' THEN N'תתקפ"א'
      WHEN N'5982' THEN N'תתקפ"ב'
      WHEN N'5983' THEN N'תתקפ"ג'
      WHEN N'5984' THEN N'תתקפ"ד'
      WHEN N'5985' THEN N'תתקפ"ה'
      WHEN N'5986' THEN N'תתקפ"ו'
      WHEN N'5987' THEN N'תתקפ"ז'
      WHEN N'5988' THEN N'תתקפ"ח'
      WHEN N'5989' THEN N'תתקפ"ט'
      WHEN N'5990' THEN N'תתק"צ'
      WHEN N'5991' THEN N'תתקצ"א'
      WHEN N'5992' THEN N'תתקצ"ב'
      WHEN N'5993' THEN N'תתקצ"ג'
      WHEN N'5994' THEN N'תתקצ"ד'
      WHEN N'5995' THEN N'תתקצ"ה'
      WHEN N'5996' THEN N'תתקצ"ו'
      WHEN N'5997' THEN N'תתקצ"ז'
      WHEN N'5998' THEN N'תתקצ"ח'
      WHEN N'5999' THEN N'תתקצ"ט'
      ELSE [שנת_לימודים]
    END;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260616232951_CanonicalizeStoredAcademicYears'
)
BEGIN
    UPDATE [קלט_עוקץ_חודשי_שורה]
    SET [שנת_לימודים] = LTRIM(RTRIM([שנת_לימודים]));

    UPDATE [קלט_עוקץ_חודשי_שורה]
    SET [שנת_לימודים] = CASE [שנת_לימודים]
      WHEN N'2000' THEN N'תש"ס'
      WHEN N'2001' THEN N'תשס"א'
      WHEN N'2002' THEN N'תשס"ב'
      WHEN N'2003' THEN N'תשס"ג'
      WHEN N'2004' THEN N'תשס"ד'
      WHEN N'2005' THEN N'תשס"ה'
      WHEN N'2006' THEN N'תשס"ו'
      WHEN N'2007' THEN N'תשס"ז'
      WHEN N'2008' THEN N'תשס"ח'
      WHEN N'2009' THEN N'תשס"ט'
      WHEN N'2010' THEN N'תש"ע'
      WHEN N'2011' THEN N'תשע"א'
      WHEN N'2012' THEN N'תשע"ב'
      WHEN N'2013' THEN N'תשע"ג'
      WHEN N'2014' THEN N'תשע"ד'
      WHEN N'2015' THEN N'תשע"ה'
      WHEN N'2016' THEN N'תשע"ו'
      WHEN N'2017' THEN N'תשע"ז'
      WHEN N'2018' THEN N'תשע"ח'
      WHEN N'2019' THEN N'תשע"ט'
      WHEN N'2020' THEN N'תש"פ'
      WHEN N'2021' THEN N'תשפ"א'
      WHEN N'2022' THEN N'תשפ"ב'
      WHEN N'2023' THEN N'תשפ"ג'
      WHEN N'2024' THEN N'תשפ"ד'
      WHEN N'2025' THEN N'תשפ"ה'
      WHEN N'2026' THEN N'תשפ"ו'
      WHEN N'2027' THEN N'תשפ"ז'
      WHEN N'2028' THEN N'תשפ"ח'
      WHEN N'2029' THEN N'תשפ"ט'
      WHEN N'2030' THEN N'תש"צ'
      WHEN N'2031' THEN N'תשצ"א'
      WHEN N'2032' THEN N'תשצ"ב'
      WHEN N'2033' THEN N'תשצ"ג'
      WHEN N'2034' THEN N'תשצ"ד'
      WHEN N'2035' THEN N'תשצ"ה'
      WHEN N'2036' THEN N'תשצ"ו'
      WHEN N'2037' THEN N'תשצ"ז'
      WHEN N'2038' THEN N'תשצ"ח'
      WHEN N'2039' THEN N'תשצ"ט'
      WHEN N'2040' THEN N'ת"ת'
      WHEN N'2041' THEN N'תת"א'
      WHEN N'2042' THEN N'תת"ב'
      WHEN N'2043' THEN N'תת"ג'
      WHEN N'2044' THEN N'תת"ד'
      WHEN N'2045' THEN N'תת"ה'
      WHEN N'2046' THEN N'תת"ו'
      WHEN N'2047' THEN N'תת"ז'
      WHEN N'2048' THEN N'תת"ח'
      WHEN N'2049' THEN N'תת"ט'
      WHEN N'2050' THEN N'תת"י'
      WHEN N'2051' THEN N'תתי"א'
      WHEN N'2052' THEN N'תתי"ב'
      WHEN N'2053' THEN N'תתי"ג'
      WHEN N'2054' THEN N'תתי"ד'
      WHEN N'2055' THEN N'תתט"ו'
      WHEN N'2056' THEN N'תתט"ז'
      WHEN N'2057' THEN N'תתי"ז'
      WHEN N'2058' THEN N'תתי"ח'
      WHEN N'2059' THEN N'תתי"ט'
      WHEN N'2060' THEN N'תת"כ'
      WHEN N'2061' THEN N'תתכ"א'
      WHEN N'2062' THEN N'תתכ"ב'
      WHEN N'2063' THEN N'תתכ"ג'
      WHEN N'2064' THEN N'תתכ"ד'
      WHEN N'2065' THEN N'תתכ"ה'
      WHEN N'2066' THEN N'תתכ"ו'
      WHEN N'2067' THEN N'תתכ"ז'
      WHEN N'2068' THEN N'תתכ"ח'
      WHEN N'2069' THEN N'תתכ"ט'
      WHEN N'2070' THEN N'תת"ל'
      WHEN N'2071' THEN N'תתל"א'
      WHEN N'2072' THEN N'תתל"ב'
      WHEN N'2073' THEN N'תתל"ג'
      WHEN N'2074' THEN N'תתל"ד'
      WHEN N'2075' THEN N'תתל"ה'
      WHEN N'2076' THEN N'תתל"ו'
      WHEN N'2077' THEN N'תתל"ז'
      WHEN N'2078' THEN N'תתל"ח'
      WHEN N'2079' THEN N'תתל"ט'
      WHEN N'2080' THEN N'תת"מ'
      WHEN N'2081' THEN N'תתמ"א'
      WHEN N'2082' THEN N'תתמ"ב'
      WHEN N'2083' THEN N'תתמ"ג'
      WHEN N'2084' THEN N'תתמ"ד'
      WHEN N'2085' THEN N'תתמ"ה'
      WHEN N'2086' THEN N'תתמ"ו'
      WHEN N'2087' THEN N'תתמ"ז'
      WHEN N'2088' THEN N'תתמ"ח'
      WHEN N'2089' THEN N'תתמ"ט'
      WHEN N'2090' THEN N'תת"נ'
      WHEN N'2091' THEN N'תתנ"א'
      WHEN N'2092' THEN N'תתנ"ב'
      WHEN N'2093' THEN N'תתנ"ג'
      WHEN N'2094' THEN N'תתנ"ד'
      WHEN N'2095' THEN N'תתנ"ה'
      WHEN N'2096' THEN N'תתנ"ו'
      WHEN N'2097' THEN N'תתנ"ז'
      WHEN N'2098' THEN N'תתנ"ח'
      WHEN N'2099' THEN N'תתנ"ט'
      WHEN N'2100' THEN N'תת"ס'
      WHEN N'2101' THEN N'תתס"א'
      WHEN N'2102' THEN N'תתס"ב'
      WHEN N'2103' THEN N'תתס"ג'
      WHEN N'2104' THEN N'תתס"ד'
      WHEN N'2105' THEN N'תתס"ה'
      WHEN N'2106' THEN N'תתס"ו'
      WHEN N'2107' THEN N'תתס"ז'
      WHEN N'2108' THEN N'תתס"ח'
      WHEN N'2109' THEN N'תתס"ט'
      WHEN N'2110' THEN N'תת"ע'
      WHEN N'2111' THEN N'תתע"א'
      WHEN N'2112' THEN N'תתע"ב'
      WHEN N'2113' THEN N'תתע"ג'
      WHEN N'2114' THEN N'תתע"ד'
      WHEN N'2115' THEN N'תתע"ה'
      WHEN N'2116' THEN N'תתע"ו'
      WHEN N'2117' THEN N'תתע"ז'
      WHEN N'2118' THEN N'תתע"ח'
      WHEN N'2119' THEN N'תתע"ט'
      WHEN N'2120' THEN N'תת"פ'
      WHEN N'2121' THEN N'תתפ"א'
      WHEN N'2122' THEN N'תתפ"ב'
      WHEN N'2123' THEN N'תתפ"ג'
      WHEN N'2124' THEN N'תתפ"ד'
      WHEN N'2125' THEN N'תתפ"ה'
      WHEN N'2126' THEN N'תתפ"ו'
      WHEN N'2127' THEN N'תתפ"ז'
      WHEN N'2128' THEN N'תתפ"ח'
      WHEN N'2129' THEN N'תתפ"ט'
      WHEN N'2130' THEN N'תת"צ'
      WHEN N'2131' THEN N'תתצ"א'
      WHEN N'2132' THEN N'תתצ"ב'
      WHEN N'2133' THEN N'תתצ"ג'
      WHEN N'2134' THEN N'תתצ"ד'
      WHEN N'2135' THEN N'תתצ"ה'
      WHEN N'2136' THEN N'תתצ"ו'
      WHEN N'2137' THEN N'תתצ"ז'
      WHEN N'2138' THEN N'תתצ"ח'
      WHEN N'2139' THEN N'תתצ"ט'
      WHEN N'2140' THEN N'תת"ק'
      WHEN N'2141' THEN N'תתק"א'
      WHEN N'2142' THEN N'תתק"ב'
      WHEN N'2143' THEN N'תתק"ג'
      WHEN N'2144' THEN N'תתק"ד'
      WHEN N'2145' THEN N'תתק"ה'
      WHEN N'2146' THEN N'תתק"ו'
      WHEN N'2147' THEN N'תתק"ז'
      WHEN N'2148' THEN N'תתק"ח'
      WHEN N'2149' THEN N'תתק"ט'
      WHEN N'2150' THEN N'תתק"י'
      WHEN N'2151' THEN N'תתקי"א'
      WHEN N'2152' THEN N'תתקי"ב'
      WHEN N'2153' THEN N'תתקי"ג'
      WHEN N'2154' THEN N'תתקי"ד'
      WHEN N'2155' THEN N'תתקט"ו'
      WHEN N'2156' THEN N'תתקט"ז'
      WHEN N'2157' THEN N'תתקי"ז'
      WHEN N'2158' THEN N'תתקי"ח'
      WHEN N'2159' THEN N'תתקי"ט'
      WHEN N'2160' THEN N'תתק"כ'
      WHEN N'2161' THEN N'תתקכ"א'
      WHEN N'2162' THEN N'תתקכ"ב'
      WHEN N'2163' THEN N'תתקכ"ג'
      WHEN N'2164' THEN N'תתקכ"ד'
      WHEN N'2165' THEN N'תתקכ"ה'
      WHEN N'2166' THEN N'תתקכ"ו'
      WHEN N'2167' THEN N'תתקכ"ז'
      WHEN N'2168' THEN N'תתקכ"ח'
      WHEN N'2169' THEN N'תתקכ"ט'
      WHEN N'2170' THEN N'תתק"ל'
      WHEN N'2171' THEN N'תתקל"א'
      WHEN N'2172' THEN N'תתקל"ב'
      WHEN N'2173' THEN N'תתקל"ג'
      WHEN N'2174' THEN N'תתקל"ד'
      WHEN N'2175' THEN N'תתקל"ה'
      WHEN N'2176' THEN N'תתקל"ו'
      WHEN N'2177' THEN N'תתקל"ז'
      WHEN N'2178' THEN N'תתקל"ח'
      WHEN N'2179' THEN N'תתקל"ט'
      WHEN N'2180' THEN N'תתק"מ'
      WHEN N'2181' THEN N'תתקמ"א'
      WHEN N'2182' THEN N'תתקמ"ב'
      WHEN N'2183' THEN N'תתקמ"ג'
      WHEN N'2184' THEN N'תתקמ"ד'
      WHEN N'2185' THEN N'תתקמ"ה'
      WHEN N'2186' THEN N'תתקמ"ו'
      WHEN N'2187' THEN N'תתקמ"ז'
      WHEN N'2188' THEN N'תתקמ"ח'
      WHEN N'2189' THEN N'תתקמ"ט'
      WHEN N'2190' THEN N'תתק"נ'
      WHEN N'2191' THEN N'תתקנ"א'
      WHEN N'2192' THEN N'תתקנ"ב'
      WHEN N'2193' THEN N'תתקנ"ג'
      WHEN N'2194' THEN N'תתקנ"ד'
      WHEN N'2195' THEN N'תתקנ"ה'
      WHEN N'2196' THEN N'תתקנ"ו'
      WHEN N'2197' THEN N'תתקנ"ז'
      WHEN N'2198' THEN N'תתקנ"ח'
      WHEN N'2199' THEN N'תתקנ"ט'
      WHEN N'2200' THEN N'תתק"ס'
      WHEN N'5001' THEN N'א'''
      WHEN N'5002' THEN N'ב'''
      WHEN N'5003' THEN N'ג'''
      WHEN N'5004' THEN N'ד'''
      WHEN N'5005' THEN N'ה'''
      WHEN N'5006' THEN N'ו'''
      WHEN N'5007' THEN N'ז'''
      WHEN N'5008' THEN N'ח'''
      WHEN N'5009' THEN N'ט'''
      WHEN N'5010' THEN N'י'''
      WHEN N'5011' THEN N'י"א'
      WHEN N'5012' THEN N'י"ב'
      WHEN N'5013' THEN N'י"ג'
      WHEN N'5014' THEN N'י"ד'
      WHEN N'5015' THEN N'ט"ו'
      WHEN N'5016' THEN N'ט"ז'
      WHEN N'5017' THEN N'י"ז'
      WHEN N'5018' THEN N'י"ח'
      WHEN N'5019' THEN N'י"ט'
      WHEN N'5020' THEN N'כ'''
      WHEN N'5021' THEN N'כ"א'
      WHEN N'5022' THEN N'כ"ב'
      WHEN N'5023' THEN N'כ"ג'
      WHEN N'5024' THEN N'כ"ד'
      WHEN N'5025' THEN N'כ"ה'
      WHEN N'5026' THEN N'כ"ו'
      WHEN N'5027' THEN N'כ"ז'
      WHEN N'5028' THEN N'כ"ח'
      WHEN N'5029' THEN N'כ"ט'
      WHEN N'5030' THEN N'ל'''
      WHEN N'5031' THEN N'ל"א'
      WHEN N'5032' THEN N'ל"ב'
      WHEN N'5033' THEN N'ל"ג'
      WHEN N'5034' THEN N'ל"ד'
      WHEN N'5035' THEN N'ל"ה'
      WHEN N'5036' THEN N'ל"ו'
      WHEN N'5037' THEN N'ל"ז'
      WHEN N'5038' THEN N'ל"ח'
      WHEN N'5039' THEN N'ל"ט'
      WHEN N'5040' THEN N'מ'''
      WHEN N'5041' THEN N'מ"א'
      WHEN N'5042' THEN N'מ"ב'
      WHEN N'5043' THEN N'מ"ג'
      WHEN N'5044' THEN N'מ"ד'
      WHEN N'5045' THEN N'מ"ה'
      WHEN N'5046' THEN N'מ"ו'
      WHEN N'5047' THEN N'מ"ז'
      WHEN N'5048' THEN N'מ"ח'
      WHEN N'5049' THEN N'מ"ט'
      WHEN N'5050' THEN N'נ'''
      WHEN N'5051' THEN N'נ"א'
      WHEN N'5052' THEN N'נ"ב'
      WHEN N'5053' THEN N'נ"ג'
      WHEN N'5054' THEN N'נ"ד'
      WHEN N'5055' THEN N'נ"ה'
      WHEN N'5056' THEN N'נ"ו'
      WHEN N'5057' THEN N'נ"ז'
      WHEN N'5058' THEN N'נ"ח'
      WHEN N'5059' THEN N'נ"ט'
      WHEN N'5060' THEN N'ס'''
      WHEN N'5061' THEN N'ס"א'
      WHEN N'5062' THEN N'ס"ב'
      WHEN N'5063' THEN N'ס"ג'
      WHEN N'5064' THEN N'ס"ד'
      WHEN N'5065' THEN N'ס"ה'
      WHEN N'5066' THEN N'ס"ו'
      WHEN N'5067' THEN N'ס"ז'
      WHEN N'5068' THEN N'ס"ח'
      WHEN N'5069' THEN N'ס"ט'
      WHEN N'5070' THEN N'ע'''
      WHEN N'5071' THEN N'ע"א'
      WHEN N'5072' THEN N'ע"ב'
      WHEN N'5073' THEN N'ע"ג'
      WHEN N'5074' THEN N'ע"ד'
      WHEN N'5075' THEN N'ע"ה'
      WHEN N'5076' THEN N'ע"ו'
      WHEN N'5077' THEN N'ע"ז'
      WHEN N'5078' THEN N'ע"ח'
      WHEN N'5079' THEN N'ע"ט'
      WHEN N'5080' THEN N'פ'''
      WHEN N'5081' THEN N'פ"א'
      WHEN N'5082' THEN N'פ"ב'
      WHEN N'5083' THEN N'פ"ג'
      WHEN N'5084' THEN N'פ"ד'
      WHEN N'5085' THEN N'פ"ה'
      WHEN N'5086' THEN N'פ"ו'
      WHEN N'5087' THEN N'פ"ז'
      WHEN N'5088' THEN N'פ"ח'
      WHEN N'5089' THEN N'פ"ט'
      WHEN N'5090' THEN N'צ'''
      WHEN N'5091' THEN N'צ"א'
      WHEN N'5092' THEN N'צ"ב'
      WHEN N'5093' THEN N'צ"ג'
      WHEN N'5094' THEN N'צ"ד'
      WHEN N'5095' THEN N'צ"ה'
      WHEN N'5096' THEN N'צ"ו'
      WHEN N'5097' THEN N'צ"ז'
      WHEN N'5098' THEN N'צ"ח'
      WHEN N'5099' THEN N'צ"ט'
      WHEN N'5100' THEN N'ק'''
      WHEN N'5101' THEN N'ק"א'
      WHEN N'5102' THEN N'ק"ב'
      WHEN N'5103' THEN N'ק"ג'
      WHEN N'5104' THEN N'ק"ד'
      WHEN N'5105' THEN N'ק"ה'
      WHEN N'5106' THEN N'ק"ו'
      WHEN N'5107' THEN N'ק"ז'
      WHEN N'5108' THEN N'ק"ח'
      WHEN N'5109' THEN N'ק"ט'
      WHEN N'5110' THEN N'ק"י'
      WHEN N'5111' THEN N'קי"א'
      WHEN N'5112' THEN N'קי"ב'
      WHEN N'5113' THEN N'קי"ג'
      WHEN N'5114' THEN N'קי"ד'
      WHEN N'5115' THEN N'קט"ו'
      WHEN N'5116' THEN N'קט"ז'
      WHEN N'5117' THEN N'קי"ז'
      WHEN N'5118' THEN N'קי"ח'
      WHEN N'5119' THEN N'קי"ט'
      WHEN N'5120' THEN N'ק"כ'
      WHEN N'5121' THEN N'קכ"א'
      WHEN N'5122' THEN N'קכ"ב'
      WHEN N'5123' THEN N'קכ"ג'
      WHEN N'5124' THEN N'קכ"ד'
      WHEN N'5125' THEN N'קכ"ה'
      WHEN N'5126' THEN N'קכ"ו'
      WHEN N'5127' THEN N'קכ"ז'
      WHEN N'5128' THEN N'קכ"ח'
      WHEN N'5129' THEN N'קכ"ט'
      WHEN N'5130' THEN N'ק"ל'
      WHEN N'5131' THEN N'קל"א'
      WHEN N'5132' THEN N'קל"ב'
      WHEN N'5133' THEN N'קל"ג'
      WHEN N'5134' THEN N'קל"ד'
      WHEN N'5135' THEN N'קל"ה'
      WHEN N'5136' THEN N'קל"ו'
      WHEN N'5137' THEN N'קל"ז'
      WHEN N'5138' THEN N'קל"ח'
      WHEN N'5139' THEN N'קל"ט'
      WHEN N'5140' THEN N'ק"מ'
      WHEN N'5141' THEN N'קמ"א'
      WHEN N'5142' THEN N'קמ"ב'
      WHEN N'5143' THEN N'קמ"ג'
      WHEN N'5144' THEN N'קמ"ד'
      WHEN N'5145' THEN N'קמ"ה'
      WHEN N'5146' THEN N'קמ"ו'
      WHEN N'5147' THEN N'קמ"ז'
      WHEN N'5148' THEN N'קמ"ח'
      WHEN N'5149' THEN N'קמ"ט'
      WHEN N'5150' THEN N'ק"נ'
      WHEN N'5151' THEN N'קנ"א'
      WHEN N'5152' THEN N'קנ"ב'
      WHEN N'5153' THEN N'קנ"ג'
      WHEN N'5154' THEN N'קנ"ד'
      WHEN N'5155' THEN N'קנ"ה'
      WHEN N'5156' THEN N'קנ"ו'
      WHEN N'5157' THEN N'קנ"ז'
      WHEN N'5158' THEN N'קנ"ח'
      WHEN N'5159' THEN N'קנ"ט'
      WHEN N'5160' THEN N'ק"ס'
      WHEN N'5161' THEN N'קס"א'
      WHEN N'5162' THEN N'קס"ב'
      WHEN N'5163' THEN N'קס"ג'
      WHEN N'5164' THEN N'קס"ד'
      WHEN N'5165' THEN N'קס"ה'
      WHEN N'5166' THEN N'קס"ו'
      WHEN N'5167' THEN N'קס"ז'
      WHEN N'5168' THEN N'קס"ח'
      WHEN N'5169' THEN N'קס"ט'
      WHEN N'5170' THEN N'ק"ע'
      WHEN N'5171' THEN N'קע"א'
      WHEN N'5172' THEN N'קע"ב'
      WHEN N'5173' THEN N'קע"ג'
      WHEN N'5174' THEN N'קע"ד'
      WHEN N'5175' THEN N'קע"ה'
      WHEN N'5176' THEN N'קע"ו'
      WHEN N'5177' THEN N'קע"ז'
      WHEN N'5178' THEN N'קע"ח'
      WHEN N'5179' THEN N'קע"ט'
      WHEN N'5180' THEN N'ק"פ'
      WHEN N'5181' THEN N'קפ"א'
      WHEN N'5182' THEN N'קפ"ב'
      WHEN N'5183' THEN N'קפ"ג'
      WHEN N'5184' THEN N'קפ"ד'
      WHEN N'5185' THEN N'קפ"ה'
      WHEN N'5186' THEN N'קפ"ו'
      WHEN N'5187' THEN N'קפ"ז'
      WHEN N'5188' THEN N'קפ"ח'
      WHEN N'5189' THEN N'קפ"ט'
      WHEN N'5190' THEN N'ק"צ'
      WHEN N'5191' THEN N'קצ"א'
      WHEN N'5192' THEN N'קצ"ב'
      WHEN N'5193' THEN N'קצ"ג'
      WHEN N'5194' THEN N'קצ"ד'
      WHEN N'5195' THEN N'קצ"ה'
      WHEN N'5196' THEN N'קצ"ו'
      WHEN N'5197' THEN N'קצ"ז'
      WHEN N'5198' THEN N'קצ"ח'
      WHEN N'5199' THEN N'קצ"ט'
      WHEN N'5200' THEN N'ר'''
      WHEN N'5201' THEN N'ר"א'
      WHEN N'5202' THEN N'ר"ב'
      WHEN N'5203' THEN N'ר"ג'
      WHEN N'5204' THEN N'ר"ד'
      WHEN N'5205' THEN N'ר"ה'
      WHEN N'5206' THEN N'ר"ו'
      WHEN N'5207' THEN N'ר"ז'
      WHEN N'5208' THEN N'ר"ח'
      WHEN N'5209' THEN N'ר"ט'
      WHEN N'5210' THEN N'ר"י'
      WHEN N'5211' THEN N'רי"א'
      WHEN N'5212' THEN N'רי"ב'
      WHEN N'5213' THEN N'רי"ג'
      WHEN N'5214' THEN N'רי"ד'
      WHEN N'5215' THEN N'רט"ו'
      WHEN N'5216' THEN N'רט"ז'
      WHEN N'5217' THEN N'רי"ז'
      WHEN N'5218' THEN N'רי"ח'
      WHEN N'5219' THEN N'רי"ט'
      WHEN N'5220' THEN N'ר"כ'
      WHEN N'5221' THEN N'רכ"א'
      WHEN N'5222' THEN N'רכ"ב'
      WHEN N'5223' THEN N'רכ"ג'
      WHEN N'5224' THEN N'רכ"ד'
      WHEN N'5225' THEN N'רכ"ה'
      WHEN N'5226' THEN N'רכ"ו'
      WHEN N'5227' THEN N'רכ"ז'
      WHEN N'5228' THEN N'רכ"ח'
      WHEN N'5229' THEN N'רכ"ט'
      WHEN N'5230' THEN N'ר"ל'
      WHEN N'5231' THEN N'רל"א'
      WHEN N'5232' THEN N'רל"ב'
      WHEN N'5233' THEN N'רל"ג'
      WHEN N'5234' THEN N'רל"ד'
      WHEN N'5235' THEN N'רל"ה'
      WHEN N'5236' THEN N'רל"ו'
      WHEN N'5237' THEN N'רל"ז'
      WHEN N'5238' THEN N'רל"ח'
      WHEN N'5239' THEN N'רל"ט'
      WHEN N'5240' THEN N'ר"מ'
      WHEN N'5241' THEN N'רמ"א'
      WHEN N'5242' THEN N'רמ"ב'
      WHEN N'5243' THEN N'רמ"ג'
      WHEN N'5244' THEN N'רמ"ד'
      WHEN N'5245' THEN N'רמ"ה'
      WHEN N'5246' THEN N'רמ"ו'
      WHEN N'5247' THEN N'רמ"ז'
      WHEN N'5248' THEN N'רמ"ח'
      WHEN N'5249' THEN N'רמ"ט'
      WHEN N'5250' THEN N'ר"נ'
      WHEN N'5251' THEN N'רנ"א'
      WHEN N'5252' THEN N'רנ"ב'
      WHEN N'5253' THEN N'רנ"ג'
      WHEN N'5254' THEN N'רנ"ד'
      WHEN N'5255' THEN N'רנ"ה'
      WHEN N'5256' THEN N'רנ"ו'
      WHEN N'5257' THEN N'רנ"ז'
      WHEN N'5258' THEN N'רנ"ח'
      WHEN N'5259' THEN N'רנ"ט'
      WHEN N'5260' THEN N'ר"ס'
      WHEN N'5261' THEN N'רס"א'
      WHEN N'5262' THEN N'רס"ב'
      WHEN N'5263' THEN N'רס"ג'
      WHEN N'5264' THEN N'רס"ד'
      WHEN N'5265' THEN N'רס"ה'
      WHEN N'5266' THEN N'רס"ו'
      WHEN N'5267' THEN N'רס"ז'
      WHEN N'5268' THEN N'רס"ח'
      WHEN N'5269' THEN N'רס"ט'
      WHEN N'5270' THEN N'ר"ע'
      WHEN N'5271' THEN N'רע"א'
      WHEN N'5272' THEN N'רע"ב'
      WHEN N'5273' THEN N'רע"ג'
      WHEN N'5274' THEN N'רע"ד'
      WHEN N'5275' THEN N'רע"ה'
      WHEN N'5276' THEN N'רע"ו'
      WHEN N'5277' THEN N'רע"ז'
      WHEN N'5278' THEN N'רע"ח'
      WHEN N'5279' THEN N'רע"ט'
      WHEN N'5280' THEN N'ר"פ'
      WHEN N'5281' THEN N'רפ"א'
      WHEN N'5282' THEN N'רפ"ב'
      WHEN N'5283' THEN N'רפ"ג'
      WHEN N'5284' THEN N'רפ"ד'
      WHEN N'5285' THEN N'רפ"ה'
      WHEN N'5286' THEN N'רפ"ו'
      WHEN N'5287' THEN N'רפ"ז'
      WHEN N'5288' THEN N'רפ"ח'
      WHEN N'5289' THEN N'רפ"ט'
      WHEN N'5290' THEN N'ר"צ'
      WHEN N'5291' THEN N'רצ"א'
      WHEN N'5292' THEN N'רצ"ב'
      WHEN N'5293' THEN N'רצ"ג'
      WHEN N'5294' THEN N'רצ"ד'
      WHEN N'5295' THEN N'רצ"ה'
      WHEN N'5296' THEN N'רצ"ו'
      WHEN N'5297' THEN N'רצ"ז'
      WHEN N'5298' THEN N'רצ"ח'
      WHEN N'5299' THEN N'רצ"ט'
      WHEN N'5300' THEN N'ש'''
      WHEN N'5301' THEN N'ש"א'
      WHEN N'5302' THEN N'ש"ב'
      WHEN N'5303' THEN N'ש"ג'
      WHEN N'5304' THEN N'ש"ד'
      WHEN N'5305' THEN N'ש"ה'
      WHEN N'5306' THEN N'ש"ו'
      WHEN N'5307' THEN N'ש"ז'
      WHEN N'5308' THEN N'ש"ח'
      WHEN N'5309' THEN N'ש"ט'
      WHEN N'5310' THEN N'ש"י'
      WHEN N'5311' THEN N'שי"א'
      WHEN N'5312' THEN N'שי"ב'
      WHEN N'5313' THEN N'שי"ג'
      WHEN N'5314' THEN N'שי"ד'
      WHEN N'5315' THEN N'שט"ו'
      WHEN N'5316' THEN N'שט"ז'
      WHEN N'5317' THEN N'שי"ז'
      WHEN N'5318' THEN N'שי"ח'
      WHEN N'5319' THEN N'שי"ט'
      WHEN N'5320' THEN N'ש"כ'
      WHEN N'5321' THEN N'שכ"א'
      WHEN N'5322' THEN N'שכ"ב'
      WHEN N'5323' THEN N'שכ"ג'
      WHEN N'5324' THEN N'שכ"ד'
      WHEN N'5325' THEN N'שכ"ה'
      WHEN N'5326' THEN N'שכ"ו'
      WHEN N'5327' THEN N'שכ"ז'
      WHEN N'5328' THEN N'שכ"ח'
      WHEN N'5329' THEN N'שכ"ט'
      WHEN N'5330' THEN N'ש"ל'
      WHEN N'5331' THEN N'של"א'
      WHEN N'5332' THEN N'של"ב'
      WHEN N'5333' THEN N'של"ג'
      WHEN N'5334' THEN N'של"ד'
      WHEN N'5335' THEN N'של"ה'
      WHEN N'5336' THEN N'של"ו'
      WHEN N'5337' THEN N'של"ז'
      WHEN N'5338' THEN N'של"ח'
      WHEN N'5339' THEN N'של"ט'
      WHEN N'5340' THEN N'ש"מ'
      WHEN N'5341' THEN N'שמ"א'
      WHEN N'5342' THEN N'שמ"ב'
      WHEN N'5343' THEN N'שמ"ג'
      WHEN N'5344' THEN N'שמ"ד'
      WHEN N'5345' THEN N'שמ"ה'
      WHEN N'5346' THEN N'שמ"ו'
      WHEN N'5347' THEN N'שמ"ז'
      WHEN N'5348' THEN N'שמ"ח'
      WHEN N'5349' THEN N'שמ"ט'
      WHEN N'5350' THEN N'ש"נ'
      WHEN N'5351' THEN N'שנ"א'
      WHEN N'5352' THEN N'שנ"ב'
      WHEN N'5353' THEN N'שנ"ג'
      WHEN N'5354' THEN N'שנ"ד'
      WHEN N'5355' THEN N'שנ"ה'
      WHEN N'5356' THEN N'שנ"ו'
      WHEN N'5357' THEN N'שנ"ז'
      WHEN N'5358' THEN N'שנ"ח'
      WHEN N'5359' THEN N'שנ"ט'
      WHEN N'5360' THEN N'ש"ס'
      WHEN N'5361' THEN N'שס"א'
      WHEN N'5362' THEN N'שס"ב'
      WHEN N'5363' THEN N'שס"ג'
      WHEN N'5364' THEN N'שס"ד'
      WHEN N'5365' THEN N'שס"ה'
      WHEN N'5366' THEN N'שס"ו'
      WHEN N'5367' THEN N'שס"ז'
      WHEN N'5368' THEN N'שס"ח'
      WHEN N'5369' THEN N'שס"ט'
      WHEN N'5370' THEN N'ש"ע'
      WHEN N'5371' THEN N'שע"א'
      WHEN N'5372' THEN N'שע"ב'
      WHEN N'5373' THEN N'שע"ג'
      WHEN N'5374' THEN N'שע"ד'
      WHEN N'5375' THEN N'שע"ה'
      WHEN N'5376' THEN N'שע"ו'
      WHEN N'5377' THEN N'שע"ז'
      WHEN N'5378' THEN N'שע"ח'
      WHEN N'5379' THEN N'שע"ט'
      WHEN N'5380' THEN N'ש"פ'
      WHEN N'5381' THEN N'שפ"א'
      WHEN N'5382' THEN N'שפ"ב'
      WHEN N'5383' THEN N'שפ"ג'
      WHEN N'5384' THEN N'שפ"ד'
      WHEN N'5385' THEN N'שפ"ה'
      WHEN N'5386' THEN N'שפ"ו'
      WHEN N'5387' THEN N'שפ"ז'
      WHEN N'5388' THEN N'שפ"ח'
      WHEN N'5389' THEN N'שפ"ט'
      WHEN N'5390' THEN N'ש"צ'
      WHEN N'5391' THEN N'שצ"א'
      WHEN N'5392' THEN N'שצ"ב'
      WHEN N'5393' THEN N'שצ"ג'
      WHEN N'5394' THEN N'שצ"ד'
      WHEN N'5395' THEN N'שצ"ה'
      WHEN N'5396' THEN N'שצ"ו'
      WHEN N'5397' THEN N'שצ"ז'
      WHEN N'5398' THEN N'שצ"ח'
      WHEN N'5399' THEN N'שצ"ט'
      WHEN N'5400' THEN N'ת'''
      WHEN N'5401' THEN N'ת"א'
      WHEN N'5402' THEN N'ת"ב'
      WHEN N'5403' THEN N'ת"ג'
      WHEN N'5404' THEN N'ת"ד'
      WHEN N'5405' THEN N'ת"ה'
      WHEN N'5406' THEN N'ת"ו'
      WHEN N'5407' THEN N'ת"ז'
      WHEN N'5408' THEN N'ת"ח'
      WHEN N'5409' THEN N'ת"ט'
      WHEN N'5410' THEN N'ת"י'
      WHEN N'5411' THEN N'תי"א'
      WHEN N'5412' THEN N'תי"ב'
      WHEN N'5413' THEN N'תי"ג'
      WHEN N'5414' THEN N'תי"ד'
      WHEN N'5415' THEN N'תט"ו'
      WHEN N'5416' THEN N'תט"ז'
      WHEN N'5417' THEN N'תי"ז'
      WHEN N'5418' THEN N'תי"ח'
      WHEN N'5419' THEN N'תי"ט'
      WHEN N'5420' THEN N'ת"כ'
      WHEN N'5421' THEN N'תכ"א'
      WHEN N'5422' THEN N'תכ"ב'
      WHEN N'5423' THEN N'תכ"ג'
      WHEN N'5424' THEN N'תכ"ד'
      WHEN N'5425' THEN N'תכ"ה'
      WHEN N'5426' THEN N'תכ"ו'
      WHEN N'5427' THEN N'תכ"ז'
      WHEN N'5428' THEN N'תכ"ח'
      WHEN N'5429' THEN N'תכ"ט'
      WHEN N'5430' THEN N'ת"ל'
      WHEN N'5431' THEN N'תל"א'
      WHEN N'5432' THEN N'תל"ב'
      WHEN N'5433' THEN N'תל"ג'
      WHEN N'5434' THEN N'תל"ד'
      WHEN N'5435' THEN N'תל"ה'
      WHEN N'5436' THEN N'תל"ו'
      WHEN N'5437' THEN N'תל"ז'
      WHEN N'5438' THEN N'תל"ח'
      WHEN N'5439' THEN N'תל"ט'
      WHEN N'5440' THEN N'ת"מ'
      WHEN N'5441' THEN N'תמ"א'
      WHEN N'5442' THEN N'תמ"ב'
      WHEN N'5443' THEN N'תמ"ג'
      WHEN N'5444' THEN N'תמ"ד'
      WHEN N'5445' THEN N'תמ"ה'
      WHEN N'5446' THEN N'תמ"ו'
      WHEN N'5447' THEN N'תמ"ז'
      WHEN N'5448' THEN N'תמ"ח'
      WHEN N'5449' THEN N'תמ"ט'
      WHEN N'5450' THEN N'ת"נ'
      WHEN N'5451' THEN N'תנ"א'
      WHEN N'5452' THEN N'תנ"ב'
      WHEN N'5453' THEN N'תנ"ג'
      WHEN N'5454' THEN N'תנ"ד'
      WHEN N'5455' THEN N'תנ"ה'
      WHEN N'5456' THEN N'תנ"ו'
      WHEN N'5457' THEN N'תנ"ז'
      WHEN N'5458' THEN N'תנ"ח'
      WHEN N'5459' THEN N'תנ"ט'
      WHEN N'5460' THEN N'ת"ס'
      WHEN N'5461' THEN N'תס"א'
      WHEN N'5462' THEN N'תס"ב'
      WHEN N'5463' THEN N'תס"ג'
      WHEN N'5464' THEN N'תס"ד'
      WHEN N'5465' THEN N'תס"ה'
      WHEN N'5466' THEN N'תס"ו'
      WHEN N'5467' THEN N'תס"ז'
      WHEN N'5468' THEN N'תס"ח'
      WHEN N'5469' THEN N'תס"ט'
      WHEN N'5470' THEN N'ת"ע'
      WHEN N'5471' THEN N'תע"א'
      WHEN N'5472' THEN N'תע"ב'
      WHEN N'5473' THEN N'תע"ג'
      WHEN N'5474' THEN N'תע"ד'
      WHEN N'5475' THEN N'תע"ה'
      WHEN N'5476' THEN N'תע"ו'
      WHEN N'5477' THEN N'תע"ז'
      WHEN N'5478' THEN N'תע"ח'
      WHEN N'5479' THEN N'תע"ט'
      WHEN N'5480' THEN N'ת"פ'
      WHEN N'5481' THEN N'תפ"א'
      WHEN N'5482' THEN N'תפ"ב'
      WHEN N'5483' THEN N'תפ"ג'
      WHEN N'5484' THEN N'תפ"ד'
      WHEN N'5485' THEN N'תפ"ה'
      WHEN N'5486' THEN N'תפ"ו'
      WHEN N'5487' THEN N'תפ"ז'
      WHEN N'5488' THEN N'תפ"ח'
      WHEN N'5489' THEN N'תפ"ט'
      WHEN N'5490' THEN N'ת"צ'
      WHEN N'5491' THEN N'תצ"א'
      WHEN N'5492' THEN N'תצ"ב'
      WHEN N'5493' THEN N'תצ"ג'
      WHEN N'5494' THEN N'תצ"ד'
      WHEN N'5495' THEN N'תצ"ה'
      WHEN N'5496' THEN N'תצ"ו'
      WHEN N'5497' THEN N'תצ"ז'
      WHEN N'5498' THEN N'תצ"ח'
      WHEN N'5499' THEN N'תצ"ט'
      WHEN N'5500' THEN N'ת"ק'
      WHEN N'5501' THEN N'תק"א'
      WHEN N'5502' THEN N'תק"ב'
      WHEN N'5503' THEN N'תק"ג'
      WHEN N'5504' THEN N'תק"ד'
      WHEN N'5505' THEN N'תק"ה'
      WHEN N'5506' THEN N'תק"ו'
      WHEN N'5507' THEN N'תק"ז'
      WHEN N'5508' THEN N'תק"ח'
      WHEN N'5509' THEN N'תק"ט'
      WHEN N'5510' THEN N'תק"י'
      WHEN N'5511' THEN N'תקי"א'
      WHEN N'5512' THEN N'תקי"ב'
      WHEN N'5513' THEN N'תקי"ג'
      WHEN N'5514' THEN N'תקי"ד'
      WHEN N'5515' THEN N'תקט"ו'
      WHEN N'5516' THEN N'תקט"ז'
      WHEN N'5517' THEN N'תקי"ז'
      WHEN N'5518' THEN N'תקי"ח'
      WHEN N'5519' THEN N'תקי"ט'
      WHEN N'5520' THEN N'תק"כ'
      WHEN N'5521' THEN N'תקכ"א'
      WHEN N'5522' THEN N'תקכ"ב'
      WHEN N'5523' THEN N'תקכ"ג'
      WHEN N'5524' THEN N'תקכ"ד'
      WHEN N'5525' THEN N'תקכ"ה'
      WHEN N'5526' THEN N'תקכ"ו'
      WHEN N'5527' THEN N'תקכ"ז'
      WHEN N'5528' THEN N'תקכ"ח'
      WHEN N'5529' THEN N'תקכ"ט'
      WHEN N'5530' THEN N'תק"ל'
      WHEN N'5531' THEN N'תקל"א'
      WHEN N'5532' THEN N'תקל"ב'
      WHEN N'5533' THEN N'תקל"ג'
      WHEN N'5534' THEN N'תקל"ד'
      WHEN N'5535' THEN N'תקל"ה'
      WHEN N'5536' THEN N'תקל"ו'
      WHEN N'5537' THEN N'תקל"ז'
      WHEN N'5538' THEN N'תקל"ח'
      WHEN N'5539' THEN N'תקל"ט'
      WHEN N'5540' THEN N'תק"מ'
      WHEN N'5541' THEN N'תקמ"א'
      WHEN N'5542' THEN N'תקמ"ב'
      WHEN N'5543' THEN N'תקמ"ג'
      WHEN N'5544' THEN N'תקמ"ד'
      WHEN N'5545' THEN N'תקמ"ה'
      WHEN N'5546' THEN N'תקמ"ו'
      WHEN N'5547' THEN N'תקמ"ז'
      WHEN N'5548' THEN N'תקמ"ח'
      WHEN N'5549' THEN N'תקמ"ט'
      WHEN N'5550' THEN N'תק"נ'
      WHEN N'5551' THEN N'תקנ"א'
      WHEN N'5552' THEN N'תקנ"ב'
      WHEN N'5553' THEN N'תקנ"ג'
      WHEN N'5554' THEN N'תקנ"ד'
      WHEN N'5555' THEN N'תקנ"ה'
      WHEN N'5556' THEN N'תקנ"ו'
      WHEN N'5557' THEN N'תקנ"ז'
      WHEN N'5558' THEN N'תקנ"ח'
      WHEN N'5559' THEN N'תקנ"ט'
      WHEN N'5560' THEN N'תק"ס'
      WHEN N'5561' THEN N'תקס"א'
      WHEN N'5562' THEN N'תקס"ב'
      WHEN N'5563' THEN N'תקס"ג'
      WHEN N'5564' THEN N'תקס"ד'
      WHEN N'5565' THEN N'תקס"ה'
      WHEN N'5566' THEN N'תקס"ו'
      WHEN N'5567' THEN N'תקס"ז'
      WHEN N'5568' THEN N'תקס"ח'
      WHEN N'5569' THEN N'תקס"ט'
      WHEN N'5570' THEN N'תק"ע'
      WHEN N'5571' THEN N'תקע"א'
      WHEN N'5572' THEN N'תקע"ב'
      WHEN N'5573' THEN N'תקע"ג'
      WHEN N'5574' THEN N'תקע"ד'
      WHEN N'5575' THEN N'תקע"ה'
      WHEN N'5576' THEN N'תקע"ו'
      WHEN N'5577' THEN N'תקע"ז'
      WHEN N'5578' THEN N'תקע"ח'
      WHEN N'5579' THEN N'תקע"ט'
      WHEN N'5580' THEN N'תק"פ'
      WHEN N'5581' THEN N'תקפ"א'
      WHEN N'5582' THEN N'תקפ"ב'
      WHEN N'5583' THEN N'תקפ"ג'
      WHEN N'5584' THEN N'תקפ"ד'
      WHEN N'5585' THEN N'תקפ"ה'
      WHEN N'5586' THEN N'תקפ"ו'
      WHEN N'5587' THEN N'תקפ"ז'
      WHEN N'5588' THEN N'תקפ"ח'
      WHEN N'5589' THEN N'תקפ"ט'
      WHEN N'5590' THEN N'תק"צ'
      WHEN N'5591' THEN N'תקצ"א'
      WHEN N'5592' THEN N'תקצ"ב'
      WHEN N'5593' THEN N'תקצ"ג'
      WHEN N'5594' THEN N'תקצ"ד'
      WHEN N'5595' THEN N'תקצ"ה'
      WHEN N'5596' THEN N'תקצ"ו'
      WHEN N'5597' THEN N'תקצ"ז'
      WHEN N'5598' THEN N'תקצ"ח'
      WHEN N'5599' THEN N'תקצ"ט'
      WHEN N'5600' THEN N'ת"ר'
      WHEN N'5601' THEN N'תר"א'
      WHEN N'5602' THEN N'תר"ב'
      WHEN N'5603' THEN N'תר"ג'
      WHEN N'5604' THEN N'תר"ד'
      WHEN N'5605' THEN N'תר"ה'
      WHEN N'5606' THEN N'תר"ו'
      WHEN N'5607' THEN N'תר"ז'
      WHEN N'5608' THEN N'תר"ח'
      WHEN N'5609' THEN N'תר"ט'
      WHEN N'5610' THEN N'תר"י'
      WHEN N'5611' THEN N'תרי"א'
      WHEN N'5612' THEN N'תרי"ב'
      WHEN N'5613' THEN N'תרי"ג'
      WHEN N'5614' THEN N'תרי"ד'
      WHEN N'5615' THEN N'תרט"ו'
      WHEN N'5616' THEN N'תרט"ז'
      WHEN N'5617' THEN N'תרי"ז'
      WHEN N'5618' THEN N'תרי"ח'
      WHEN N'5619' THEN N'תרי"ט'
      WHEN N'5620' THEN N'תר"כ'
      WHEN N'5621' THEN N'תרכ"א'
      WHEN N'5622' THEN N'תרכ"ב'
      WHEN N'5623' THEN N'תרכ"ג'
      WHEN N'5624' THEN N'תרכ"ד'
      WHEN N'5625' THEN N'תרכ"ה'
      WHEN N'5626' THEN N'תרכ"ו'
      WHEN N'5627' THEN N'תרכ"ז'
      WHEN N'5628' THEN N'תרכ"ח'
      WHEN N'5629' THEN N'תרכ"ט'
      WHEN N'5630' THEN N'תר"ל'
      WHEN N'5631' THEN N'תרל"א'
      WHEN N'5632' THEN N'תרל"ב'
      WHEN N'5633' THEN N'תרל"ג'
      WHEN N'5634' THEN N'תרל"ד'
      WHEN N'5635' THEN N'תרל"ה'
      WHEN N'5636' THEN N'תרל"ו'
      WHEN N'5637' THEN N'תרל"ז'
      WHEN N'5638' THEN N'תרל"ח'
      WHEN N'5639' THEN N'תרל"ט'
      WHEN N'5640' THEN N'תר"מ'
      WHEN N'5641' THEN N'תרמ"א'
      WHEN N'5642' THEN N'תרמ"ב'
      WHEN N'5643' THEN N'תרמ"ג'
      WHEN N'5644' THEN N'תרמ"ד'
      WHEN N'5645' THEN N'תרמ"ה'
      WHEN N'5646' THEN N'תרמ"ו'
      WHEN N'5647' THEN N'תרמ"ז'
      WHEN N'5648' THEN N'תרמ"ח'
      WHEN N'5649' THEN N'תרמ"ט'
      WHEN N'5650' THEN N'תר"נ'
      WHEN N'5651' THEN N'תרנ"א'
      WHEN N'5652' THEN N'תרנ"ב'
      WHEN N'5653' THEN N'תרנ"ג'
      WHEN N'5654' THEN N'תרנ"ד'
      WHEN N'5655' THEN N'תרנ"ה'
      WHEN N'5656' THEN N'תרנ"ו'
      WHEN N'5657' THEN N'תרנ"ז'
      WHEN N'5658' THEN N'תרנ"ח'
      WHEN N'5659' THEN N'תרנ"ט'
      WHEN N'5660' THEN N'תר"ס'
      WHEN N'5661' THEN N'תרס"א'
      WHEN N'5662' THEN N'תרס"ב'
      WHEN N'5663' THEN N'תרס"ג'
      WHEN N'5664' THEN N'תרס"ד'
      WHEN N'5665' THEN N'תרס"ה'
      WHEN N'5666' THEN N'תרס"ו'
      WHEN N'5667' THEN N'תרס"ז'
      WHEN N'5668' THEN N'תרס"ח'
      WHEN N'5669' THEN N'תרס"ט'
      WHEN N'5670' THEN N'תר"ע'
      WHEN N'5671' THEN N'תרע"א'
      WHEN N'5672' THEN N'תרע"ב'
      WHEN N'5673' THEN N'תרע"ג'
      WHEN N'5674' THEN N'תרע"ד'
      WHEN N'5675' THEN N'תרע"ה'
      WHEN N'5676' THEN N'תרע"ו'
      WHEN N'5677' THEN N'תרע"ז'
      WHEN N'5678' THEN N'תרע"ח'
      WHEN N'5679' THEN N'תרע"ט'
      WHEN N'5680' THEN N'תר"פ'
      WHEN N'5681' THEN N'תרפ"א'
      WHEN N'5682' THEN N'תרפ"ב'
      WHEN N'5683' THEN N'תרפ"ג'
      WHEN N'5684' THEN N'תרפ"ד'
      WHEN N'5685' THEN N'תרפ"ה'
      WHEN N'5686' THEN N'תרפ"ו'
      WHEN N'5687' THEN N'תרפ"ז'
      WHEN N'5688' THEN N'תרפ"ח'
      WHEN N'5689' THEN N'תרפ"ט'
      WHEN N'5690' THEN N'תר"צ'
      WHEN N'5691' THEN N'תרצ"א'
      WHEN N'5692' THEN N'תרצ"ב'
      WHEN N'5693' THEN N'תרצ"ג'
      WHEN N'5694' THEN N'תרצ"ד'
      WHEN N'5695' THEN N'תרצ"ה'
      WHEN N'5696' THEN N'תרצ"ו'
      WHEN N'5697' THEN N'תרצ"ז'
      WHEN N'5698' THEN N'תרצ"ח'
      WHEN N'5699' THEN N'תרצ"ט'
      WHEN N'5700' THEN N'ת"ש'
      WHEN N'5701' THEN N'תש"א'
      WHEN N'5702' THEN N'תש"ב'
      WHEN N'5703' THEN N'תש"ג'
      WHEN N'5704' THEN N'תש"ד'
      WHEN N'5705' THEN N'תש"ה'
      WHEN N'5706' THEN N'תש"ו'
      WHEN N'5707' THEN N'תש"ז'
      WHEN N'5708' THEN N'תש"ח'
      WHEN N'5709' THEN N'תש"ט'
      WHEN N'5710' THEN N'תש"י'
      WHEN N'5711' THEN N'תשי"א'
      WHEN N'5712' THEN N'תשי"ב'
      WHEN N'5713' THEN N'תשי"ג'
      WHEN N'5714' THEN N'תשי"ד'
      WHEN N'5715' THEN N'תשט"ו'
      WHEN N'5716' THEN N'תשט"ז'
      WHEN N'5717' THEN N'תשי"ז'
      WHEN N'5718' THEN N'תשי"ח'
      WHEN N'5719' THEN N'תשי"ט'
      WHEN N'5720' THEN N'תש"כ'
      WHEN N'5721' THEN N'תשכ"א'
      WHEN N'5722' THEN N'תשכ"ב'
      WHEN N'5723' THEN N'תשכ"ג'
      WHEN N'5724' THEN N'תשכ"ד'
      WHEN N'5725' THEN N'תשכ"ה'
      WHEN N'5726' THEN N'תשכ"ו'
      WHEN N'5727' THEN N'תשכ"ז'
      WHEN N'5728' THEN N'תשכ"ח'
      WHEN N'5729' THEN N'תשכ"ט'
      WHEN N'5730' THEN N'תש"ל'
      WHEN N'5731' THEN N'תשל"א'
      WHEN N'5732' THEN N'תשל"ב'
      WHEN N'5733' THEN N'תשל"ג'
      WHEN N'5734' THEN N'תשל"ד'
      WHEN N'5735' THEN N'תשל"ה'
      WHEN N'5736' THEN N'תשל"ו'
      WHEN N'5737' THEN N'תשל"ז'
      WHEN N'5738' THEN N'תשל"ח'
      WHEN N'5739' THEN N'תשל"ט'
      WHEN N'5740' THEN N'תש"מ'
      WHEN N'5741' THEN N'תשמ"א'
      WHEN N'5742' THEN N'תשמ"ב'
      WHEN N'5743' THEN N'תשמ"ג'
      WHEN N'5744' THEN N'תשמ"ד'
      WHEN N'5745' THEN N'תשמ"ה'
      WHEN N'5746' THEN N'תשמ"ו'
      WHEN N'5747' THEN N'תשמ"ז'
      WHEN N'5748' THEN N'תשמ"ח'
      WHEN N'5749' THEN N'תשמ"ט'
      WHEN N'5750' THEN N'תש"נ'
      WHEN N'5751' THEN N'תשנ"א'
      WHEN N'5752' THEN N'תשנ"ב'
      WHEN N'5753' THEN N'תשנ"ג'
      WHEN N'5754' THEN N'תשנ"ד'
      WHEN N'5755' THEN N'תשנ"ה'
      WHEN N'5756' THEN N'תשנ"ו'
      WHEN N'5757' THEN N'תשנ"ז'
      WHEN N'5758' THEN N'תשנ"ח'
      WHEN N'5759' THEN N'תשנ"ט'
      WHEN N'5760' THEN N'תש"ס'
      WHEN N'5761' THEN N'תשס"א'
      WHEN N'5762' THEN N'תשס"ב'
      WHEN N'5763' THEN N'תשס"ג'
      WHEN N'5764' THEN N'תשס"ד'
      WHEN N'5765' THEN N'תשס"ה'
      WHEN N'5766' THEN N'תשס"ו'
      WHEN N'5767' THEN N'תשס"ז'
      WHEN N'5768' THEN N'תשס"ח'
      WHEN N'5769' THEN N'תשס"ט'
      WHEN N'5770' THEN N'תש"ע'
      WHEN N'5771' THEN N'תשע"א'
      WHEN N'5772' THEN N'תשע"ב'
      WHEN N'5773' THEN N'תשע"ג'
      WHEN N'5774' THEN N'תשע"ד'
      WHEN N'5775' THEN N'תשע"ה'
      WHEN N'5776' THEN N'תשע"ו'
      WHEN N'5777' THEN N'תשע"ז'
      WHEN N'5778' THEN N'תשע"ח'
      WHEN N'5779' THEN N'תשע"ט'
      WHEN N'5780' THEN N'תש"פ'
      WHEN N'5781' THEN N'תשפ"א'
      WHEN N'5782' THEN N'תשפ"ב'
      WHEN N'5783' THEN N'תשפ"ג'
      WHEN N'5784' THEN N'תשפ"ד'
      WHEN N'5785' THEN N'תשפ"ה'
      WHEN N'5786' THEN N'תשפ"ו'
      WHEN N'5787' THEN N'תשפ"ז'
      WHEN N'5788' THEN N'תשפ"ח'
      WHEN N'5789' THEN N'תשפ"ט'
      WHEN N'5790' THEN N'תש"צ'
      WHEN N'5791' THEN N'תשצ"א'
      WHEN N'5792' THEN N'תשצ"ב'
      WHEN N'5793' THEN N'תשצ"ג'
      WHEN N'5794' THEN N'תשצ"ד'
      WHEN N'5795' THEN N'תשצ"ה'
      WHEN N'5796' THEN N'תשצ"ו'
      WHEN N'5797' THEN N'תשצ"ז'
      WHEN N'5798' THEN N'תשצ"ח'
      WHEN N'5799' THEN N'תשצ"ט'
      WHEN N'5800' THEN N'ת"ת'
      WHEN N'5801' THEN N'תת"א'
      WHEN N'5802' THEN N'תת"ב'
      WHEN N'5803' THEN N'תת"ג'
      WHEN N'5804' THEN N'תת"ד'
      WHEN N'5805' THEN N'תת"ה'
      WHEN N'5806' THEN N'תת"ו'
      WHEN N'5807' THEN N'תת"ז'
      WHEN N'5808' THEN N'תת"ח'
      WHEN N'5809' THEN N'תת"ט'
      WHEN N'5810' THEN N'תת"י'
      WHEN N'5811' THEN N'תתי"א'
      WHEN N'5812' THEN N'תתי"ב'
      WHEN N'5813' THEN N'תתי"ג'
      WHEN N'5814' THEN N'תתי"ד'
      WHEN N'5815' THEN N'תתט"ו'
      WHEN N'5816' THEN N'תתט"ז'
      WHEN N'5817' THEN N'תתי"ז'
      WHEN N'5818' THEN N'תתי"ח'
      WHEN N'5819' THEN N'תתי"ט'
      WHEN N'5820' THEN N'תת"כ'
      WHEN N'5821' THEN N'תתכ"א'
      WHEN N'5822' THEN N'תתכ"ב'
      WHEN N'5823' THEN N'תתכ"ג'
      WHEN N'5824' THEN N'תתכ"ד'
      WHEN N'5825' THEN N'תתכ"ה'
      WHEN N'5826' THEN N'תתכ"ו'
      WHEN N'5827' THEN N'תתכ"ז'
      WHEN N'5828' THEN N'תתכ"ח'
      WHEN N'5829' THEN N'תתכ"ט'
      WHEN N'5830' THEN N'תת"ל'
      WHEN N'5831' THEN N'תתל"א'
      WHEN N'5832' THEN N'תתל"ב'
      WHEN N'5833' THEN N'תתל"ג'
      WHEN N'5834' THEN N'תתל"ד'
      WHEN N'5835' THEN N'תתל"ה'
      WHEN N'5836' THEN N'תתל"ו'
      WHEN N'5837' THEN N'תתל"ז'
      WHEN N'5838' THEN N'תתל"ח'
      WHEN N'5839' THEN N'תתל"ט'
      WHEN N'5840' THEN N'תת"מ'
      WHEN N'5841' THEN N'תתמ"א'
      WHEN N'5842' THEN N'תתמ"ב'
      WHEN N'5843' THEN N'תתמ"ג'
      WHEN N'5844' THEN N'תתמ"ד'
      WHEN N'5845' THEN N'תתמ"ה'
      WHEN N'5846' THEN N'תתמ"ו'
      WHEN N'5847' THEN N'תתמ"ז'
      WHEN N'5848' THEN N'תתמ"ח'
      WHEN N'5849' THEN N'תתמ"ט'
      WHEN N'5850' THEN N'תת"נ'
      WHEN N'5851' THEN N'תתנ"א'
      WHEN N'5852' THEN N'תתנ"ב'
      WHEN N'5853' THEN N'תתנ"ג'
      WHEN N'5854' THEN N'תתנ"ד'
      WHEN N'5855' THEN N'תתנ"ה'
      WHEN N'5856' THEN N'תתנ"ו'
      WHEN N'5857' THEN N'תתנ"ז'
      WHEN N'5858' THEN N'תתנ"ח'
      WHEN N'5859' THEN N'תתנ"ט'
      WHEN N'5860' THEN N'תת"ס'
      WHEN N'5861' THEN N'תתס"א'
      WHEN N'5862' THEN N'תתס"ב'
      WHEN N'5863' THEN N'תתס"ג'
      WHEN N'5864' THEN N'תתס"ד'
      WHEN N'5865' THEN N'תתס"ה'
      WHEN N'5866' THEN N'תתס"ו'
      WHEN N'5867' THEN N'תתס"ז'
      WHEN N'5868' THEN N'תתס"ח'
      WHEN N'5869' THEN N'תתס"ט'
      WHEN N'5870' THEN N'תת"ע'
      WHEN N'5871' THEN N'תתע"א'
      WHEN N'5872' THEN N'תתע"ב'
      WHEN N'5873' THEN N'תתע"ג'
      WHEN N'5874' THEN N'תתע"ד'
      WHEN N'5875' THEN N'תתע"ה'
      WHEN N'5876' THEN N'תתע"ו'
      WHEN N'5877' THEN N'תתע"ז'
      WHEN N'5878' THEN N'תתע"ח'
      WHEN N'5879' THEN N'תתע"ט'
      WHEN N'5880' THEN N'תת"פ'
      WHEN N'5881' THEN N'תתפ"א'
      WHEN N'5882' THEN N'תתפ"ב'
      WHEN N'5883' THEN N'תתפ"ג'
      WHEN N'5884' THEN N'תתפ"ד'
      WHEN N'5885' THEN N'תתפ"ה'
      WHEN N'5886' THEN N'תתפ"ו'
      WHEN N'5887' THEN N'תתפ"ז'
      WHEN N'5888' THEN N'תתפ"ח'
      WHEN N'5889' THEN N'תתפ"ט'
      WHEN N'5890' THEN N'תת"צ'
      WHEN N'5891' THEN N'תתצ"א'
      WHEN N'5892' THEN N'תתצ"ב'
      WHEN N'5893' THEN N'תתצ"ג'
      WHEN N'5894' THEN N'תתצ"ד'
      WHEN N'5895' THEN N'תתצ"ה'
      WHEN N'5896' THEN N'תתצ"ו'
      WHEN N'5897' THEN N'תתצ"ז'
      WHEN N'5898' THEN N'תתצ"ח'
      WHEN N'5899' THEN N'תתצ"ט'
      WHEN N'5900' THEN N'תת"ק'
      WHEN N'5901' THEN N'תתק"א'
      WHEN N'5902' THEN N'תתק"ב'
      WHEN N'5903' THEN N'תתק"ג'
      WHEN N'5904' THEN N'תתק"ד'
      WHEN N'5905' THEN N'תתק"ה'
      WHEN N'5906' THEN N'תתק"ו'
      WHEN N'5907' THEN N'תתק"ז'
      WHEN N'5908' THEN N'תתק"ח'
      WHEN N'5909' THEN N'תתק"ט'
      WHEN N'5910' THEN N'תתק"י'
      WHEN N'5911' THEN N'תתקי"א'
      WHEN N'5912' THEN N'תתקי"ב'
      WHEN N'5913' THEN N'תתקי"ג'
      WHEN N'5914' THEN N'תתקי"ד'
      WHEN N'5915' THEN N'תתקט"ו'
      WHEN N'5916' THEN N'תתקט"ז'
      WHEN N'5917' THEN N'תתקי"ז'
      WHEN N'5918' THEN N'תתקי"ח'
      WHEN N'5919' THEN N'תתקי"ט'
      WHEN N'5920' THEN N'תתק"כ'
      WHEN N'5921' THEN N'תתקכ"א'
      WHEN N'5922' THEN N'תתקכ"ב'
      WHEN N'5923' THEN N'תתקכ"ג'
      WHEN N'5924' THEN N'תתקכ"ד'
      WHEN N'5925' THEN N'תתקכ"ה'
      WHEN N'5926' THEN N'תתקכ"ו'
      WHEN N'5927' THEN N'תתקכ"ז'
      WHEN N'5928' THEN N'תתקכ"ח'
      WHEN N'5929' THEN N'תתקכ"ט'
      WHEN N'5930' THEN N'תתק"ל'
      WHEN N'5931' THEN N'תתקל"א'
      WHEN N'5932' THEN N'תתקל"ב'
      WHEN N'5933' THEN N'תתקל"ג'
      WHEN N'5934' THEN N'תתקל"ד'
      WHEN N'5935' THEN N'תתקל"ה'
      WHEN N'5936' THEN N'תתקל"ו'
      WHEN N'5937' THEN N'תתקל"ז'
      WHEN N'5938' THEN N'תתקל"ח'
      WHEN N'5939' THEN N'תתקל"ט'
      WHEN N'5940' THEN N'תתק"מ'
      WHEN N'5941' THEN N'תתקמ"א'
      WHEN N'5942' THEN N'תתקמ"ב'
      WHEN N'5943' THEN N'תתקמ"ג'
      WHEN N'5944' THEN N'תתקמ"ד'
      WHEN N'5945' THEN N'תתקמ"ה'
      WHEN N'5946' THEN N'תתקמ"ו'
      WHEN N'5947' THEN N'תתקמ"ז'
      WHEN N'5948' THEN N'תתקמ"ח'
      WHEN N'5949' THEN N'תתקמ"ט'
      WHEN N'5950' THEN N'תתק"נ'
      WHEN N'5951' THEN N'תתקנ"א'
      WHEN N'5952' THEN N'תתקנ"ב'
      WHEN N'5953' THEN N'תתקנ"ג'
      WHEN N'5954' THEN N'תתקנ"ד'
      WHEN N'5955' THEN N'תתקנ"ה'
      WHEN N'5956' THEN N'תתקנ"ו'
      WHEN N'5957' THEN N'תתקנ"ז'
      WHEN N'5958' THEN N'תתקנ"ח'
      WHEN N'5959' THEN N'תתקנ"ט'
      WHEN N'5960' THEN N'תתק"ס'
      WHEN N'5961' THEN N'תתקס"א'
      WHEN N'5962' THEN N'תתקס"ב'
      WHEN N'5963' THEN N'תתקס"ג'
      WHEN N'5964' THEN N'תתקס"ד'
      WHEN N'5965' THEN N'תתקס"ה'
      WHEN N'5966' THEN N'תתקס"ו'
      WHEN N'5967' THEN N'תתקס"ז'
      WHEN N'5968' THEN N'תתקס"ח'
      WHEN N'5969' THEN N'תתקס"ט'
      WHEN N'5970' THEN N'תתק"ע'
      WHEN N'5971' THEN N'תתקע"א'
      WHEN N'5972' THEN N'תתקע"ב'
      WHEN N'5973' THEN N'תתקע"ג'
      WHEN N'5974' THEN N'תתקע"ד'
      WHEN N'5975' THEN N'תתקע"ה'
      WHEN N'5976' THEN N'תתקע"ו'
      WHEN N'5977' THEN N'תתקע"ז'
      WHEN N'5978' THEN N'תתקע"ח'
      WHEN N'5979' THEN N'תתקע"ט'
      WHEN N'5980' THEN N'תתק"פ'
      WHEN N'5981' THEN N'תתקפ"א'
      WHEN N'5982' THEN N'תתקפ"ב'
      WHEN N'5983' THEN N'תתקפ"ג'
      WHEN N'5984' THEN N'תתקפ"ד'
      WHEN N'5985' THEN N'תתקפ"ה'
      WHEN N'5986' THEN N'תתקפ"ו'
      WHEN N'5987' THEN N'תתקפ"ז'
      WHEN N'5988' THEN N'תתקפ"ח'
      WHEN N'5989' THEN N'תתקפ"ט'
      WHEN N'5990' THEN N'תתק"צ'
      WHEN N'5991' THEN N'תתקצ"א'
      WHEN N'5992' THEN N'תתקצ"ב'
      WHEN N'5993' THEN N'תתקצ"ג'
      WHEN N'5994' THEN N'תתקצ"ד'
      WHEN N'5995' THEN N'תתקצ"ה'
      WHEN N'5996' THEN N'תתקצ"ו'
      WHEN N'5997' THEN N'תתקצ"ז'
      WHEN N'5998' THEN N'תתקצ"ח'
      WHEN N'5999' THEN N'תתקצ"ט'
      ELSE [שנת_לימודים]
    END;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260616232951_CanonicalizeStoredAcademicYears'
)
BEGIN
    UPDATE [דריסות_דוח_השוואה_שנתי]
    SET [שנת_לימודים] = LTRIM(RTRIM([שנת_לימודים]));

    UPDATE [דריסות_דוח_השוואה_שנתי]
    SET [שנת_לימודים] = CASE [שנת_לימודים]
      WHEN N'2000' THEN N'תש"ס'
      WHEN N'2001' THEN N'תשס"א'
      WHEN N'2002' THEN N'תשס"ב'
      WHEN N'2003' THEN N'תשס"ג'
      WHEN N'2004' THEN N'תשס"ד'
      WHEN N'2005' THEN N'תשס"ה'
      WHEN N'2006' THEN N'תשס"ו'
      WHEN N'2007' THEN N'תשס"ז'
      WHEN N'2008' THEN N'תשס"ח'
      WHEN N'2009' THEN N'תשס"ט'
      WHEN N'2010' THEN N'תש"ע'
      WHEN N'2011' THEN N'תשע"א'
      WHEN N'2012' THEN N'תשע"ב'
      WHEN N'2013' THEN N'תשע"ג'
      WHEN N'2014' THEN N'תשע"ד'
      WHEN N'2015' THEN N'תשע"ה'
      WHEN N'2016' THEN N'תשע"ו'
      WHEN N'2017' THEN N'תשע"ז'
      WHEN N'2018' THEN N'תשע"ח'
      WHEN N'2019' THEN N'תשע"ט'
      WHEN N'2020' THEN N'תש"פ'
      WHEN N'2021' THEN N'תשפ"א'
      WHEN N'2022' THEN N'תשפ"ב'
      WHEN N'2023' THEN N'תשפ"ג'
      WHEN N'2024' THEN N'תשפ"ד'
      WHEN N'2025' THEN N'תשפ"ה'
      WHEN N'2026' THEN N'תשפ"ו'
      WHEN N'2027' THEN N'תשפ"ז'
      WHEN N'2028' THEN N'תשפ"ח'
      WHEN N'2029' THEN N'תשפ"ט'
      WHEN N'2030' THEN N'תש"צ'
      WHEN N'2031' THEN N'תשצ"א'
      WHEN N'2032' THEN N'תשצ"ב'
      WHEN N'2033' THEN N'תשצ"ג'
      WHEN N'2034' THEN N'תשצ"ד'
      WHEN N'2035' THEN N'תשצ"ה'
      WHEN N'2036' THEN N'תשצ"ו'
      WHEN N'2037' THEN N'תשצ"ז'
      WHEN N'2038' THEN N'תשצ"ח'
      WHEN N'2039' THEN N'תשצ"ט'
      WHEN N'2040' THEN N'ת"ת'
      WHEN N'2041' THEN N'תת"א'
      WHEN N'2042' THEN N'תת"ב'
      WHEN N'2043' THEN N'תת"ג'
      WHEN N'2044' THEN N'תת"ד'
      WHEN N'2045' THEN N'תת"ה'
      WHEN N'2046' THEN N'תת"ו'
      WHEN N'2047' THEN N'תת"ז'
      WHEN N'2048' THEN N'תת"ח'
      WHEN N'2049' THEN N'תת"ט'
      WHEN N'2050' THEN N'תת"י'
      WHEN N'2051' THEN N'תתי"א'
      WHEN N'2052' THEN N'תתי"ב'
      WHEN N'2053' THEN N'תתי"ג'
      WHEN N'2054' THEN N'תתי"ד'
      WHEN N'2055' THEN N'תתט"ו'
      WHEN N'2056' THEN N'תתט"ז'
      WHEN N'2057' THEN N'תתי"ז'
      WHEN N'2058' THEN N'תתי"ח'
      WHEN N'2059' THEN N'תתי"ט'
      WHEN N'2060' THEN N'תת"כ'
      WHEN N'2061' THEN N'תתכ"א'
      WHEN N'2062' THEN N'תתכ"ב'
      WHEN N'2063' THEN N'תתכ"ג'
      WHEN N'2064' THEN N'תתכ"ד'
      WHEN N'2065' THEN N'תתכ"ה'
      WHEN N'2066' THEN N'תתכ"ו'
      WHEN N'2067' THEN N'תתכ"ז'
      WHEN N'2068' THEN N'תתכ"ח'
      WHEN N'2069' THEN N'תתכ"ט'
      WHEN N'2070' THEN N'תת"ל'
      WHEN N'2071' THEN N'תתל"א'
      WHEN N'2072' THEN N'תתל"ב'
      WHEN N'2073' THEN N'תתל"ג'
      WHEN N'2074' THEN N'תתל"ד'
      WHEN N'2075' THEN N'תתל"ה'
      WHEN N'2076' THEN N'תתל"ו'
      WHEN N'2077' THEN N'תתל"ז'
      WHEN N'2078' THEN N'תתל"ח'
      WHEN N'2079' THEN N'תתל"ט'
      WHEN N'2080' THEN N'תת"מ'
      WHEN N'2081' THEN N'תתמ"א'
      WHEN N'2082' THEN N'תתמ"ב'
      WHEN N'2083' THEN N'תתמ"ג'
      WHEN N'2084' THEN N'תתמ"ד'
      WHEN N'2085' THEN N'תתמ"ה'
      WHEN N'2086' THEN N'תתמ"ו'
      WHEN N'2087' THEN N'תתמ"ז'
      WHEN N'2088' THEN N'תתמ"ח'
      WHEN N'2089' THEN N'תתמ"ט'
      WHEN N'2090' THEN N'תת"נ'
      WHEN N'2091' THEN N'תתנ"א'
      WHEN N'2092' THEN N'תתנ"ב'
      WHEN N'2093' THEN N'תתנ"ג'
      WHEN N'2094' THEN N'תתנ"ד'
      WHEN N'2095' THEN N'תתנ"ה'
      WHEN N'2096' THEN N'תתנ"ו'
      WHEN N'2097' THEN N'תתנ"ז'
      WHEN N'2098' THEN N'תתנ"ח'
      WHEN N'2099' THEN N'תתנ"ט'
      WHEN N'2100' THEN N'תת"ס'
      WHEN N'2101' THEN N'תתס"א'
      WHEN N'2102' THEN N'תתס"ב'
      WHEN N'2103' THEN N'תתס"ג'
      WHEN N'2104' THEN N'תתס"ד'
      WHEN N'2105' THEN N'תתס"ה'
      WHEN N'2106' THEN N'תתס"ו'
      WHEN N'2107' THEN N'תתס"ז'
      WHEN N'2108' THEN N'תתס"ח'
      WHEN N'2109' THEN N'תתס"ט'
      WHEN N'2110' THEN N'תת"ע'
      WHEN N'2111' THEN N'תתע"א'
      WHEN N'2112' THEN N'תתע"ב'
      WHEN N'2113' THEN N'תתע"ג'
      WHEN N'2114' THEN N'תתע"ד'
      WHEN N'2115' THEN N'תתע"ה'
      WHEN N'2116' THEN N'תתע"ו'
      WHEN N'2117' THEN N'תתע"ז'
      WHEN N'2118' THEN N'תתע"ח'
      WHEN N'2119' THEN N'תתע"ט'
      WHEN N'2120' THEN N'תת"פ'
      WHEN N'2121' THEN N'תתפ"א'
      WHEN N'2122' THEN N'תתפ"ב'
      WHEN N'2123' THEN N'תתפ"ג'
      WHEN N'2124' THEN N'תתפ"ד'
      WHEN N'2125' THEN N'תתפ"ה'
      WHEN N'2126' THEN N'תתפ"ו'
      WHEN N'2127' THEN N'תתפ"ז'
      WHEN N'2128' THEN N'תתפ"ח'
      WHEN N'2129' THEN N'תתפ"ט'
      WHEN N'2130' THEN N'תת"צ'
      WHEN N'2131' THEN N'תתצ"א'
      WHEN N'2132' THEN N'תתצ"ב'
      WHEN N'2133' THEN N'תתצ"ג'
      WHEN N'2134' THEN N'תתצ"ד'
      WHEN N'2135' THEN N'תתצ"ה'
      WHEN N'2136' THEN N'תתצ"ו'
      WHEN N'2137' THEN N'תתצ"ז'
      WHEN N'2138' THEN N'תתצ"ח'
      WHEN N'2139' THEN N'תתצ"ט'
      WHEN N'2140' THEN N'תת"ק'
      WHEN N'2141' THEN N'תתק"א'
      WHEN N'2142' THEN N'תתק"ב'
      WHEN N'2143' THEN N'תתק"ג'
      WHEN N'2144' THEN N'תתק"ד'
      WHEN N'2145' THEN N'תתק"ה'
      WHEN N'2146' THEN N'תתק"ו'
      WHEN N'2147' THEN N'תתק"ז'
      WHEN N'2148' THEN N'תתק"ח'
      WHEN N'2149' THEN N'תתק"ט'
      WHEN N'2150' THEN N'תתק"י'
      WHEN N'2151' THEN N'תתקי"א'
      WHEN N'2152' THEN N'תתקי"ב'
      WHEN N'2153' THEN N'תתקי"ג'
      WHEN N'2154' THEN N'תתקי"ד'
      WHEN N'2155' THEN N'תתקט"ו'
      WHEN N'2156' THEN N'תתקט"ז'
      WHEN N'2157' THEN N'תתקי"ז'
      WHEN N'2158' THEN N'תתקי"ח'
      WHEN N'2159' THEN N'תתקי"ט'
      WHEN N'2160' THEN N'תתק"כ'
      WHEN N'2161' THEN N'תתקכ"א'
      WHEN N'2162' THEN N'תתקכ"ב'
      WHEN N'2163' THEN N'תתקכ"ג'
      WHEN N'2164' THEN N'תתקכ"ד'
      WHEN N'2165' THEN N'תתקכ"ה'
      WHEN N'2166' THEN N'תתקכ"ו'
      WHEN N'2167' THEN N'תתקכ"ז'
      WHEN N'2168' THEN N'תתקכ"ח'
      WHEN N'2169' THEN N'תתקכ"ט'
      WHEN N'2170' THEN N'תתק"ל'
      WHEN N'2171' THEN N'תתקל"א'
      WHEN N'2172' THEN N'תתקל"ב'
      WHEN N'2173' THEN N'תתקל"ג'
      WHEN N'2174' THEN N'תתקל"ד'
      WHEN N'2175' THEN N'תתקל"ה'
      WHEN N'2176' THEN N'תתקל"ו'
      WHEN N'2177' THEN N'תתקל"ז'
      WHEN N'2178' THEN N'תתקל"ח'
      WHEN N'2179' THEN N'תתקל"ט'
      WHEN N'2180' THEN N'תתק"מ'
      WHEN N'2181' THEN N'תתקמ"א'
      WHEN N'2182' THEN N'תתקמ"ב'
      WHEN N'2183' THEN N'תתקמ"ג'
      WHEN N'2184' THEN N'תתקמ"ד'
      WHEN N'2185' THEN N'תתקמ"ה'
      WHEN N'2186' THEN N'תתקמ"ו'
      WHEN N'2187' THEN N'תתקמ"ז'
      WHEN N'2188' THEN N'תתקמ"ח'
      WHEN N'2189' THEN N'תתקמ"ט'
      WHEN N'2190' THEN N'תתק"נ'
      WHEN N'2191' THEN N'תתקנ"א'
      WHEN N'2192' THEN N'תתקנ"ב'
      WHEN N'2193' THEN N'תתקנ"ג'
      WHEN N'2194' THEN N'תתקנ"ד'
      WHEN N'2195' THEN N'תתקנ"ה'
      WHEN N'2196' THEN N'תתקנ"ו'
      WHEN N'2197' THEN N'תתקנ"ז'
      WHEN N'2198' THEN N'תתקנ"ח'
      WHEN N'2199' THEN N'תתקנ"ט'
      WHEN N'2200' THEN N'תתק"ס'
      WHEN N'5001' THEN N'א'''
      WHEN N'5002' THEN N'ב'''
      WHEN N'5003' THEN N'ג'''
      WHEN N'5004' THEN N'ד'''
      WHEN N'5005' THEN N'ה'''
      WHEN N'5006' THEN N'ו'''
      WHEN N'5007' THEN N'ז'''
      WHEN N'5008' THEN N'ח'''
      WHEN N'5009' THEN N'ט'''
      WHEN N'5010' THEN N'י'''
      WHEN N'5011' THEN N'י"א'
      WHEN N'5012' THEN N'י"ב'
      WHEN N'5013' THEN N'י"ג'
      WHEN N'5014' THEN N'י"ד'
      WHEN N'5015' THEN N'ט"ו'
      WHEN N'5016' THEN N'ט"ז'
      WHEN N'5017' THEN N'י"ז'
      WHEN N'5018' THEN N'י"ח'
      WHEN N'5019' THEN N'י"ט'
      WHEN N'5020' THEN N'כ'''
      WHEN N'5021' THEN N'כ"א'
      WHEN N'5022' THEN N'כ"ב'
      WHEN N'5023' THEN N'כ"ג'
      WHEN N'5024' THEN N'כ"ד'
      WHEN N'5025' THEN N'כ"ה'
      WHEN N'5026' THEN N'כ"ו'
      WHEN N'5027' THEN N'כ"ז'
      WHEN N'5028' THEN N'כ"ח'
      WHEN N'5029' THEN N'כ"ט'
      WHEN N'5030' THEN N'ל'''
      WHEN N'5031' THEN N'ל"א'
      WHEN N'5032' THEN N'ל"ב'
      WHEN N'5033' THEN N'ל"ג'
      WHEN N'5034' THEN N'ל"ד'
      WHEN N'5035' THEN N'ל"ה'
      WHEN N'5036' THEN N'ל"ו'
      WHEN N'5037' THEN N'ל"ז'
      WHEN N'5038' THEN N'ל"ח'
      WHEN N'5039' THEN N'ל"ט'
      WHEN N'5040' THEN N'מ'''
      WHEN N'5041' THEN N'מ"א'
      WHEN N'5042' THEN N'מ"ב'
      WHEN N'5043' THEN N'מ"ג'
      WHEN N'5044' THEN N'מ"ד'
      WHEN N'5045' THEN N'מ"ה'
      WHEN N'5046' THEN N'מ"ו'
      WHEN N'5047' THEN N'מ"ז'
      WHEN N'5048' THEN N'מ"ח'
      WHEN N'5049' THEN N'מ"ט'
      WHEN N'5050' THEN N'נ'''
      WHEN N'5051' THEN N'נ"א'
      WHEN N'5052' THEN N'נ"ב'
      WHEN N'5053' THEN N'נ"ג'
      WHEN N'5054' THEN N'נ"ד'
      WHEN N'5055' THEN N'נ"ה'
      WHEN N'5056' THEN N'נ"ו'
      WHEN N'5057' THEN N'נ"ז'
      WHEN N'5058' THEN N'נ"ח'
      WHEN N'5059' THEN N'נ"ט'
      WHEN N'5060' THEN N'ס'''
      WHEN N'5061' THEN N'ס"א'
      WHEN N'5062' THEN N'ס"ב'
      WHEN N'5063' THEN N'ס"ג'
      WHEN N'5064' THEN N'ס"ד'
      WHEN N'5065' THEN N'ס"ה'
      WHEN N'5066' THEN N'ס"ו'
      WHEN N'5067' THEN N'ס"ז'
      WHEN N'5068' THEN N'ס"ח'
      WHEN N'5069' THEN N'ס"ט'
      WHEN N'5070' THEN N'ע'''
      WHEN N'5071' THEN N'ע"א'
      WHEN N'5072' THEN N'ע"ב'
      WHEN N'5073' THEN N'ע"ג'
      WHEN N'5074' THEN N'ע"ד'
      WHEN N'5075' THEN N'ע"ה'
      WHEN N'5076' THEN N'ע"ו'
      WHEN N'5077' THEN N'ע"ז'
      WHEN N'5078' THEN N'ע"ח'
      WHEN N'5079' THEN N'ע"ט'
      WHEN N'5080' THEN N'פ'''
      WHEN N'5081' THEN N'פ"א'
      WHEN N'5082' THEN N'פ"ב'
      WHEN N'5083' THEN N'פ"ג'
      WHEN N'5084' THEN N'פ"ד'
      WHEN N'5085' THEN N'פ"ה'
      WHEN N'5086' THEN N'פ"ו'
      WHEN N'5087' THEN N'פ"ז'
      WHEN N'5088' THEN N'פ"ח'
      WHEN N'5089' THEN N'פ"ט'
      WHEN N'5090' THEN N'צ'''
      WHEN N'5091' THEN N'צ"א'
      WHEN N'5092' THEN N'צ"ב'
      WHEN N'5093' THEN N'צ"ג'
      WHEN N'5094' THEN N'צ"ד'
      WHEN N'5095' THEN N'צ"ה'
      WHEN N'5096' THEN N'צ"ו'
      WHEN N'5097' THEN N'צ"ז'
      WHEN N'5098' THEN N'צ"ח'
      WHEN N'5099' THEN N'צ"ט'
      WHEN N'5100' THEN N'ק'''
      WHEN N'5101' THEN N'ק"א'
      WHEN N'5102' THEN N'ק"ב'
      WHEN N'5103' THEN N'ק"ג'
      WHEN N'5104' THEN N'ק"ד'
      WHEN N'5105' THEN N'ק"ה'
      WHEN N'5106' THEN N'ק"ו'
      WHEN N'5107' THEN N'ק"ז'
      WHEN N'5108' THEN N'ק"ח'
      WHEN N'5109' THEN N'ק"ט'
      WHEN N'5110' THEN N'ק"י'
      WHEN N'5111' THEN N'קי"א'
      WHEN N'5112' THEN N'קי"ב'
      WHEN N'5113' THEN N'קי"ג'
      WHEN N'5114' THEN N'קי"ד'
      WHEN N'5115' THEN N'קט"ו'
      WHEN N'5116' THEN N'קט"ז'
      WHEN N'5117' THEN N'קי"ז'
      WHEN N'5118' THEN N'קי"ח'
      WHEN N'5119' THEN N'קי"ט'
      WHEN N'5120' THEN N'ק"כ'
      WHEN N'5121' THEN N'קכ"א'
      WHEN N'5122' THEN N'קכ"ב'
      WHEN N'5123' THEN N'קכ"ג'
      WHEN N'5124' THEN N'קכ"ד'
      WHEN N'5125' THEN N'קכ"ה'
      WHEN N'5126' THEN N'קכ"ו'
      WHEN N'5127' THEN N'קכ"ז'
      WHEN N'5128' THEN N'קכ"ח'
      WHEN N'5129' THEN N'קכ"ט'
      WHEN N'5130' THEN N'ק"ל'
      WHEN N'5131' THEN N'קל"א'
      WHEN N'5132' THEN N'קל"ב'
      WHEN N'5133' THEN N'קל"ג'
      WHEN N'5134' THEN N'קל"ד'
      WHEN N'5135' THEN N'קל"ה'
      WHEN N'5136' THEN N'קל"ו'
      WHEN N'5137' THEN N'קל"ז'
      WHEN N'5138' THEN N'קל"ח'
      WHEN N'5139' THEN N'קל"ט'
      WHEN N'5140' THEN N'ק"מ'
      WHEN N'5141' THEN N'קמ"א'
      WHEN N'5142' THEN N'קמ"ב'
      WHEN N'5143' THEN N'קמ"ג'
      WHEN N'5144' THEN N'קמ"ד'
      WHEN N'5145' THEN N'קמ"ה'
      WHEN N'5146' THEN N'קמ"ו'
      WHEN N'5147' THEN N'קמ"ז'
      WHEN N'5148' THEN N'קמ"ח'
      WHEN N'5149' THEN N'קמ"ט'
      WHEN N'5150' THEN N'ק"נ'
      WHEN N'5151' THEN N'קנ"א'
      WHEN N'5152' THEN N'קנ"ב'
      WHEN N'5153' THEN N'קנ"ג'
      WHEN N'5154' THEN N'קנ"ד'
      WHEN N'5155' THEN N'קנ"ה'
      WHEN N'5156' THEN N'קנ"ו'
      WHEN N'5157' THEN N'קנ"ז'
      WHEN N'5158' THEN N'קנ"ח'
      WHEN N'5159' THEN N'קנ"ט'
      WHEN N'5160' THEN N'ק"ס'
      WHEN N'5161' THEN N'קס"א'
      WHEN N'5162' THEN N'קס"ב'
      WHEN N'5163' THEN N'קס"ג'
      WHEN N'5164' THEN N'קס"ד'
      WHEN N'5165' THEN N'קס"ה'
      WHEN N'5166' THEN N'קס"ו'
      WHEN N'5167' THEN N'קס"ז'
      WHEN N'5168' THEN N'קס"ח'
      WHEN N'5169' THEN N'קס"ט'
      WHEN N'5170' THEN N'ק"ע'
      WHEN N'5171' THEN N'קע"א'
      WHEN N'5172' THEN N'קע"ב'
      WHEN N'5173' THEN N'קע"ג'
      WHEN N'5174' THEN N'קע"ד'
      WHEN N'5175' THEN N'קע"ה'
      WHEN N'5176' THEN N'קע"ו'
      WHEN N'5177' THEN N'קע"ז'
      WHEN N'5178' THEN N'קע"ח'
      WHEN N'5179' THEN N'קע"ט'
      WHEN N'5180' THEN N'ק"פ'
      WHEN N'5181' THEN N'קפ"א'
      WHEN N'5182' THEN N'קפ"ב'
      WHEN N'5183' THEN N'קפ"ג'
      WHEN N'5184' THEN N'קפ"ד'
      WHEN N'5185' THEN N'קפ"ה'
      WHEN N'5186' THEN N'קפ"ו'
      WHEN N'5187' THEN N'קפ"ז'
      WHEN N'5188' THEN N'קפ"ח'
      WHEN N'5189' THEN N'קפ"ט'
      WHEN N'5190' THEN N'ק"צ'
      WHEN N'5191' THEN N'קצ"א'
      WHEN N'5192' THEN N'קצ"ב'
      WHEN N'5193' THEN N'קצ"ג'
      WHEN N'5194' THEN N'קצ"ד'
      WHEN N'5195' THEN N'קצ"ה'
      WHEN N'5196' THEN N'קצ"ו'
      WHEN N'5197' THEN N'קצ"ז'
      WHEN N'5198' THEN N'קצ"ח'
      WHEN N'5199' THEN N'קצ"ט'
      WHEN N'5200' THEN N'ר'''
      WHEN N'5201' THEN N'ר"א'
      WHEN N'5202' THEN N'ר"ב'
      WHEN N'5203' THEN N'ר"ג'
      WHEN N'5204' THEN N'ר"ד'
      WHEN N'5205' THEN N'ר"ה'
      WHEN N'5206' THEN N'ר"ו'
      WHEN N'5207' THEN N'ר"ז'
      WHEN N'5208' THEN N'ר"ח'
      WHEN N'5209' THEN N'ר"ט'
      WHEN N'5210' THEN N'ר"י'
      WHEN N'5211' THEN N'רי"א'
      WHEN N'5212' THEN N'רי"ב'
      WHEN N'5213' THEN N'רי"ג'
      WHEN N'5214' THEN N'רי"ד'
      WHEN N'5215' THEN N'רט"ו'
      WHEN N'5216' THEN N'רט"ז'
      WHEN N'5217' THEN N'רי"ז'
      WHEN N'5218' THEN N'רי"ח'
      WHEN N'5219' THEN N'רי"ט'
      WHEN N'5220' THEN N'ר"כ'
      WHEN N'5221' THEN N'רכ"א'
      WHEN N'5222' THEN N'רכ"ב'
      WHEN N'5223' THEN N'רכ"ג'
      WHEN N'5224' THEN N'רכ"ד'
      WHEN N'5225' THEN N'רכ"ה'
      WHEN N'5226' THEN N'רכ"ו'
      WHEN N'5227' THEN N'רכ"ז'
      WHEN N'5228' THEN N'רכ"ח'
      WHEN N'5229' THEN N'רכ"ט'
      WHEN N'5230' THEN N'ר"ל'
      WHEN N'5231' THEN N'רל"א'
      WHEN N'5232' THEN N'רל"ב'
      WHEN N'5233' THEN N'רל"ג'
      WHEN N'5234' THEN N'רל"ד'
      WHEN N'5235' THEN N'רל"ה'
      WHEN N'5236' THEN N'רל"ו'
      WHEN N'5237' THEN N'רל"ז'
      WHEN N'5238' THEN N'רל"ח'
      WHEN N'5239' THEN N'רל"ט'
      WHEN N'5240' THEN N'ר"מ'
      WHEN N'5241' THEN N'רמ"א'
      WHEN N'5242' THEN N'רמ"ב'
      WHEN N'5243' THEN N'רמ"ג'
      WHEN N'5244' THEN N'רמ"ד'
      WHEN N'5245' THEN N'רמ"ה'
      WHEN N'5246' THEN N'רמ"ו'
      WHEN N'5247' THEN N'רמ"ז'
      WHEN N'5248' THEN N'רמ"ח'
      WHEN N'5249' THEN N'רמ"ט'
      WHEN N'5250' THEN N'ר"נ'
      WHEN N'5251' THEN N'רנ"א'
      WHEN N'5252' THEN N'רנ"ב'
      WHEN N'5253' THEN N'רנ"ג'
      WHEN N'5254' THEN N'רנ"ד'
      WHEN N'5255' THEN N'רנ"ה'
      WHEN N'5256' THEN N'רנ"ו'
      WHEN N'5257' THEN N'רנ"ז'
      WHEN N'5258' THEN N'רנ"ח'
      WHEN N'5259' THEN N'רנ"ט'
      WHEN N'5260' THEN N'ר"ס'
      WHEN N'5261' THEN N'רס"א'
      WHEN N'5262' THEN N'רס"ב'
      WHEN N'5263' THEN N'רס"ג'
      WHEN N'5264' THEN N'רס"ד'
      WHEN N'5265' THEN N'רס"ה'
      WHEN N'5266' THEN N'רס"ו'
      WHEN N'5267' THEN N'רס"ז'
      WHEN N'5268' THEN N'רס"ח'
      WHEN N'5269' THEN N'רס"ט'
      WHEN N'5270' THEN N'ר"ע'
      WHEN N'5271' THEN N'רע"א'
      WHEN N'5272' THEN N'רע"ב'
      WHEN N'5273' THEN N'רע"ג'
      WHEN N'5274' THEN N'רע"ד'
      WHEN N'5275' THEN N'רע"ה'
      WHEN N'5276' THEN N'רע"ו'
      WHEN N'5277' THEN N'רע"ז'
      WHEN N'5278' THEN N'רע"ח'
      WHEN N'5279' THEN N'רע"ט'
      WHEN N'5280' THEN N'ר"פ'
      WHEN N'5281' THEN N'רפ"א'
      WHEN N'5282' THEN N'רפ"ב'
      WHEN N'5283' THEN N'רפ"ג'
      WHEN N'5284' THEN N'רפ"ד'
      WHEN N'5285' THEN N'רפ"ה'
      WHEN N'5286' THEN N'רפ"ו'
      WHEN N'5287' THEN N'רפ"ז'
      WHEN N'5288' THEN N'רפ"ח'
      WHEN N'5289' THEN N'רפ"ט'
      WHEN N'5290' THEN N'ר"צ'
      WHEN N'5291' THEN N'רצ"א'
      WHEN N'5292' THEN N'רצ"ב'
      WHEN N'5293' THEN N'רצ"ג'
      WHEN N'5294' THEN N'רצ"ד'
      WHEN N'5295' THEN N'רצ"ה'
      WHEN N'5296' THEN N'רצ"ו'
      WHEN N'5297' THEN N'רצ"ז'
      WHEN N'5298' THEN N'רצ"ח'
      WHEN N'5299' THEN N'רצ"ט'
      WHEN N'5300' THEN N'ש'''
      WHEN N'5301' THEN N'ש"א'
      WHEN N'5302' THEN N'ש"ב'
      WHEN N'5303' THEN N'ש"ג'
      WHEN N'5304' THEN N'ש"ד'
      WHEN N'5305' THEN N'ש"ה'
      WHEN N'5306' THEN N'ש"ו'
      WHEN N'5307' THEN N'ש"ז'
      WHEN N'5308' THEN N'ש"ח'
      WHEN N'5309' THEN N'ש"ט'
      WHEN N'5310' THEN N'ש"י'
      WHEN N'5311' THEN N'שי"א'
      WHEN N'5312' THEN N'שי"ב'
      WHEN N'5313' THEN N'שי"ג'
      WHEN N'5314' THEN N'שי"ד'
      WHEN N'5315' THEN N'שט"ו'
      WHEN N'5316' THEN N'שט"ז'
      WHEN N'5317' THEN N'שי"ז'
      WHEN N'5318' THEN N'שי"ח'
      WHEN N'5319' THEN N'שי"ט'
      WHEN N'5320' THEN N'ש"כ'
      WHEN N'5321' THEN N'שכ"א'
      WHEN N'5322' THEN N'שכ"ב'
      WHEN N'5323' THEN N'שכ"ג'
      WHEN N'5324' THEN N'שכ"ד'
      WHEN N'5325' THEN N'שכ"ה'
      WHEN N'5326' THEN N'שכ"ו'
      WHEN N'5327' THEN N'שכ"ז'
      WHEN N'5328' THEN N'שכ"ח'
      WHEN N'5329' THEN N'שכ"ט'
      WHEN N'5330' THEN N'ש"ל'
      WHEN N'5331' THEN N'של"א'
      WHEN N'5332' THEN N'של"ב'
      WHEN N'5333' THEN N'של"ג'
      WHEN N'5334' THEN N'של"ד'
      WHEN N'5335' THEN N'של"ה'
      WHEN N'5336' THEN N'של"ו'
      WHEN N'5337' THEN N'של"ז'
      WHEN N'5338' THEN N'של"ח'
      WHEN N'5339' THEN N'של"ט'
      WHEN N'5340' THEN N'ש"מ'
      WHEN N'5341' THEN N'שמ"א'
      WHEN N'5342' THEN N'שמ"ב'
      WHEN N'5343' THEN N'שמ"ג'
      WHEN N'5344' THEN N'שמ"ד'
      WHEN N'5345' THEN N'שמ"ה'
      WHEN N'5346' THEN N'שמ"ו'
      WHEN N'5347' THEN N'שמ"ז'
      WHEN N'5348' THEN N'שמ"ח'
      WHEN N'5349' THEN N'שמ"ט'
      WHEN N'5350' THEN N'ש"נ'
      WHEN N'5351' THEN N'שנ"א'
      WHEN N'5352' THEN N'שנ"ב'
      WHEN N'5353' THEN N'שנ"ג'
      WHEN N'5354' THEN N'שנ"ד'
      WHEN N'5355' THEN N'שנ"ה'
      WHEN N'5356' THEN N'שנ"ו'
      WHEN N'5357' THEN N'שנ"ז'
      WHEN N'5358' THEN N'שנ"ח'
      WHEN N'5359' THEN N'שנ"ט'
      WHEN N'5360' THEN N'ש"ס'
      WHEN N'5361' THEN N'שס"א'
      WHEN N'5362' THEN N'שס"ב'
      WHEN N'5363' THEN N'שס"ג'
      WHEN N'5364' THEN N'שס"ד'
      WHEN N'5365' THEN N'שס"ה'
      WHEN N'5366' THEN N'שס"ו'
      WHEN N'5367' THEN N'שס"ז'
      WHEN N'5368' THEN N'שס"ח'
      WHEN N'5369' THEN N'שס"ט'
      WHEN N'5370' THEN N'ש"ע'
      WHEN N'5371' THEN N'שע"א'
      WHEN N'5372' THEN N'שע"ב'
      WHEN N'5373' THEN N'שע"ג'
      WHEN N'5374' THEN N'שע"ד'
      WHEN N'5375' THEN N'שע"ה'
      WHEN N'5376' THEN N'שע"ו'
      WHEN N'5377' THEN N'שע"ז'
      WHEN N'5378' THEN N'שע"ח'
      WHEN N'5379' THEN N'שע"ט'
      WHEN N'5380' THEN N'ש"פ'
      WHEN N'5381' THEN N'שפ"א'
      WHEN N'5382' THEN N'שפ"ב'
      WHEN N'5383' THEN N'שפ"ג'
      WHEN N'5384' THEN N'שפ"ד'
      WHEN N'5385' THEN N'שפ"ה'
      WHEN N'5386' THEN N'שפ"ו'
      WHEN N'5387' THEN N'שפ"ז'
      WHEN N'5388' THEN N'שפ"ח'
      WHEN N'5389' THEN N'שפ"ט'
      WHEN N'5390' THEN N'ש"צ'
      WHEN N'5391' THEN N'שצ"א'
      WHEN N'5392' THEN N'שצ"ב'
      WHEN N'5393' THEN N'שצ"ג'
      WHEN N'5394' THEN N'שצ"ד'
      WHEN N'5395' THEN N'שצ"ה'
      WHEN N'5396' THEN N'שצ"ו'
      WHEN N'5397' THEN N'שצ"ז'
      WHEN N'5398' THEN N'שצ"ח'
      WHEN N'5399' THEN N'שצ"ט'
      WHEN N'5400' THEN N'ת'''
      WHEN N'5401' THEN N'ת"א'
      WHEN N'5402' THEN N'ת"ב'
      WHEN N'5403' THEN N'ת"ג'
      WHEN N'5404' THEN N'ת"ד'
      WHEN N'5405' THEN N'ת"ה'
      WHEN N'5406' THEN N'ת"ו'
      WHEN N'5407' THEN N'ת"ז'
      WHEN N'5408' THEN N'ת"ח'
      WHEN N'5409' THEN N'ת"ט'
      WHEN N'5410' THEN N'ת"י'
      WHEN N'5411' THEN N'תי"א'
      WHEN N'5412' THEN N'תי"ב'
      WHEN N'5413' THEN N'תי"ג'
      WHEN N'5414' THEN N'תי"ד'
      WHEN N'5415' THEN N'תט"ו'
      WHEN N'5416' THEN N'תט"ז'
      WHEN N'5417' THEN N'תי"ז'
      WHEN N'5418' THEN N'תי"ח'
      WHEN N'5419' THEN N'תי"ט'
      WHEN N'5420' THEN N'ת"כ'
      WHEN N'5421' THEN N'תכ"א'
      WHEN N'5422' THEN N'תכ"ב'
      WHEN N'5423' THEN N'תכ"ג'
      WHEN N'5424' THEN N'תכ"ד'
      WHEN N'5425' THEN N'תכ"ה'
      WHEN N'5426' THEN N'תכ"ו'
      WHEN N'5427' THEN N'תכ"ז'
      WHEN N'5428' THEN N'תכ"ח'
      WHEN N'5429' THEN N'תכ"ט'
      WHEN N'5430' THEN N'ת"ל'
      WHEN N'5431' THEN N'תל"א'
      WHEN N'5432' THEN N'תל"ב'
      WHEN N'5433' THEN N'תל"ג'
      WHEN N'5434' THEN N'תל"ד'
      WHEN N'5435' THEN N'תל"ה'
      WHEN N'5436' THEN N'תל"ו'
      WHEN N'5437' THEN N'תל"ז'
      WHEN N'5438' THEN N'תל"ח'
      WHEN N'5439' THEN N'תל"ט'
      WHEN N'5440' THEN N'ת"מ'
      WHEN N'5441' THEN N'תמ"א'
      WHEN N'5442' THEN N'תמ"ב'
      WHEN N'5443' THEN N'תמ"ג'
      WHEN N'5444' THEN N'תמ"ד'
      WHEN N'5445' THEN N'תמ"ה'
      WHEN N'5446' THEN N'תמ"ו'
      WHEN N'5447' THEN N'תמ"ז'
      WHEN N'5448' THEN N'תמ"ח'
      WHEN N'5449' THEN N'תמ"ט'
      WHEN N'5450' THEN N'ת"נ'
      WHEN N'5451' THEN N'תנ"א'
      WHEN N'5452' THEN N'תנ"ב'
      WHEN N'5453' THEN N'תנ"ג'
      WHEN N'5454' THEN N'תנ"ד'
      WHEN N'5455' THEN N'תנ"ה'
      WHEN N'5456' THEN N'תנ"ו'
      WHEN N'5457' THEN N'תנ"ז'
      WHEN N'5458' THEN N'תנ"ח'
      WHEN N'5459' THEN N'תנ"ט'
      WHEN N'5460' THEN N'ת"ס'
      WHEN N'5461' THEN N'תס"א'
      WHEN N'5462' THEN N'תס"ב'
      WHEN N'5463' THEN N'תס"ג'
      WHEN N'5464' THEN N'תס"ד'
      WHEN N'5465' THEN N'תס"ה'
      WHEN N'5466' THEN N'תס"ו'
      WHEN N'5467' THEN N'תס"ז'
      WHEN N'5468' THEN N'תס"ח'
      WHEN N'5469' THEN N'תס"ט'
      WHEN N'5470' THEN N'ת"ע'
      WHEN N'5471' THEN N'תע"א'
      WHEN N'5472' THEN N'תע"ב'
      WHEN N'5473' THEN N'תע"ג'
      WHEN N'5474' THEN N'תע"ד'
      WHEN N'5475' THEN N'תע"ה'
      WHEN N'5476' THEN N'תע"ו'
      WHEN N'5477' THEN N'תע"ז'
      WHEN N'5478' THEN N'תע"ח'
      WHEN N'5479' THEN N'תע"ט'
      WHEN N'5480' THEN N'ת"פ'
      WHEN N'5481' THEN N'תפ"א'
      WHEN N'5482' THEN N'תפ"ב'
      WHEN N'5483' THEN N'תפ"ג'
      WHEN N'5484' THEN N'תפ"ד'
      WHEN N'5485' THEN N'תפ"ה'
      WHEN N'5486' THEN N'תפ"ו'
      WHEN N'5487' THEN N'תפ"ז'
      WHEN N'5488' THEN N'תפ"ח'
      WHEN N'5489' THEN N'תפ"ט'
      WHEN N'5490' THEN N'ת"צ'
      WHEN N'5491' THEN N'תצ"א'
      WHEN N'5492' THEN N'תצ"ב'
      WHEN N'5493' THEN N'תצ"ג'
      WHEN N'5494' THEN N'תצ"ד'
      WHEN N'5495' THEN N'תצ"ה'
      WHEN N'5496' THEN N'תצ"ו'
      WHEN N'5497' THEN N'תצ"ז'
      WHEN N'5498' THEN N'תצ"ח'
      WHEN N'5499' THEN N'תצ"ט'
      WHEN N'5500' THEN N'ת"ק'
      WHEN N'5501' THEN N'תק"א'
      WHEN N'5502' THEN N'תק"ב'
      WHEN N'5503' THEN N'תק"ג'
      WHEN N'5504' THEN N'תק"ד'
      WHEN N'5505' THEN N'תק"ה'
      WHEN N'5506' THEN N'תק"ו'
      WHEN N'5507' THEN N'תק"ז'
      WHEN N'5508' THEN N'תק"ח'
      WHEN N'5509' THEN N'תק"ט'
      WHEN N'5510' THEN N'תק"י'
      WHEN N'5511' THEN N'תקי"א'
      WHEN N'5512' THEN N'תקי"ב'
      WHEN N'5513' THEN N'תקי"ג'
      WHEN N'5514' THEN N'תקי"ד'
      WHEN N'5515' THEN N'תקט"ו'
      WHEN N'5516' THEN N'תקט"ז'
      WHEN N'5517' THEN N'תקי"ז'
      WHEN N'5518' THEN N'תקי"ח'
      WHEN N'5519' THEN N'תקי"ט'
      WHEN N'5520' THEN N'תק"כ'
      WHEN N'5521' THEN N'תקכ"א'
      WHEN N'5522' THEN N'תקכ"ב'
      WHEN N'5523' THEN N'תקכ"ג'
      WHEN N'5524' THEN N'תקכ"ד'
      WHEN N'5525' THEN N'תקכ"ה'
      WHEN N'5526' THEN N'תקכ"ו'
      WHEN N'5527' THEN N'תקכ"ז'
      WHEN N'5528' THEN N'תקכ"ח'
      WHEN N'5529' THEN N'תקכ"ט'
      WHEN N'5530' THEN N'תק"ל'
      WHEN N'5531' THEN N'תקל"א'
      WHEN N'5532' THEN N'תקל"ב'
      WHEN N'5533' THEN N'תקל"ג'
      WHEN N'5534' THEN N'תקל"ד'
      WHEN N'5535' THEN N'תקל"ה'
      WHEN N'5536' THEN N'תקל"ו'
      WHEN N'5537' THEN N'תקל"ז'
      WHEN N'5538' THEN N'תקל"ח'
      WHEN N'5539' THEN N'תקל"ט'
      WHEN N'5540' THEN N'תק"מ'
      WHEN N'5541' THEN N'תקמ"א'
      WHEN N'5542' THEN N'תקמ"ב'
      WHEN N'5543' THEN N'תקמ"ג'
      WHEN N'5544' THEN N'תקמ"ד'
      WHEN N'5545' THEN N'תקמ"ה'
      WHEN N'5546' THEN N'תקמ"ו'
      WHEN N'5547' THEN N'תקמ"ז'
      WHEN N'5548' THEN N'תקמ"ח'
      WHEN N'5549' THEN N'תקמ"ט'
      WHEN N'5550' THEN N'תק"נ'
      WHEN N'5551' THEN N'תקנ"א'
      WHEN N'5552' THEN N'תקנ"ב'
      WHEN N'5553' THEN N'תקנ"ג'
      WHEN N'5554' THEN N'תקנ"ד'
      WHEN N'5555' THEN N'תקנ"ה'
      WHEN N'5556' THEN N'תקנ"ו'
      WHEN N'5557' THEN N'תקנ"ז'
      WHEN N'5558' THEN N'תקנ"ח'
      WHEN N'5559' THEN N'תקנ"ט'
      WHEN N'5560' THEN N'תק"ס'
      WHEN N'5561' THEN N'תקס"א'
      WHEN N'5562' THEN N'תקס"ב'
      WHEN N'5563' THEN N'תקס"ג'
      WHEN N'5564' THEN N'תקס"ד'
      WHEN N'5565' THEN N'תקס"ה'
      WHEN N'5566' THEN N'תקס"ו'
      WHEN N'5567' THEN N'תקס"ז'
      WHEN N'5568' THEN N'תקס"ח'
      WHEN N'5569' THEN N'תקס"ט'
      WHEN N'5570' THEN N'תק"ע'
      WHEN N'5571' THEN N'תקע"א'
      WHEN N'5572' THEN N'תקע"ב'
      WHEN N'5573' THEN N'תקע"ג'
      WHEN N'5574' THEN N'תקע"ד'
      WHEN N'5575' THEN N'תקע"ה'
      WHEN N'5576' THEN N'תקע"ו'
      WHEN N'5577' THEN N'תקע"ז'
      WHEN N'5578' THEN N'תקע"ח'
      WHEN N'5579' THEN N'תקע"ט'
      WHEN N'5580' THEN N'תק"פ'
      WHEN N'5581' THEN N'תקפ"א'
      WHEN N'5582' THEN N'תקפ"ב'
      WHEN N'5583' THEN N'תקפ"ג'
      WHEN N'5584' THEN N'תקפ"ד'
      WHEN N'5585' THEN N'תקפ"ה'
      WHEN N'5586' THEN N'תקפ"ו'
      WHEN N'5587' THEN N'תקפ"ז'
      WHEN N'5588' THEN N'תקפ"ח'
      WHEN N'5589' THEN N'תקפ"ט'
      WHEN N'5590' THEN N'תק"צ'
      WHEN N'5591' THEN N'תקצ"א'
      WHEN N'5592' THEN N'תקצ"ב'
      WHEN N'5593' THEN N'תקצ"ג'
      WHEN N'5594' THEN N'תקצ"ד'
      WHEN N'5595' THEN N'תקצ"ה'
      WHEN N'5596' THEN N'תקצ"ו'
      WHEN N'5597' THEN N'תקצ"ז'
      WHEN N'5598' THEN N'תקצ"ח'
      WHEN N'5599' THEN N'תקצ"ט'
      WHEN N'5600' THEN N'ת"ר'
      WHEN N'5601' THEN N'תר"א'
      WHEN N'5602' THEN N'תר"ב'
      WHEN N'5603' THEN N'תר"ג'
      WHEN N'5604' THEN N'תר"ד'
      WHEN N'5605' THEN N'תר"ה'
      WHEN N'5606' THEN N'תר"ו'
      WHEN N'5607' THEN N'תר"ז'
      WHEN N'5608' THEN N'תר"ח'
      WHEN N'5609' THEN N'תר"ט'
      WHEN N'5610' THEN N'תר"י'
      WHEN N'5611' THEN N'תרי"א'
      WHEN N'5612' THEN N'תרי"ב'
      WHEN N'5613' THEN N'תרי"ג'
      WHEN N'5614' THEN N'תרי"ד'
      WHEN N'5615' THEN N'תרט"ו'
      WHEN N'5616' THEN N'תרט"ז'
      WHEN N'5617' THEN N'תרי"ז'
      WHEN N'5618' THEN N'תרי"ח'
      WHEN N'5619' THEN N'תרי"ט'
      WHEN N'5620' THEN N'תר"כ'
      WHEN N'5621' THEN N'תרכ"א'
      WHEN N'5622' THEN N'תרכ"ב'
      WHEN N'5623' THEN N'תרכ"ג'
      WHEN N'5624' THEN N'תרכ"ד'
      WHEN N'5625' THEN N'תרכ"ה'
      WHEN N'5626' THEN N'תרכ"ו'
      WHEN N'5627' THEN N'תרכ"ז'
      WHEN N'5628' THEN N'תרכ"ח'
      WHEN N'5629' THEN N'תרכ"ט'
      WHEN N'5630' THEN N'תר"ל'
      WHEN N'5631' THEN N'תרל"א'
      WHEN N'5632' THEN N'תרל"ב'
      WHEN N'5633' THEN N'תרל"ג'
      WHEN N'5634' THEN N'תרל"ד'
      WHEN N'5635' THEN N'תרל"ה'
      WHEN N'5636' THEN N'תרל"ו'
      WHEN N'5637' THEN N'תרל"ז'
      WHEN N'5638' THEN N'תרל"ח'
      WHEN N'5639' THEN N'תרל"ט'
      WHEN N'5640' THEN N'תר"מ'
      WHEN N'5641' THEN N'תרמ"א'
      WHEN N'5642' THEN N'תרמ"ב'
      WHEN N'5643' THEN N'תרמ"ג'
      WHEN N'5644' THEN N'תרמ"ד'
      WHEN N'5645' THEN N'תרמ"ה'
      WHEN N'5646' THEN N'תרמ"ו'
      WHEN N'5647' THEN N'תרמ"ז'
      WHEN N'5648' THEN N'תרמ"ח'
      WHEN N'5649' THEN N'תרמ"ט'
      WHEN N'5650' THEN N'תר"נ'
      WHEN N'5651' THEN N'תרנ"א'
      WHEN N'5652' THEN N'תרנ"ב'
      WHEN N'5653' THEN N'תרנ"ג'
      WHEN N'5654' THEN N'תרנ"ד'
      WHEN N'5655' THEN N'תרנ"ה'
      WHEN N'5656' THEN N'תרנ"ו'
      WHEN N'5657' THEN N'תרנ"ז'
      WHEN N'5658' THEN N'תרנ"ח'
      WHEN N'5659' THEN N'תרנ"ט'
      WHEN N'5660' THEN N'תר"ס'
      WHEN N'5661' THEN N'תרס"א'
      WHEN N'5662' THEN N'תרס"ב'
      WHEN N'5663' THEN N'תרס"ג'
      WHEN N'5664' THEN N'תרס"ד'
      WHEN N'5665' THEN N'תרס"ה'
      WHEN N'5666' THEN N'תרס"ו'
      WHEN N'5667' THEN N'תרס"ז'
      WHEN N'5668' THEN N'תרס"ח'
      WHEN N'5669' THEN N'תרס"ט'
      WHEN N'5670' THEN N'תר"ע'
      WHEN N'5671' THEN N'תרע"א'
      WHEN N'5672' THEN N'תרע"ב'
      WHEN N'5673' THEN N'תרע"ג'
      WHEN N'5674' THEN N'תרע"ד'
      WHEN N'5675' THEN N'תרע"ה'
      WHEN N'5676' THEN N'תרע"ו'
      WHEN N'5677' THEN N'תרע"ז'
      WHEN N'5678' THEN N'תרע"ח'
      WHEN N'5679' THEN N'תרע"ט'
      WHEN N'5680' THEN N'תר"פ'
      WHEN N'5681' THEN N'תרפ"א'
      WHEN N'5682' THEN N'תרפ"ב'
      WHEN N'5683' THEN N'תרפ"ג'
      WHEN N'5684' THEN N'תרפ"ד'
      WHEN N'5685' THEN N'תרפ"ה'
      WHEN N'5686' THEN N'תרפ"ו'
      WHEN N'5687' THEN N'תרפ"ז'
      WHEN N'5688' THEN N'תרפ"ח'
      WHEN N'5689' THEN N'תרפ"ט'
      WHEN N'5690' THEN N'תר"צ'
      WHEN N'5691' THEN N'תרצ"א'
      WHEN N'5692' THEN N'תרצ"ב'
      WHEN N'5693' THEN N'תרצ"ג'
      WHEN N'5694' THEN N'תרצ"ד'
      WHEN N'5695' THEN N'תרצ"ה'
      WHEN N'5696' THEN N'תרצ"ו'
      WHEN N'5697' THEN N'תרצ"ז'
      WHEN N'5698' THEN N'תרצ"ח'
      WHEN N'5699' THEN N'תרצ"ט'
      WHEN N'5700' THEN N'ת"ש'
      WHEN N'5701' THEN N'תש"א'
      WHEN N'5702' THEN N'תש"ב'
      WHEN N'5703' THEN N'תש"ג'
      WHEN N'5704' THEN N'תש"ד'
      WHEN N'5705' THEN N'תש"ה'
      WHEN N'5706' THEN N'תש"ו'
      WHEN N'5707' THEN N'תש"ז'
      WHEN N'5708' THEN N'תש"ח'
      WHEN N'5709' THEN N'תש"ט'
      WHEN N'5710' THEN N'תש"י'
      WHEN N'5711' THEN N'תשי"א'
      WHEN N'5712' THEN N'תשי"ב'
      WHEN N'5713' THEN N'תשי"ג'
      WHEN N'5714' THEN N'תשי"ד'
      WHEN N'5715' THEN N'תשט"ו'
      WHEN N'5716' THEN N'תשט"ז'
      WHEN N'5717' THEN N'תשי"ז'
      WHEN N'5718' THEN N'תשי"ח'
      WHEN N'5719' THEN N'תשי"ט'
      WHEN N'5720' THEN N'תש"כ'
      WHEN N'5721' THEN N'תשכ"א'
      WHEN N'5722' THEN N'תשכ"ב'
      WHEN N'5723' THEN N'תשכ"ג'
      WHEN N'5724' THEN N'תשכ"ד'
      WHEN N'5725' THEN N'תשכ"ה'
      WHEN N'5726' THEN N'תשכ"ו'
      WHEN N'5727' THEN N'תשכ"ז'
      WHEN N'5728' THEN N'תשכ"ח'
      WHEN N'5729' THEN N'תשכ"ט'
      WHEN N'5730' THEN N'תש"ל'
      WHEN N'5731' THEN N'תשל"א'
      WHEN N'5732' THEN N'תשל"ב'
      WHEN N'5733' THEN N'תשל"ג'
      WHEN N'5734' THEN N'תשל"ד'
      WHEN N'5735' THEN N'תשל"ה'
      WHEN N'5736' THEN N'תשל"ו'
      WHEN N'5737' THEN N'תשל"ז'
      WHEN N'5738' THEN N'תשל"ח'
      WHEN N'5739' THEN N'תשל"ט'
      WHEN N'5740' THEN N'תש"מ'
      WHEN N'5741' THEN N'תשמ"א'
      WHEN N'5742' THEN N'תשמ"ב'
      WHEN N'5743' THEN N'תשמ"ג'
      WHEN N'5744' THEN N'תשמ"ד'
      WHEN N'5745' THEN N'תשמ"ה'
      WHEN N'5746' THEN N'תשמ"ו'
      WHEN N'5747' THEN N'תשמ"ז'
      WHEN N'5748' THEN N'תשמ"ח'
      WHEN N'5749' THEN N'תשמ"ט'
      WHEN N'5750' THEN N'תש"נ'
      WHEN N'5751' THEN N'תשנ"א'
      WHEN N'5752' THEN N'תשנ"ב'
      WHEN N'5753' THEN N'תשנ"ג'
      WHEN N'5754' THEN N'תשנ"ד'
      WHEN N'5755' THEN N'תשנ"ה'
      WHEN N'5756' THEN N'תשנ"ו'
      WHEN N'5757' THEN N'תשנ"ז'
      WHEN N'5758' THEN N'תשנ"ח'
      WHEN N'5759' THEN N'תשנ"ט'
      WHEN N'5760' THEN N'תש"ס'
      WHEN N'5761' THEN N'תשס"א'
      WHEN N'5762' THEN N'תשס"ב'
      WHEN N'5763' THEN N'תשס"ג'
      WHEN N'5764' THEN N'תשס"ד'
      WHEN N'5765' THEN N'תשס"ה'
      WHEN N'5766' THEN N'תשס"ו'
      WHEN N'5767' THEN N'תשס"ז'
      WHEN N'5768' THEN N'תשס"ח'
      WHEN N'5769' THEN N'תשס"ט'
      WHEN N'5770' THEN N'תש"ע'
      WHEN N'5771' THEN N'תשע"א'
      WHEN N'5772' THEN N'תשע"ב'
      WHEN N'5773' THEN N'תשע"ג'
      WHEN N'5774' THEN N'תשע"ד'
      WHEN N'5775' THEN N'תשע"ה'
      WHEN N'5776' THEN N'תשע"ו'
      WHEN N'5777' THEN N'תשע"ז'
      WHEN N'5778' THEN N'תשע"ח'
      WHEN N'5779' THEN N'תשע"ט'
      WHEN N'5780' THEN N'תש"פ'
      WHEN N'5781' THEN N'תשפ"א'
      WHEN N'5782' THEN N'תשפ"ב'
      WHEN N'5783' THEN N'תשפ"ג'
      WHEN N'5784' THEN N'תשפ"ד'
      WHEN N'5785' THEN N'תשפ"ה'
      WHEN N'5786' THEN N'תשפ"ו'
      WHEN N'5787' THEN N'תשפ"ז'
      WHEN N'5788' THEN N'תשפ"ח'
      WHEN N'5789' THEN N'תשפ"ט'
      WHEN N'5790' THEN N'תש"צ'
      WHEN N'5791' THEN N'תשצ"א'
      WHEN N'5792' THEN N'תשצ"ב'
      WHEN N'5793' THEN N'תשצ"ג'
      WHEN N'5794' THEN N'תשצ"ד'
      WHEN N'5795' THEN N'תשצ"ה'
      WHEN N'5796' THEN N'תשצ"ו'
      WHEN N'5797' THEN N'תשצ"ז'
      WHEN N'5798' THEN N'תשצ"ח'
      WHEN N'5799' THEN N'תשצ"ט'
      WHEN N'5800' THEN N'ת"ת'
      WHEN N'5801' THEN N'תת"א'
      WHEN N'5802' THEN N'תת"ב'
      WHEN N'5803' THEN N'תת"ג'
      WHEN N'5804' THEN N'תת"ד'
      WHEN N'5805' THEN N'תת"ה'
      WHEN N'5806' THEN N'תת"ו'
      WHEN N'5807' THEN N'תת"ז'
      WHEN N'5808' THEN N'תת"ח'
      WHEN N'5809' THEN N'תת"ט'
      WHEN N'5810' THEN N'תת"י'
      WHEN N'5811' THEN N'תתי"א'
      WHEN N'5812' THEN N'תתי"ב'
      WHEN N'5813' THEN N'תתי"ג'
      WHEN N'5814' THEN N'תתי"ד'
      WHEN N'5815' THEN N'תתט"ו'
      WHEN N'5816' THEN N'תתט"ז'
      WHEN N'5817' THEN N'תתי"ז'
      WHEN N'5818' THEN N'תתי"ח'
      WHEN N'5819' THEN N'תתי"ט'
      WHEN N'5820' THEN N'תת"כ'
      WHEN N'5821' THEN N'תתכ"א'
      WHEN N'5822' THEN N'תתכ"ב'
      WHEN N'5823' THEN N'תתכ"ג'
      WHEN N'5824' THEN N'תתכ"ד'
      WHEN N'5825' THEN N'תתכ"ה'
      WHEN N'5826' THEN N'תתכ"ו'
      WHEN N'5827' THEN N'תתכ"ז'
      WHEN N'5828' THEN N'תתכ"ח'
      WHEN N'5829' THEN N'תתכ"ט'
      WHEN N'5830' THEN N'תת"ל'
      WHEN N'5831' THEN N'תתל"א'
      WHEN N'5832' THEN N'תתל"ב'
      WHEN N'5833' THEN N'תתל"ג'
      WHEN N'5834' THEN N'תתל"ד'
      WHEN N'5835' THEN N'תתל"ה'
      WHEN N'5836' THEN N'תתל"ו'
      WHEN N'5837' THEN N'תתל"ז'
      WHEN N'5838' THEN N'תתל"ח'
      WHEN N'5839' THEN N'תתל"ט'
      WHEN N'5840' THEN N'תת"מ'
      WHEN N'5841' THEN N'תתמ"א'
      WHEN N'5842' THEN N'תתמ"ב'
      WHEN N'5843' THEN N'תתמ"ג'
      WHEN N'5844' THEN N'תתמ"ד'
      WHEN N'5845' THEN N'תתמ"ה'
      WHEN N'5846' THEN N'תתמ"ו'
      WHEN N'5847' THEN N'תתמ"ז'
      WHEN N'5848' THEN N'תתמ"ח'
      WHEN N'5849' THEN N'תתמ"ט'
      WHEN N'5850' THEN N'תת"נ'
      WHEN N'5851' THEN N'תתנ"א'
      WHEN N'5852' THEN N'תתנ"ב'
      WHEN N'5853' THEN N'תתנ"ג'
      WHEN N'5854' THEN N'תתנ"ד'
      WHEN N'5855' THEN N'תתנ"ה'
      WHEN N'5856' THEN N'תתנ"ו'
      WHEN N'5857' THEN N'תתנ"ז'
      WHEN N'5858' THEN N'תתנ"ח'
      WHEN N'5859' THEN N'תתנ"ט'
      WHEN N'5860' THEN N'תת"ס'
      WHEN N'5861' THEN N'תתס"א'
      WHEN N'5862' THEN N'תתס"ב'
      WHEN N'5863' THEN N'תתס"ג'
      WHEN N'5864' THEN N'תתס"ד'
      WHEN N'5865' THEN N'תתס"ה'
      WHEN N'5866' THEN N'תתס"ו'
      WHEN N'5867' THEN N'תתס"ז'
      WHEN N'5868' THEN N'תתס"ח'
      WHEN N'5869' THEN N'תתס"ט'
      WHEN N'5870' THEN N'תת"ע'
      WHEN N'5871' THEN N'תתע"א'
      WHEN N'5872' THEN N'תתע"ב'
      WHEN N'5873' THEN N'תתע"ג'
      WHEN N'5874' THEN N'תתע"ד'
      WHEN N'5875' THEN N'תתע"ה'
      WHEN N'5876' THEN N'תתע"ו'
      WHEN N'5877' THEN N'תתע"ז'
      WHEN N'5878' THEN N'תתע"ח'
      WHEN N'5879' THEN N'תתע"ט'
      WHEN N'5880' THEN N'תת"פ'
      WHEN N'5881' THEN N'תתפ"א'
      WHEN N'5882' THEN N'תתפ"ב'
      WHEN N'5883' THEN N'תתפ"ג'
      WHEN N'5884' THEN N'תתפ"ד'
      WHEN N'5885' THEN N'תתפ"ה'
      WHEN N'5886' THEN N'תתפ"ו'
      WHEN N'5887' THEN N'תתפ"ז'
      WHEN N'5888' THEN N'תתפ"ח'
      WHEN N'5889' THEN N'תתפ"ט'
      WHEN N'5890' THEN N'תת"צ'
      WHEN N'5891' THEN N'תתצ"א'
      WHEN N'5892' THEN N'תתצ"ב'
      WHEN N'5893' THEN N'תתצ"ג'
      WHEN N'5894' THEN N'תתצ"ד'
      WHEN N'5895' THEN N'תתצ"ה'
      WHEN N'5896' THEN N'תתצ"ו'
      WHEN N'5897' THEN N'תתצ"ז'
      WHEN N'5898' THEN N'תתצ"ח'
      WHEN N'5899' THEN N'תתצ"ט'
      WHEN N'5900' THEN N'תת"ק'
      WHEN N'5901' THEN N'תתק"א'
      WHEN N'5902' THEN N'תתק"ב'
      WHEN N'5903' THEN N'תתק"ג'
      WHEN N'5904' THEN N'תתק"ד'
      WHEN N'5905' THEN N'תתק"ה'
      WHEN N'5906' THEN N'תתק"ו'
      WHEN N'5907' THEN N'תתק"ז'
      WHEN N'5908' THEN N'תתק"ח'
      WHEN N'5909' THEN N'תתק"ט'
      WHEN N'5910' THEN N'תתק"י'
      WHEN N'5911' THEN N'תתקי"א'
      WHEN N'5912' THEN N'תתקי"ב'
      WHEN N'5913' THEN N'תתקי"ג'
      WHEN N'5914' THEN N'תתקי"ד'
      WHEN N'5915' THEN N'תתקט"ו'
      WHEN N'5916' THEN N'תתקט"ז'
      WHEN N'5917' THEN N'תתקי"ז'
      WHEN N'5918' THEN N'תתקי"ח'
      WHEN N'5919' THEN N'תתקי"ט'
      WHEN N'5920' THEN N'תתק"כ'
      WHEN N'5921' THEN N'תתקכ"א'
      WHEN N'5922' THEN N'תתקכ"ב'
      WHEN N'5923' THEN N'תתקכ"ג'
      WHEN N'5924' THEN N'תתקכ"ד'
      WHEN N'5925' THEN N'תתקכ"ה'
      WHEN N'5926' THEN N'תתקכ"ו'
      WHEN N'5927' THEN N'תתקכ"ז'
      WHEN N'5928' THEN N'תתקכ"ח'
      WHEN N'5929' THEN N'תתקכ"ט'
      WHEN N'5930' THEN N'תתק"ל'
      WHEN N'5931' THEN N'תתקל"א'
      WHEN N'5932' THEN N'תתקל"ב'
      WHEN N'5933' THEN N'תתקל"ג'
      WHEN N'5934' THEN N'תתקל"ד'
      WHEN N'5935' THEN N'תתקל"ה'
      WHEN N'5936' THEN N'תתקל"ו'
      WHEN N'5937' THEN N'תתקל"ז'
      WHEN N'5938' THEN N'תתקל"ח'
      WHEN N'5939' THEN N'תתקל"ט'
      WHEN N'5940' THEN N'תתק"מ'
      WHEN N'5941' THEN N'תתקמ"א'
      WHEN N'5942' THEN N'תתקמ"ב'
      WHEN N'5943' THEN N'תתקמ"ג'
      WHEN N'5944' THEN N'תתקמ"ד'
      WHEN N'5945' THEN N'תתקמ"ה'
      WHEN N'5946' THEN N'תתקמ"ו'
      WHEN N'5947' THEN N'תתקמ"ז'
      WHEN N'5948' THEN N'תתקמ"ח'
      WHEN N'5949' THEN N'תתקמ"ט'
      WHEN N'5950' THEN N'תתק"נ'
      WHEN N'5951' THEN N'תתקנ"א'
      WHEN N'5952' THEN N'תתקנ"ב'
      WHEN N'5953' THEN N'תתקנ"ג'
      WHEN N'5954' THEN N'תתקנ"ד'
      WHEN N'5955' THEN N'תתקנ"ה'
      WHEN N'5956' THEN N'תתקנ"ו'
      WHEN N'5957' THEN N'תתקנ"ז'
      WHEN N'5958' THEN N'תתקנ"ח'
      WHEN N'5959' THEN N'תתקנ"ט'
      WHEN N'5960' THEN N'תתק"ס'
      WHEN N'5961' THEN N'תתקס"א'
      WHEN N'5962' THEN N'תתקס"ב'
      WHEN N'5963' THEN N'תתקס"ג'
      WHEN N'5964' THEN N'תתקס"ד'
      WHEN N'5965' THEN N'תתקס"ה'
      WHEN N'5966' THEN N'תתקס"ו'
      WHEN N'5967' THEN N'תתקס"ז'
      WHEN N'5968' THEN N'תתקס"ח'
      WHEN N'5969' THEN N'תתקס"ט'
      WHEN N'5970' THEN N'תתק"ע'
      WHEN N'5971' THEN N'תתקע"א'
      WHEN N'5972' THEN N'תתקע"ב'
      WHEN N'5973' THEN N'תתקע"ג'
      WHEN N'5974' THEN N'תתקע"ד'
      WHEN N'5975' THEN N'תתקע"ה'
      WHEN N'5976' THEN N'תתקע"ו'
      WHEN N'5977' THEN N'תתקע"ז'
      WHEN N'5978' THEN N'תתקע"ח'
      WHEN N'5979' THEN N'תתקע"ט'
      WHEN N'5980' THEN N'תתק"פ'
      WHEN N'5981' THEN N'תתקפ"א'
      WHEN N'5982' THEN N'תתקפ"ב'
      WHEN N'5983' THEN N'תתקפ"ג'
      WHEN N'5984' THEN N'תתקפ"ד'
      WHEN N'5985' THEN N'תתקפ"ה'
      WHEN N'5986' THEN N'תתקפ"ו'
      WHEN N'5987' THEN N'תתקפ"ז'
      WHEN N'5988' THEN N'תתקפ"ח'
      WHEN N'5989' THEN N'תתקפ"ט'
      WHEN N'5990' THEN N'תתק"צ'
      WHEN N'5991' THEN N'תתקצ"א'
      WHEN N'5992' THEN N'תתקצ"ב'
      WHEN N'5993' THEN N'תתקצ"ג'
      WHEN N'5994' THEN N'תתקצ"ד'
      WHEN N'5995' THEN N'תתקצ"ה'
      WHEN N'5996' THEN N'תתקצ"ו'
      WHEN N'5997' THEN N'תתקצ"ז'
      WHEN N'5998' THEN N'תתקצ"ח'
      WHEN N'5999' THEN N'תתקצ"ט'
      ELSE [שנת_לימודים]
    END;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260616232951_CanonicalizeStoredAcademicYears'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260616232951_CanonicalizeStoredAcademicYears', N'8.0.4');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260621115616_RepairMissingProductionSchema'
)
BEGIN
    IF COL_LENGTH(N'נתוני_העסקה_מקטע', N'מקטע_הורה_שעות_נוספות') IS NULL
    BEGIN
        ALTER TABLE [נתוני_העסקה_מקטע]
            ADD [מקטע_הורה_שעות_נוספות] tinyint NULL;
    END;

    IF COL_LENGTH(N'סמלי_מוסד_מעסיקים', N'סוג_מוסד') IS NULL
    BEGIN
        ALTER TABLE [סמלי_מוסד_מעסיקים]
            ADD [סוג_מוסד] nvarchar(20) NOT NULL
                CONSTRAINT [DF_סמלי_מוסד_מעסיקים_סוג_מוסד] DEFAULT N'אחר';
    END;

    IF EXISTS (
        SELECT 1
        FROM sys.indexes i
        WHERE i.[name] = N'IX_עובדים_מזהה_מעסיק_תז'
          AND i.[object_id] = OBJECT_ID(N'[עובדים]')
          AND (i.[has_filter] = 0 OR i.[filter_definition] NOT LIKE N'%IsDeleted%')
    )
    BEGIN
        DROP INDEX [IX_עובדים_מזהה_מעסיק_תז] ON [עובדים];
    END;

    IF NOT EXISTS (
        SELECT 1
        FROM sys.indexes i
        WHERE i.[name] = N'IX_עובדים_מזהה_מעסיק_תז'
          AND i.[object_id] = OBJECT_ID(N'[עובדים]')
    )
    BEGIN
        CREATE UNIQUE INDEX [IX_עובדים_מזהה_מעסיק_תז]
            ON [עובדים] ([מזהה_מעסיק], [תז])
            WHERE [IsDeleted] = 0;
    END;

    IF COL_LENGTH(N'נתוני_העסקה_מקטע', N'מקטע_הורה_שעות_נוספות') IS NOT NULL
    BEGIN
        DELETE FROM [נתוני_העסקה_מקטע]
        WHERE [מקטע_הורה_שעות_נוספות] IS NULL
          AND ([סמל_מוסד] IS NULL OR LTRIM(RTRIM([סמל_מוסד])) = '')
          AND ([שעות_שבועיות] IS NULL OR [שעות_שבועיות] = 0);
    END;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260621115616_RepairMissingProductionSchema'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260621115616_RepairMissingProductionSchema', N'8.0.4');
END;
GO

COMMIT;
GO

