CREATE TABLE [מעסיקים] (
    [מזהה_מעסיק] int NOT NULL IDENTITY,
    [שם_מעסיק] nvarchar(450) NOT NULL,
    [חפ] nvarchar(450) NULL,
    [סמל_מוטב] nvarchar(max) NULL,
    [מספר_עוקץ] nvarchar(max) NULL,
    [CreatedAtUtc] datetimeoffset NOT NULL,
    [UpdatedAtUtc] datetimeoffset NOT NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAtUtc] datetimeoffset NULL,
    CONSTRAINT [PK_מעסיקים] PRIMARY KEY ([מזהה_מעסיק])
);
GO


CREATE TABLE [AuditLogs] (
    [Id] int NOT NULL IDENTITY,
    [EntityName] nvarchar(100) NOT NULL,
    [Action] nvarchar(50) NOT NULL,
    [EntityKey] nvarchar(200) NULL,
    [ChangesJson] nvarchar(max) NULL,
    [ChangedBy] nvarchar(100) NOT NULL,
    [ChangedAtUtc] datetimeoffset NOT NULL,
    CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Users] (
    [Id] int NOT NULL IDENTITY,
    [Username] nvarchar(128) NOT NULL,
    [PasswordHash] nvarchar(500) NOT NULL,
    [Role] nvarchar(64) NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [סמלי_מוסד_מעסיקים] (
    [מזהה_סמל_מוסד_מעסיק] int NOT NULL IDENTITY,
    [מזהה_מעסיק] int NOT NULL,
    [סמל_מוסד] nvarchar(450) NOT NULL,
    [שם_סמל_מוסד] nvarchar(max) NULL,
    CONSTRAINT [PK_סמלי_מוסד_מעסיקים] PRIMARY KEY ([מזהה_סמל_מוסד_מעסיק]),
    CONSTRAINT [FK_סמלי_מוסד_מעסיקים_מעסיקים_מזהה_מעסיק] FOREIGN KEY ([מזהה_מעסיק]) REFERENCES [מעסיקים] ([מזהה_מעסיק]) ON DELETE CASCADE
);
GO


CREATE TABLE [עובדים] (
    [מזהה_עובד] int NOT NULL IDENTITY,
    [מזהה_מעסיק] int NOT NULL,
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
    [סטטוס_פעילות_ידני] bit NULL,
    [CreatedAtUtc] datetimeoffset NOT NULL,
    [UpdatedAtUtc] datetimeoffset NOT NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAtUtc] datetimeoffset NULL,
    CONSTRAINT [PK_עובדים] PRIMARY KEY ([מזהה_עובד]),
    CONSTRAINT [FK_עובדים_מעסיקים_מזהה_מעסיק] FOREIGN KEY ([מזהה_מעסיק]) REFERENCES [מעסיקים] ([מזהה_מעסיק]) ON DELETE NO ACTION
);
GO


CREATE TABLE [נתוני_העסקה] (
    [מזהה_נתון_העסקה] int NOT NULL IDENTITY,
    [מזהה_עובד] int NOT NULL,
    [מזהה_מעסיק] int NOT NULL,
    [שנת_לימודים] nvarchar(20) NOT NULL,
    [דרגה1_סהכ] decimal(18,2) NULL,
    [דרגה1_אחוז_משרה] decimal(18,2) NULL,
    [דרגה1_קרן_השתלמות_אחוז] decimal(18,2) NULL,
    [דרגה1_שעות_גיל] decimal(18,2) NULL,
    [דרגה1_אחוז_תוספת_אם] decimal(18,2) NULL,
    [דרגה2_סהכ] decimal(18,2) NULL,
    [דרגה2_אחוז_משרה] decimal(18,2) NULL,
    [דרגה2_קרן_השתלמות_אחוז] decimal(18,2) NULL,
    [דרגה2_שעות_גיל] decimal(18,2) NULL,
    [דרגה2_אחוז_תוספת_אם] decimal(18,2) NULL,
    [דרגה1_שם_הדירוג] nvarchar(max) NULL,
    [דרגה1_דרגה] nvarchar(max) NULL,
    [דרגה1_תפקיד] nvarchar(max) NULL,
    [דרגה1_ותק] nvarchar(max) NULL,
    [דרגה2_שם_הדירוג] nvarchar(max) NULL,
    [דרגה2_דרגה] nvarchar(max) NULL,
    [דרגה2_תפקיד] nvarchar(max) NULL,
    [דרגה2_ותק] nvarchar(max) NULL,
    [CreatedAtUtc] datetimeoffset NOT NULL,
    [UpdatedAtUtc] datetimeoffset NOT NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAtUtc] datetimeoffset NULL,
    CONSTRAINT [PK_נתוני_העסקה] PRIMARY KEY ([מזהה_נתון_העסקה]),
    CONSTRAINT [FK_נתוני_העסקה_מעסיקים_מזהה_מעסיק] FOREIGN KEY ([מזהה_מעסיק]) REFERENCES [מעסיקים] ([מזהה_מעסיק]) ON DELETE NO ACTION,
    CONSTRAINT [FK_נתוני_העסקה_עובדים_מזהה_עובד] FOREIGN KEY ([מזהה_עובד]) REFERENCES [עובדים] ([מזהה_עובד]) ON DELETE NO ACTION
);
GO


CREATE TABLE [נתוני_העסקה_מקטע] (
    [מזהה_מקטע] int NOT NULL IDENTITY,
    [מזהה_נתון_העסקה] int NOT NULL,
    [רמת_דרגה] tinyint NOT NULL,
    [אינדקס_מקטע] tinyint NOT NULL,
    [סמל_מוסד] nvarchar(max) NULL,
    [שעות_שבועיות] decimal(18,2) NULL,
    [בסיס_משרה] decimal(18,2) NULL,
    [מקטע_הורה_שעות_נוספות] tinyint NULL,
    CONSTRAINT [PK_נתוני_העסקה_מקטע] PRIMARY KEY ([מזהה_מקטע]),
    CONSTRAINT [FK_נתוני_העסקה_מקטע_נתוני_העסקה_מזהה_נתון_העסקה] FOREIGN KEY ([מזהה_נתון_העסקה]) REFERENCES [נתוני_העסקה] ([מזהה_נתון_העסקה]) ON DELETE CASCADE
);
GO


CREATE UNIQUE INDEX [IX_מעסיקים_חפ] ON [מעסיקים] ([חפ]) WHERE [חפ] IS NOT NULL AND [IsDeleted] = 0;
GO


CREATE INDEX [IX_מעסיקים_שם_מעסיק] ON [מעסיקים] ([שם_מעסיק]);
GO


CREATE INDEX [IX_נתוני_העסקה_מזהה_מעסיק] ON [נתוני_העסקה] ([מזהה_מעסיק]);
GO


CREATE UNIQUE INDEX [IX_נתוני_העסקה_מזהה_עובד_מזהה_מעסיק_שנת_לימודים] ON [נתוני_העסקה] ([מזהה_עובד], [מזהה_מעסיק], [שנת_לימודים]) WHERE [IsDeleted] = 0;
GO


CREATE UNIQUE INDEX [IX_נתוני_העסקה_מקטע_מזהה_נתון_העסקה_רמת_דרגה_אינדקס_מקטע] ON [נתוני_העסקה_מקטע] ([מזהה_נתון_העסקה], [רמת_דרגה], [אינדקס_מקטע]);
GO


CREATE UNIQUE INDEX [IX_סמלי_מוסד_מעסיקים_מזהה_מעסיק_סמל_מוסד] ON [סמלי_מוסד_מעסיקים] ([מזהה_מעסיק], [סמל_מוסד]);
GO


CREATE UNIQUE INDEX [IX_עובדים_מזהה_מעסיק_תז] ON [עובדים] ([מזהה_מעסיק], [תז]) WHERE [IsDeleted] = 0;
GO


CREATE INDEX [IX_עובדים_מספר_עובד] ON [עובדים] ([מספר_עובד]);
GO


CREATE INDEX [IX_AuditLogs_ChangedAtUtc] ON [AuditLogs] ([ChangedAtUtc]);
GO


CREATE UNIQUE INDEX [IX_Users_Username] ON [Users] ([Username]);
GO


