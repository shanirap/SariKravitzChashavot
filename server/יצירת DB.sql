--------------------------------------------------
-- מחיקת ה-DB הקיים אם הוא קיים
--------------------------------------------------
USE master;
GO

IF DB_ID(N'מערכת_שכר') IS NOT NULL
BEGIN
    ALTER DATABASE [מערכת_שכר] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [מערכת_שכר];
END
GO

--------------------------------------------------
-- יצירת DB חדש
--------------------------------------------------
CREATE DATABASE [מערכת_שכר];
GO

USE [מערכת_שכר];
GO

--------------------------------------------------
-- טבלת עובדים
--------------------------------------------------
CREATE TABLE [עובדים] (
    [����_����]          INT IDENTITY(1,1) PRIMARY KEY,
    [����_����]          NVARCHAR(50)  NULL,
    [��]                 NVARCHAR(20)  NOT NULL,
    [��_����]            NVARCHAR(100) NULL,
    [��_�����]           NVARCHAR(100) NULL,
    [�����_����]         DATE          NULL,
    [���]                NVARCHAR(20)  NULL,
    [��]                 NVARCHAR(50)  NULL,
    [�����_����_���_1]  DATE NULL,
    [�����_����_���_2]  DATE NULL,
    [�����_����_���_3]  DATE NULL,
    [�����_����_���_4]  DATE NULL,
    [�����_����_���_5]  DATE NULL,
    [�����_����_���_6]  DATE NULL,
    [�����_����_���_7]  DATE NULL,
    [�����_����_���_8]  DATE NULL,
    [�����_����_���_9]  DATE NULL,
    [�����_����_���_10] DATE NULL
);
GO

--------------------------------------------------
-- ���� �������
--------------------------------------------------
CREATE TABLE [�������] (
    [����_�����] INT IDENTITY(1,1) PRIMARY KEY,
    [��_�����]   NVARCHAR(200) NOT NULL,
    [��]          NVARCHAR(50)  NULL,
    [����_���]   NVARCHAR(50)  NULL,
    [����_����]  NVARCHAR(50)  NULL
);
GO

--------------------------------------------------
-- ���� ����� �����
--------------------------------------------------
CREATE TABLE [�����_�����] (
    [����_����_�����] INT IDENTITY(1,1) PRIMARY KEY,
    [����_����]        INT           NOT NULL,
    [����_�����]       INT           NOT NULL,
    [����_���]         INT           NOT NULL,
    [���_���]          INT           NOT NULL,
    [���_����]         NVARCHAR(50)  NOT NULL,

    -- �����
    [����]             BIT           NOT NULL DEFAULT 1,
    [���_�����]        BIT           NOT NULL DEFAULT 0,
    [���_���]          BIT           NOT NULL DEFAULT 0,
    [������_�����]    BIT           NOT NULL DEFAULT 0,

    -- ����� �����
    [�����_1]          NVARCHAR(100) NULL,  -- �����
    [��_����]          NVARCHAR(100) NULL,
    [����]             NVARCHAR(50)  NULL,
    [���]              NVARCHAR(50)  NULL,
    [�����]            NVARCHAR(100) NULL,
    [����_���_1]       DECIMAL(18,2) NULL,  -- ����
    [����_�����_1]     DECIMAL(18,2) NULL,  -- ����
    [������_�������]  DECIMAL(18,2) NULL,
    [������_���_����] DECIMAL(18,2) NULL,
    [�����_���]        DECIMAL(18,2) NULL,
    [���_�������]      DECIMAL(18,2) NULL,
    [����_�����]       DECIMAL(18,2) NULL,
    [����_����]        DECIMAL(5,2)  NULL,
    [����_����]        DECIMAL(5,2)  NULL,
    [����_����]        DECIMAL(18,2) NULL,
    [�����_���]        BIT           NULL,

    -- ����� ���
    [�����_2]          NVARCHAR(100) NULL,  -- ����� 2
    [��_����_2]        NVARCHAR(100) NULL,
    [����_2]           NVARCHAR(50)  NULL,
    [���_2]            NVARCHAR(50)  NULL,
    [�����_2]          NVARCHAR(100) NULL,
    [����_���_2]       DECIMAL(18,2) NULL,
    [����_�����_2]     DECIMAL(18,2) NULL,
    [������_�������_2] DECIMAL(18,2) NULL,
    [������_���_����_2] DECIMAL(18,2) NULL,
    [�����_���_2]      DECIMAL(18,2) NULL,
    [���_�������_2]    DECIMAL(18,2) NULL,
    [����_�����_2]     DECIMAL(18,2) NULL,
    [����_����_2]      DECIMAL(5,2)  NULL,
    [����_����_2]      DECIMAL(5,2)  NULL,
    [����_����_2]      DECIMAL(18,2) NULL,
    [�����_���_2]      BIT           NULL,

    -- ���� ������ ������
    [���_�������_����]  DECIMAL(18,2) NULL,
    [���_�������_����]  DECIMAL(5,2)  NULL,
    [�����_����]        DECIMAL(18,2) NULL,
    [�����_����]        DECIMAL(5,2)  NULL,
    [����_�����]        DECIMAL(18,2) NULL,
    [����_����]         DECIMAL(18,2) NULL,
    [����_�������]      DECIMAL(18,2) NULL,
    [����_���]          DECIMAL(18,2) NULL,
    [����_����_��]      DECIMAL(5,2)  NULL,
    [����_���_3]        DECIMAL(18,2) NULL,

    CONSTRAINT [FK_�����_�����_������]
        FOREIGN KEY ([����_����]) REFERENCES [������]([����_����]),
    CONSTRAINT [FK_�����_�����_�������]
        FOREIGN KEY ([����_�����]) REFERENCES [�������]([����_�����])
);
GO

--------------------------------------------------
-- ���� ����� ������
--------------------------------------------------
CREATE TABLE [�����_������] (
    [����_����_������] INT IDENTITY(1,1) PRIMARY KEY,
    [��_����_������]   NVARCHAR(255) NOT NULL,
    [�����_�����]      DATETIME      NOT NULL DEFAULT GETDATE(),
    [����_���]         INT           NULL,
    [���_���]          INT           NULL,
    [�����]            NVARCHAR(MAX) NULL
);
GO

--------------------------------------------------
-- ���� ������ ������
--------------------------------------------------
CREATE TABLE [������_������] (
    [����_�����_������]  INT IDENTITY(1,1) PRIMARY KEY,
    [����_����_������]   INT           NOT NULL,
    [����_����_�����]    INT           NULL,
    [����_����]          INT           NULL,
    [����_�����]         INT           NULL,
    [���_����]           NVARCHAR(50)  NULL,
    [���_�����]          NVARCHAR(50)  NOT NULL,
    [�����_�����]        NVARCHAR(MAX) NULL,
    [����_����_�����]    INT           NULL,

    CONSTRAINT [FK_������_������_�����_������]
        FOREIGN KEY ([����_����_������]) REFERENCES [�����_������]([����_����_������]),
    CONSTRAINT [FK_������_������_�����_�����]
        FOREIGN KEY ([����_����_�����]) REFERENCES [�����_�����]([����_����_�����]),
    CONSTRAINT [FK_������_������_������]
        FOREIGN KEY ([����_����]) REFERENCES [������]([����_����]),
    CONSTRAINT [FK_������_������_�������]
        FOREIGN KEY ([����_�����]) REFERENCES [�������]([����_�����])
);
GO

--------------------------------------------------
-- ���� ���� ������
--------------------------------------------------
CREATE TABLE [����_������] (
    [����_���_������]     INT IDENTITY(1,1) PRIMARY KEY,
    [����_�����_������]   INT           NOT NULL,
    [��_���]              NVARCHAR(100) NOT NULL,
    [���_�����]           NVARCHAR(MAX) NULL,
    [���_�����]           NVARCHAR(MAX) NULL,
    [��_���]              BIT           NOT NULL,

    CONSTRAINT [FK_����_������_������_������]
        FOREIGN KEY ([����_�����_������]) REFERENCES [������_������]([����_�����_������])
);
GO

--------------------------------------------------
-- ��������
--------------------------------------------------
CREATE INDEX [IX_�����_�����_����_����]    ON [�����_�����]([����_����]);
CREATE INDEX [IX_�����_�����_����_�����]  ON [�����_�����]([����_�����]);
CREATE INDEX [IX_�����_�����_���_����]    ON [�����_�����]([���_����]);
CREATE INDEX [IX_�����_�����_�����]        ON [�����_�����]([����_���],[���_���]);
GO
