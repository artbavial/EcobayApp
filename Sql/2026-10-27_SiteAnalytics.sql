BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261027191939_AddSiteAnalytics'
)
BEGIN
    ALTER TABLE [Requests] ADD [AnalyticsVisitId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261027191939_AddSiteAnalytics'
)
BEGIN
    CREATE TABLE [AnalyticsEvents] (
        [Id] uniqueidentifier NOT NULL,
        [VisitId] uniqueidentifier NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [Kind] nvarchar(32) NOT NULL,
        [Path] nvarchar(300) NOT NULL,
        [CityId] bigint NULL,
        [DailyHost] nvarchar(64) NULL,
        [Location] nvarchar(40) NOT NULL,
        [Weight] decimal(12,2) NULL,
        [AmountFrom] decimal(12,2) NULL,
        [AmountTo] decimal(12,2) NULL,
        CONSTRAINT [PK_AnalyticsEvents] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261027191939_AddSiteAnalytics'
)
BEGIN
    CREATE TABLE [AnalyticsStates] (
        [Id] int NOT NULL IDENTITY,
        [InstalledAtUtc] datetime2 NOT NULL,
        CONSTRAINT [PK_AnalyticsStates] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261027191939_AddSiteAnalytics'
)
BEGIN
    CREATE TABLE [AnalyticsVisits] (
        [Id] uniqueidentifier NOT NULL,
        [VisitorId] uniqueidentifier NOT NULL,
        [StartedAtUtc] datetime2 NOT NULL,
        [LastSeenAtUtc] datetime2 NOT NULL,
        [CityId] bigint NULL,
        [CityName] nvarchar(120) NOT NULL,
        [LandingPath] nvarchar(300) NOT NULL,
        [Source] nvarchar(40) NOT NULL,
        [Campaign] nvarchar(200) NOT NULL,
        [Content] nvarchar(200) NOT NULL,
        [Term] nvarchar(200) NOT NULL,
        [ReferrerHost] nvarchar(253) NOT NULL,
        [Device] nvarchar(16) NOT NULL,
        [Browser] nvarchar(24) NOT NULL,
        CONSTRAINT [PK_AnalyticsVisits] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261027191939_AddSiteAnalytics'
)
BEGIN
    CREATE INDEX [IX_Requests_AnalyticsVisitId] ON [Requests] ([AnalyticsVisitId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261027191939_AddSiteAnalytics'
)
BEGIN
    CREATE INDEX [IX_AnalyticsEvents_CreatedAtUtc_Kind] ON [AnalyticsEvents] ([CreatedAtUtc], [Kind]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261027191939_AddSiteAnalytics'
)
BEGIN
    CREATE INDEX [IX_AnalyticsEvents_VisitId_CreatedAtUtc] ON [AnalyticsEvents] ([VisitId], [CreatedAtUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261027191939_AddSiteAnalytics'
)
BEGIN
    CREATE INDEX [IX_AnalyticsVisits_StartedAtUtc_CityId] ON [AnalyticsVisits] ([StartedAtUtc], [CityId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261027191939_AddSiteAnalytics'
)
BEGIN
    INSERT INTO [AnalyticsStates] ([InstalledAtUtc]) VALUES (SYSUTCDATETIME());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261027191939_AddSiteAnalytics'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261027191939_AddSiteAnalytics', N'8.0.22');
END;
GO

COMMIT;
GO

