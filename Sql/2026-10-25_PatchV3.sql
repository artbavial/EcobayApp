BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261025101519_AddRolesRequestWorkflowFeedbackAutoGoal'
)
BEGIN
    ALTER TABLE [Requests] ADD [Address] nvarchar(300) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261025101519_AddRolesRequestWorkflowFeedbackAutoGoal'
)
BEGIN
    ALTER TABLE [Requests] ADD [CreatedByUserId] bigint NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261025101519_AddRolesRequestWorkflowFeedbackAutoGoal'
)
BEGIN
    ALTER TABLE [Requests] ADD [DriverComment] nvarchar(1000) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261025101519_AddRolesRequestWorkflowFeedbackAutoGoal'
)
BEGIN
    ALTER TABLE [Requests] ADD [IsManual] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261025101519_AddRolesRequestWorkflowFeedbackAutoGoal'
)
BEGIN
    ALTER TABLE [Requests] ADD [PaidAmount] decimal(10,2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261025101519_AddRolesRequestWorkflowFeedbackAutoGoal'
)
BEGIN
    ALTER TABLE [Requests] ADD [ProcessedAt] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261025101519_AddRolesRequestWorkflowFeedbackAutoGoal'
)
BEGIN
    ALTER TABLE [Requests] ADD [ProcessedByName] nvarchar(100) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261025101519_AddRolesRequestWorkflowFeedbackAutoGoal'
)
BEGIN
    ALTER TABLE [Requests] ADD [ProcessedByUserId] bigint NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261025101519_AddRolesRequestWorkflowFeedbackAutoGoal'
)
BEGIN
    ALTER TABLE [Requests] ADD [ScreenshotPath] nvarchar(260) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261025101519_AddRolesRequestWorkflowFeedbackAutoGoal'
)
BEGIN
    ALTER TABLE [Requests] ADD [Status] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261025101519_AddRolesRequestWorkflowFeedbackAutoGoal'
)
BEGIN
    ALTER TABLE [Goals] ADD [AutoIncrementEnabled] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261025101519_AddRolesRequestWorkflowFeedbackAutoGoal'
)
BEGIN
    ALTER TABLE [Goals] ADD [AutoIncrementFromHour] int NOT NULL DEFAULT 8;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261025101519_AddRolesRequestWorkflowFeedbackAutoGoal'
)
BEGIN
    ALTER TABLE [Goals] ADD [AutoIncrementLastAt] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261025101519_AddRolesRequestWorkflowFeedbackAutoGoal'
)
BEGIN
    ALTER TABLE [Goals] ADD [AutoIncrementPerHour] int NOT NULL DEFAULT 1;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261025101519_AddRolesRequestWorkflowFeedbackAutoGoal'
)
BEGIN
    ALTER TABLE [Goals] ADD [AutoIncrementToHour] int NOT NULL DEFAULT 21;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261025101519_AddRolesRequestWorkflowFeedbackAutoGoal'
)
BEGIN
    ALTER TABLE [AdminUsers] ADD [Role] nvarchar(20) NOT NULL DEFAULT N'Admin';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261025101519_AddRolesRequestWorkflowFeedbackAutoGoal'
)
BEGIN
    CREATE TABLE [Feedbacks] (
        [Id] bigint NOT NULL IDENTITY,
        [Type] int NOT NULL,
        [CityId] bigint NULL,
        [Name] nvarchar(100) NULL,
        [Phone] nvarchar(30) NULL,
        [Text] nvarchar(2000) NOT NULL,
        [SourcePage] nvarchar(300) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [IsProcessed] bit NOT NULL,
        [AdminComment] nvarchar(1000) NULL,
        [ProcessedAt] datetime2 NULL,
        CONSTRAINT [PK_Feedbacks] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Feedbacks_Cities_CityId] FOREIGN KEY ([CityId]) REFERENCES [Cities] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261025101519_AddRolesRequestWorkflowFeedbackAutoGoal'
)
BEGIN
    CREATE TABLE [RequestEvents] (
        [Id] bigint NOT NULL IDENTITY,
        [RequestId] bigint NOT NULL,
        [FromStatus] int NOT NULL,
        [ToStatus] int NOT NULL,
        [Comment] nvarchar(1000) NULL,
        [PaidAmount] decimal(10,2) NULL,
        [UserId] bigint NULL,
        [UserName] nvarchar(100) NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_RequestEvents] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RequestEvents_Requests_RequestId] FOREIGN KEY ([RequestId]) REFERENCES [Requests] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261025101519_AddRolesRequestWorkflowFeedbackAutoGoal'
)
BEGIN
    CREATE INDEX [IX_Requests_Status_DateCreate] ON [Requests] ([Status], [DateCreate]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261025101519_AddRolesRequestWorkflowFeedbackAutoGoal'
)
BEGIN
    CREATE INDEX [IX_Feedbacks_CityId] ON [Feedbacks] ([CityId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261025101519_AddRolesRequestWorkflowFeedbackAutoGoal'
)
BEGIN
    CREATE INDEX [IX_Feedbacks_CreatedAt] ON [Feedbacks] ([CreatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261025101519_AddRolesRequestWorkflowFeedbackAutoGoal'
)
BEGIN
    CREATE INDEX [IX_RequestEvents_RequestId] ON [RequestEvents] ([RequestId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261025101519_AddRolesRequestWorkflowFeedbackAutoGoal'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261025101519_AddRolesRequestWorkflowFeedbackAutoGoal', N'8.0.22');
END;
GO

COMMIT;
GO

