BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261025092536_AddCityLocalContentAndRequestIndex'
)
BEGIN
    ALTER TABLE [PageCities] ADD [CoverageText] nvarchar(1500) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261025092536_AddCityLocalContentAndRequestIndex'
)
BEGIN
    ALTER TABLE [PageCities] ADD [HeroTitle] nvarchar(200) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261025092536_AddCityLocalContentAndRequestIndex'
)
BEGIN
    ALTER TABLE [PageCities] ADD [LocalIntro] nvarchar(1500) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261025092536_AddCityLocalContentAndRequestIndex'
)
BEGIN
    ALTER TABLE [PageCities] ADD [PickupDetails] nvarchar(1500) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261025092536_AddCityLocalContentAndRequestIndex'
)
BEGIN
    CREATE INDEX [IX_Requests_DateCreate_Id] ON [Requests] ([DateCreate], [Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261025092536_AddCityLocalContentAndRequestIndex'
)
BEGIN
    UPDATE [MainPageSettings]
    SET [Title] = N'Вывоз техники в наших городах и их районах'
    WHERE [Title] = N'Вывоз бытовой техники по всей Беларуси';
    UPDATE [MainPageSettings]
    SET [HeroTitle] = N'Вывоз техники в наших городах и их районах'
    WHERE [HeroTitle] = N'Быстрый вывоз техники на утилизацию по всей Беларуси';
    UPDATE [MainPageSettings]
    SET [Description] = N'Вывоз техники только в указанных на сайте городах и населённых пунктах их районов. Выберите город и узнайте условия.'
    WHERE [Description] = N'Вывоз и утилизация бытовой техники по Беларуси: приедем, вынесем, вывезем и заплатим на месте.';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261025092536_AddCityLocalContentAndRequestIndex'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261025092536_AddCityLocalContentAndRequestIndex', N'8.0.22');
END;
GO

COMMIT;
GO

