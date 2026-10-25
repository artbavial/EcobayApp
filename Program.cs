using System.Threading.RateLimiting;
using EcobayApp.Components;
using EcobayApp.Contex;
using EcobayApp.Models;
using EcobayApp.Services;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ВАЖНО: раньше здесь было принудительное builder.Environment.EnvironmentName = "Development".
// На боевом сервере это отключало страницу ошибок и HSTS и показывало пользователям подробности исключений.
// Окружение задаётся переменной ASPNETCORE_ENVIRONMENT (в launchSettings.json для локальной разработки уже Development).

builder.Services.AddRazorComponents()
	.AddInteractiveServerComponents();

// Строка подключения из appsettings.json
var connectionString = builder.Configuration.GetConnectionString("EcobayConnection");

// Фабрика контекстов: AppDBContext по-прежнему можно внедрять напрямую (@inject AppDBContext),
// а в долгоживущих интерактивных компонентах (панель управления) — создавать короткоживущий контекст на операцию.
builder.Services.AddDbContextFactory<AppDBContext>(options =>
	options.UseSqlServer(connectionString));

builder.Services.Configure<EmailSettings>(
	builder.Configuration.GetSection("Email"));

builder.Services.AddScoped<IEmailSender, EmailSender>();

// ---------- Авторизация панели управления (логин + пароль, сессия в защищённой cookie) ----------

// Ключи шифрования cookie/antiforgery храним на диске. Без этого на хостинге (IIS) после перезапуска
// пула приложений генерируются новые ключи — все администраторы «вылетают», а формы падают с ошибкой antiforgery.
var keysPath = builder.Configuration["DataProtection:KeysPath"];
if (string.IsNullOrWhiteSpace(keysPath))
	keysPath = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "keys");
builder.Services.AddDataProtection()
	.SetApplicationName("EcobayApp")
	.PersistKeysToFileSystem(new DirectoryInfo(keysPath));

builder.Services.AddSingleton<IPasswordHasher<AdminUser>, PasswordHasher<AdminUser>>();
builder.Services.AddScoped<AdminAuthService>();

builder.Services
	.AddAuthentication(AdminAuthService.Scheme)
	.AddCookie(AdminAuthService.Scheme, options =>
	{
		options.LoginPath = "/admin/login";
		options.LogoutPath = "/admin/logout";
		options.AccessDeniedPath = "/admin/login";
		options.ReturnUrlParameter = "returnUrl";

		options.Cookie.Name = ".Ecobay.Admin";
		options.Cookie.HttpOnly = true;
		options.Cookie.SameSite = SameSiteMode.Lax;
		options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
		options.Cookie.IsEssential = true;

		// Сессия живёт 8 часов и продлевается при активности.
		// При «Запомнить меня» — 14 дней (задаётся при входе через AuthenticationProperties).
		options.ExpireTimeSpan = TimeSpan.FromHours(8);
		options.SlidingExpiration = true;

		// Каждый запрос проверяем, что пользователь не отключён и пароль не менялся.
		options.Events.OnValidatePrincipal = async context =>
		{
			var auth = context.HttpContext.RequestServices.GetRequiredService<AdminAuthService>();
			if (!await auth.IsPrincipalValidAsync(context.Principal))
			{
				context.RejectPrincipal();
				await context.HttpContext.SignOutAsync(AdminAuthService.Scheme);
			}
		};
	});

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, AdminRevalidatingAuthStateProvider>();

// Ограничение частоты попыток входа: не более 10 POST-запросов на /admin/login в минуту с одного IP.
builder.Services.AddRateLimiter(options =>
{
	options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
	options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(http =>
	{
		var isLoginPost = HttpMethods.IsPost(http.Request.Method) &&
			http.Request.Path.StartsWithSegments("/admin/login", StringComparison.OrdinalIgnoreCase);

		if (!isLoginPost)
			return RateLimitPartition.GetNoLimiter("other");

		var ip = http.Connection.RemoteIpAddress?.ToString() ?? "unknown";
		return RateLimitPartition.GetFixedWindowLimiter("login:" + ip, _ => new FixedWindowRateLimiterOptions
		{
			PermitLimit = 10,
			Window = TimeSpan.FromMinutes(1),
			QueueLimit = 0
		});
	});
});

var app = builder.Build();

// Начальное заполнение БД (города, настройки страницы «Вся Беларусь», первый администратор).
try
{
	await DbSeeder.SeedAsync(app.Services, app.Configuration, app.Logger);
}
catch (Exception ex)
{
	app.Logger.LogError(ex,
		"Не удалось выполнить начальное заполнение БД. Проверьте, что миграции применены (dotnet ef database update).");
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error", createScopeForErrors: true);
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

// Выход из панели управления (POST с antiforgery-токеном, чтобы выход нельзя было вызвать чужой ссылкой).
app.MapPost("/admin/logout", async (HttpContext http, IAntiforgery antiforgery) =>
{
	try
	{
		await antiforgery.ValidateRequestAsync(http);
	}
	catch (AntiforgeryValidationException)
	{
		return Results.BadRequest();
	}

	await http.SignOutAsync(AdminAuthService.Scheme);
	return Results.LocalRedirect("/admin/login?logout=1");
});

app.MapRazorComponents<App>()
	.AddInteractiveServerRenderMode();

app.Run();
