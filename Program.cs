using EcobayApp.Components;
using EcobayApp.Contex;
using EcobayApp.Models;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// строка подключения из appsettings.json
var connectionString = builder.Configuration.GetConnectionString("EcobayConnection");

builder.Services.AddDbContext<AppDBContext>(options =>
	options.UseSqlServer(connectionString));

builder.Services.Configure<EmailSettings>(
	builder.Configuration.GetSection("Email"));

builder.Services.AddScoped<IEmailSender, EmailSender>();

builder.Environment.EnvironmentName = "Development";

var app = builder.Build();

//using (var scope = app.Services.CreateScope())
//{
//	var db = scope.ServiceProvider.GetRequiredService<AppDBContext>();

//	// создаст БД/таблицы по миграциям, если их ещё нет
//	db.Database.Migrate();
//}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
