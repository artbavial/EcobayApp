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
builder.Services.AddSingleton<AnalyticsHostHasher>();
builder.Services.AddScoped<AnalyticsReports>();

// ВАЖНО: раньше здесь было принудительное builder.Environment.EnvironmentName = "Development".
// На боевом сервере это отключало страницу ошибок и HSTS и показывало пользователям подробности исключений.
// Окружение задаётся переменной ASPNETCORE_ENVIRONMENT (в launchSettings.json для локальной разработки уже Development).

builder.Services.AddRazorComponents()
	// Запас по времени: тяжёлые операции в браузере (распознавание скриншота) не должны обрывать соединение.
	.AddInteractiveServerComponents(o => o.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(10))
	.AddHubOptions(o =>
	{
		o.ClientTimeoutInterval = TimeSpan.FromMinutes(2);
		o.KeepAliveInterval = TimeSpan.FromSeconds(15);
	});

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
builder.Services.AddScoped<SitePermissions>();
builder.Services.AddScoped<RequestWorkflow>();
builder.Services.AddScoped<RequestContacts>();
builder.Services.AddScoped<RequestWeightReports>();
builder.Services.AddScoped<PublicRequestService>();
builder.Services.AddHostedService<RequestEmailWorker>();
builder.Services.AddSingleton<RequestFileStore>();

// Каждый час добавляет заданное количество к прогрессу цели (если включено в панели).
builder.Services.AddSingleton<GoalAutoIncrementService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<GoalAutoIncrementService>());

builder.Services
	.AddAuthentication(AdminAuthService.Scheme)
	.AddCookie(AdminAuthService.Scheme, options =>
	{
		options.LoginPath = "/admin/login";
		options.LogoutPath = "/admin/logout";
		// Водитель, открывший раздел администратора, попадает на свою страницу заявок.
		options.AccessDeniedPath = "/driver";
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

        var isPhoneClick = HttpMethods.IsPost(http.Request.Method) &&
            (http.Request.Path == "/api/phone-click" || http.Request.Path == "/api/site-event");
        if (isPhoneClick)
        {
            var clickIp = http.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            return RateLimitPartition.GetFixedWindowLimiter("phone-click:" + clickIp, _ => new FixedWindowRateLimiterOptions
            { PermitLimit = 120, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 });
        }
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

// Скриншоты заявок — только для вошедших администраторов и водителей.
app.MapGet("/admin/files/requests/{id:long}", async (long id, IDbContextFactory<AppDBContext> dbFactory, RequestFileStore store) =>
{
	await using var db = await dbFactory.CreateDbContextAsync();
	var rel = await db.Requests.Where(r => r.Id == id).Select(r => r.ScreenshotPath).FirstOrDefaultAsync();
	var path = store.Resolve(rel);
	return path is null ? Results.NotFound() : Results.File(path, RequestFileStore.ContentType(path));
}).RequireAuthorization(policy => policy.RequireRole(AdminRoles.Admin, AdminRoles.Manager, AdminRoles.Driver));

// Only click events, not successful telephone conversations. No visitor phone/IP is stored.
app.MapPost("/api/phone-click", async Task<IResult> (HttpContext http, PhoneClickInput input, IDbContextFactory<AppDBContext> factory) =>
{
    if (http.Request.ContentLength is > 2048 || input.Id == Guid.Empty) return Results.BadRequest();
    var origin = http.Request.Headers["Origin"].ToString();
    if (http.Request.Headers["Sec-Fetch-Site"] == "cross-site") return Results.StatusCode(403);
    if (!string.IsNullOrEmpty(origin) && (!Uri.TryCreate(origin, UriKind.Absolute, out var originUri) ||
        !string.Equals(originUri.Authority, http.Request.Host.Value, StringComparison.OrdinalIgnoreCase)))
        return Results.StatusCode(403);
    var path = input.Path ?? "/";
    if (path.Length > 300 || !path.StartsWith('/') || path.StartsWith("//") || path.Contains('?') || path.Contains('#'))
        return Results.BadRequest();
    await using var db = await factory.CreateDbContextAsync();
    var slug = path.Trim('/');
    City? city = null;
    if (slug.Length > 0 && slug is not "contacts" and not "user-agreement")
    {
        if (slug.Length > 50 || slug.Any(c => !(c is >= 'a' and <= 'z' or >= '0' and <= '9' or '-')))
            return Results.BadRequest();
        city = await db.Cities.AsNoTracking().FirstOrDefaultAsync(c => c.Slug == slug && c.IsActive);
        if (city is null) return Results.BadRequest();
    }
    if (await db.PhoneClicks.AnyAsync(x => x.Id == input.Id)) return Results.NoContent();
    db.PhoneClicks.Add(new PhoneClick { Id = input.Id, Path = path, CityId = city?.Id,
        CityName = city?.Name ?? "Общая страница", CreatedAtUtc = DateTime.UtcNow });
    try { await db.SaveChangesAsync(); }
    catch (DbUpdateException ex) when (ex.InnerException is Microsoft.Data.SqlClient.SqlException sql && sql.Number is 2601 or 2627)
    { return Results.NoContent(); }
    return Results.NoContent();
});

app.MapSiteAnalytics();
app.MapRazorComponents<App>()
	.AddInteractiveServerRenderMode();

app.Run();
