BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261025082351_AddCityActiveMainPageAdminAuth'
)
BEGIN
    ALTER TABLE [Cities] ADD [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261025082351_AddCityActiveMainPageAdminAuth'
)
BEGIN
    ALTER TABLE [Cities] ADD [SortOrder] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261025082351_AddCityActiveMainPageAdminAuth'
)
BEGIN
    CREATE TABLE [AdminUsers] (
        [Id] bigint NOT NULL IDENTITY,
        [Login] nvarchar(64) NOT NULL,
        [DisplayName] nvarchar(100) NULL,
        [PasswordHash] nvarchar(500) NOT NULL,
        [SecurityStamp] nvarchar(64) NOT NULL,
        [IsActive] bit NOT NULL,
        [MustChangePassword] bit NOT NULL,
        [FailedLoginCount] int NOT NULL,
        [LockoutUntil] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [LastLoginAt] datetime2 NULL,
        CONSTRAINT [PK_AdminUsers] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261025082351_AddCityActiveMainPageAdminAuth'
)
BEGIN
    CREATE TABLE [MainPageSettings] (
        [Id] bigint NOT NULL IDENTITY,
        [Title] nvarchar(160) NOT NULL,
        [Description] nvarchar(300) NULL,
        [KeyWords] nvarchar(300) NULL,
        [HeroTitle] nvarchar(200) NULL,
        [HeroText] nvarchar(600) NULL,
        [IsCityListShown] bit NOT NULL,
        [Phone] nvarchar(50) NULL,
        [Price] decimal(18,2) NULL,
        [PriceToKopeks] int NULL,
        [IsDeliveryPriceMode] bit NOT NULL,
        [IsCitySelectorEnabled] bit NOT NULL,
        [IsRememberCityEnabled] bit NOT NULL,
        [CitySelectorTitle] nvarchar(160) NULL,
        [CitySelectorText] nvarchar(400) NULL,
        [IsGlobalPromoShownOnCityPages] bit NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_MainPageSettings] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261025082351_AddCityActiveMainPageAdminAuth'
)
BEGIN
    CREATE UNIQUE INDEX [IX_AdminUsers_Login] ON [AdminUsers] ([Login]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261025082351_AddCityActiveMainPageAdminAuth'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261025082351_AddCityActiveMainPageAdminAuth', N'8.0.22');
END;
GO

COMMIT;
GO

