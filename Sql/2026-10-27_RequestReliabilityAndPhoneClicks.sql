-- Run against the existing Ecobay SQL Server database before starting the updated app.
-- Alternative to dotnet ef database update. Do not run both on different database copies.
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
    THROW 51000, 'This script requires an existing Ecobay database with applied migrations.', 1;
IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20261027171900_AddRequestReliabilityAndPhoneClicks')
BEGIN
    CREATE TABLE [RequestSubmissions] (
        [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_RequestSubmissions] PRIMARY KEY,
        [RequestId] bigint NOT NULL,
        [Fingerprint] nvarchar(64) NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL
    );
    CREATE INDEX [IX_RequestSubmissions_Fingerprint_CreatedAtUtc] ON [RequestSubmissions] ([Fingerprint], [CreatedAtUtc]);
    CREATE TABLE [RequestEmailOutbox] (
        [Id] bigint IDENTITY(1,1) NOT NULL CONSTRAINT [PK_RequestEmailOutbox] PRIMARY KEY,
        [RequestId] bigint NOT NULL,
        [PayloadJson] nvarchar(max) NOT NULL,
        [Attempts] int NOT NULL,
        [AvailableAtUtc] datetime2 NOT NULL,
        [LeaseUntilUtc] datetime2 NULL,
        [SentAtUtc] datetime2 NULL,
        [LastError] nvarchar(1000) NULL
    );
    CREATE UNIQUE INDEX [IX_RequestEmailOutbox_RequestId] ON [RequestEmailOutbox] ([RequestId]);
    CREATE INDEX [IX_RequestEmailOutbox_SentAtUtc_AvailableAtUtc] ON [RequestEmailOutbox] ([SentAtUtc], [AvailableAtUtc]);
    CREATE TABLE [PhoneClicks] (
        [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_PhoneClicks] PRIMARY KEY,
        [CityId] bigint NULL,
        [CityName] nvarchar(120) NOT NULL,
        [Path] nvarchar(300) NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL
    );
    CREATE INDEX [IX_PhoneClicks_CreatedAtUtc_CityId] ON [PhoneClicks] ([CreatedAtUtc], [CityId]);
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261027171900_AddRequestReliabilityAndPhoneClicks', N'8.0.22');
END;
COMMIT;
