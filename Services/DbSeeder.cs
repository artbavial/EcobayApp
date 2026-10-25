using EcobayApp.Contex;
using EcobayApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EcobayApp.Services
{
	/// <summary>
	/// Начальное заполнение БД при старте приложения (вместо заполнения при каждом открытии страницы):
	/// базовые города, настройки страницы «Вся Беларусь» и первый администратор из конфигурации.
	/// </summary>
	public static class DbSeeder
	{
		public const string DefaultPhone = "+375 (29) 991-06-66";

		public static async Task SeedAsync(IServiceProvider services, IConfiguration config, ILogger logger)
		{
			await using var scope = services.CreateAsyncScope();
			var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDBContext>>();
			await using var db = await dbFactory.CreateDbContextAsync();

			if (config.GetValue<bool>("Database:ApplyMigrationsOnStartup"))
			{
				logger.LogInformation("Applying EF Core migrations...");
				await db.Database.MigrateAsync();
			}

			await SeedCitiesAsync(db);
			await SeedMainPageAsync(db);
			await SeedAdminAsync(db, scope.ServiceProvider.GetRequiredService<IPasswordHasher<AdminUser>>(), config, logger);
		}

		private static async Task SeedCitiesAsync(AppDBContext db)
		{
			if (await db.Cities.AnyAsync())
				return;

			const decimal basePrice = 20m;
			const int basePriceToKopeks = 35;

			var cities = new List<City>
			{
				new() { Name = "Гомель",      NameIn = "Гомеле",      Slug = "gomel",       DefaultPhone = DefaultPhone, DefaultPrice = basePrice },
				new() { Name = "Рогачёв",     NameIn = "Рогачёве",    Slug = "rogachev",    DefaultPhone = DefaultPhone, DefaultPrice = basePrice },
				new() { Name = "Бобруйск",    NameIn = "Бобруйске",   Slug = "bobruisk",    DefaultPhone = DefaultPhone, DefaultPrice = basePrice },
				new() { Name = "Жлобин",      NameIn = "Жлобине",     Slug = "zhlobin",     DefaultPhone = DefaultPhone, DefaultPrice = basePrice },
				new() { Name = "Светлогорск", NameIn = "Светлогорске", Slug = "svetlogorsk", DefaultPhone = DefaultPhone, DefaultPrice = basePrice }
			};

			db.Cities.AddRange(cities);
			await db.SaveChangesAsync();

			db.PageCities.AddRange(cities.Select(c => new PageCity
			{
				CityId = c.Id,
				Title = $"Вывоз бытовой техники в {c.NameIn}",
				Description = $"Быстрый и выгодный вывоз бытовой техники в {c.NameIn}.",
				KeyWords = $"вывоз техники {c.Name}, утилизация техники {c.Name}, приём техники {c.Name}",
				Price = basePrice,
				PriceToKopeks = basePriceToKopeks,
				PhoneCity = DefaultPhone
			}));
			await db.SaveChangesAsync();
		}

		private static async Task SeedMainPageAsync(AppDBContext db)
		{
			if (await db.MainPageSettings.AnyAsync())
				return;

			db.MainPageSettings.Add(CreateDefaultMainPage());
			await db.SaveChangesAsync();
		}

		public static MainPageSettings CreateDefaultMainPage() => new()
		{
			Title = "Вывоз техники в наших городах и их районах",
			Description = "Вывоз техники только в указанных на сайте городах и населённых пунктах их районов. Выберите город и узнайте условия.",
			KeyWords = "вывоз техники, утилизация техники, приём бытовой техники, утилизация электроники, Беларусь",
			HeroTitle = "Вывоз техники в наших городах и их районах",
			Phone = DefaultPhone,
			Price = 20m,
			IsCityListShown = true,
			IsCitySelectorEnabled = true,
			IsRememberCityEnabled = true,
			IsGlobalPromoShownOnCityPages = true,
			CitySelectorTitle = "Выберите ваш город",
			CitySelectorText = "Покажем цены, телефон и условия вывоза именно для вашего города."
		};

		private static async Task SeedAdminAsync(AppDBContext db, IPasswordHasher<AdminUser> hasher, IConfiguration config, ILogger logger)
		{
			if (await db.AdminUsers.AnyAsync())
				return;

			var login = AdminAuthService.NormalizeLogin(config["AdminSeed:Login"]);
			var password = config["AdminSeed:Password"];

			if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
			{
				logger.LogWarning(
					"В БД нет ни одного администратора, а в конфигурации не задан AdminSeed:Login / AdminSeed:Password. " +
					"Вход в панель управления невозможен, пока не будет задан первый пользователь.");
				return;
			}

			var user = new AdminUser
			{
				Login = login,
				DisplayName = "Администратор",
				IsActive = true,
				MustChangePassword = true,
				PasswordHash = ""
			};
			user.PasswordHash = hasher.HashPassword(user, password);

			db.AdminUsers.Add(user);
			await db.SaveChangesAsync();

			logger.LogWarning("Создан первый администратор '{Login}' из конфигурации AdminSeed. Смените пароль после входа.", login);
		}
	}
}
