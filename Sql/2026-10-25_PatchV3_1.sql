BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261025124845_AddRouteSheetsAndPickupTime'
)
BEGIN
    ALTER TABLE [Requests] ADD [PickupDate] date NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261025124845_AddRouteSheetsAndPickupTime'
)
BEGIN
    ALTER TABLE [Requests] ADD [PickupTime] time NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261025124845_AddRouteSheetsAndPickupTime'
)
BEGIN
    ALTER TABLE [Requests] ADD [RouteDate] date NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261025124845_AddRouteSheetsAndPickupTime'
)
BEGIN
    ALTER TABLE [Requests] ADD [RouteDriverId] bigint NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261025124845_AddRouteSheetsAndPickupTime'
)
BEGIN
    ALTER TABLE [Requests] ADD [RouteOrder] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261025124845_AddRouteSheetsAndPickupTime'
)
BEGIN
    CREATE INDEX [IX_Requests_RouteDriverId_RouteDate] ON [Requests] ([RouteDriverId], [RouteDate]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261025124845_AddRouteSheetsAndPickupTime'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261025124845_AddRouteSheetsAndPickupTime', N'8.0.22');
END;
GO

COMMIT;
GO

