BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261027211554_AddRequestWeightAndCalls'
)
BEGIN
    ALTER TABLE [Requests] ADD [CustomerCalledAtUtc] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261027211554_AddRequestWeightAndCalls'
)
BEGIN
    ALTER TABLE [Requests] ADD [CustomerCalledByName] nvarchar(100) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261027211554_AddRequestWeightAndCalls'
)
BEGIN
    ALTER TABLE [Requests] ADD [CustomerCalledByUserId] bigint NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261027211554_AddRequestWeightAndCalls'
)
BEGIN
    ALTER TABLE [Requests] ADD [WeightKg] decimal(12,2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261027211554_AddRequestWeightAndCalls'
)
BEGIN
    ALTER TABLE [RequestEvents] ADD [WeightKg] decimal(12,2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261027211554_AddRequestWeightAndCalls'
)
BEGIN
    CREATE TABLE [RequestContactEvents] (
        [Id] bigint NOT NULL IDENTITY,
        [RequestId] bigint NOT NULL,
        [IsCalled] bit NOT NULL,
        [CallAtUtc] datetime2 NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UserId] bigint NOT NULL,
        [UserName] nvarchar(100) NOT NULL,
        CONSTRAINT [PK_RequestContactEvents] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RequestContactEvents_Requests_RequestId] FOREIGN KEY ([RequestId]) REFERENCES [Requests] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261027211554_AddRequestWeightAndCalls'
)
BEGIN
    CREATE INDEX [IX_Requests_Status_ProcessedAt] ON [Requests] ([Status], [ProcessedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261027211554_AddRequestWeightAndCalls'
)
BEGIN
    CREATE INDEX [IX_RequestContactEvents_RequestId_CreatedAtUtc] ON [RequestContactEvents] ([RequestId], [CreatedAtUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261027211554_AddRequestWeightAndCalls'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261027211554_AddRequestWeightAndCalls', N'8.0.22');
END;
GO

COMMIT;
GO

