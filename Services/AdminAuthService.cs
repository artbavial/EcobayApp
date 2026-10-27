using System.Security.Claims;
using EcobayApp.Contex;
using EcobayApp.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EcobayApp.Services
{
	public enum AdminLoginStatus
	{
		Success,
		InvalidCredentials,
		LockedOut,
		Disabled
	}

	public record AdminLoginResult(AdminLoginStatus Status, AdminUser? User = null, DateTime? LockoutUntil = null);

	/// <summary>
	/// Аутентификация пользователей панели управления:
	/// хеширование паролей, блокировка после серии неудачных попыток, выпуск claims для cookie-сессии.
	/// </summary>
	public class AdminAuthService
	{
		public const string Scheme = CookieAuthenticationDefaults.AuthenticationScheme;
		public const string SecurityStampClaim = "ecobay:stamp";

		/// <summary>Сколько неудачных попыток подряд разрешено до блокировки.</summary>
		public const int MaxFailedAttempts = 5;

		/// <summary>На сколько блокируется вход.</summary>
		public static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

		public const int MinPasswordLength = 8;

		private readonly IDbContextFactory<AppDBContext> _dbFactory;
		private readonly IPasswordHasher<AdminUser> _hasher;
		private readonly ILogger<AdminAuthService> _logger;

		public AdminAuthService(
			IDbContextFactory<AppDBContext> dbFactory,
			IPasswordHasher<AdminUser> hasher,
			ILogger<AdminAuthService> logger)
		{
			_dbFactory = dbFactory;
			_hasher = hasher;
			_logger = logger;
		}

		public static string NormalizeLogin(string? login) => (login ?? string.Empty).Trim().ToLowerInvariant();

		public async Task<AdminLoginResult> ValidateCredentialsAsync(string? login, string? password)
		{
			var normalized = NormalizeLogin(login);
			if (normalized.Length == 0 || string.IsNullOrEmpty(password))
				return new AdminLoginResult(AdminLoginStatus.InvalidCredentials);

			await using var db = await _dbFactory.CreateDbContextAsync();
			var user = await db.AdminUsers.FirstOrDefaultAsync(u => u.Login == normalized);

			if (user is null)
			{
				// Тратим примерно то же время, что и на проверку реального хеша,
				// чтобы по времени ответа нельзя было угадать существующие логины.
				_hasher.HashPassword(new AdminUser { Login = normalized, PasswordHash = "" }, password);
				_logger.LogWarning("Admin login failed: unknown login '{Login}'", normalized);
				return new AdminLoginResult(AdminLoginStatus.InvalidCredentials);
			}

			var now = DateTime.Now;

			if (user.LockoutUntil.HasValue && user.LockoutUntil.Value > now)
				return new AdminLoginResult(AdminLoginStatus.LockedOut, null, user.LockoutUntil);

			if (!user.IsActive || !AdminRoles.IsKnown(user.Role))
				return new AdminLoginResult(AdminLoginStatus.Disabled);

			var verify = _hasher.VerifyHashedPassword(user, user.PasswordHash, password);

			if (verify == PasswordVerificationResult.Failed)
			{
				user.FailedLoginCount++;
				if (user.FailedLoginCount >= MaxFailedAttempts)
				{
					user.LockoutUntil = now.Add(LockoutDuration);
					user.FailedLoginCount = 0;
					await db.SaveChangesAsync();
					_logger.LogWarning("Admin '{Login}' locked out until {Until}", user.Login, user.LockoutUntil);
					return new AdminLoginResult(AdminLoginStatus.LockedOut, null, user.LockoutUntil);
				}

				await db.SaveChangesAsync();
				_logger.LogWarning("Admin login failed for '{Login}' ({Count} attempt)", user.Login, user.FailedLoginCount);
				return new AdminLoginResult(AdminLoginStatus.InvalidCredentials);
			}

			if (verify == PasswordVerificationResult.SuccessRehashNeeded)
				user.PasswordHash = _hasher.HashPassword(user, password);

			user.FailedLoginCount = 0;
			user.LockoutUntil = null;
			user.LastLoginAt = now;
			await db.SaveChangesAsync();

			_logger.LogInformation("Admin '{Login}' signed in", user.Login);
			return new AdminLoginResult(AdminLoginStatus.Success, user);
		}

		public ClaimsPrincipal CreatePrincipal(AdminUser user)
		{
            if (!AdminRoles.IsKnown(user.Role)) throw new InvalidOperationException("Unknown user role.");
			var claims = new List<Claim>
			{
				new(ClaimTypes.NameIdentifier, user.Id.ToString()),
				new(ClaimTypes.Name, user.Login),
				new("ecobay:display", string.IsNullOrWhiteSpace(user.DisplayName) ? user.Login : user.DisplayName!),
				new(SecurityStampClaim, user.SecurityStamp),
				new(ClaimTypes.Role, user.Role)
			};

			return new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme));
		}

		/// <summary>
		/// Проверка, что сессия всё ещё действительна: пользователь существует, активен
		/// и его пароль не менялся (метка безопасности совпадает).
		/// </summary>
		public async Task<bool> IsPrincipalValidAsync(ClaimsPrincipal? principal)
		{
			if (principal?.Identity?.IsAuthenticated != true)
				return false;

			var idValue = principal.FindFirstValue(ClaimTypes.NameIdentifier);
			var stamp = principal.FindFirstValue(SecurityStampClaim);
            var role = principal.FindFirstValue(ClaimTypes.Role);
            if (!AdminRoles.IsKnown(role)) return false;
			if (!long.TryParse(idValue, out var id) || string.IsNullOrEmpty(stamp))
				return false;

			await using var db = await _dbFactory.CreateDbContextAsync();
			return await db.AdminUsers
				.AsNoTracking()
				.AnyAsync(u => u.Id == id && u.IsActive && u.SecurityStamp == stamp && u.Role == role);
		}

        /// <summary>
        /// Проверяет право удаления по серверному principal и актуальной записи в БД.
        /// Право одинаково у всех пользователей роли Admin; логин значения не имеет.
        /// </summary>
        public async Task<bool> CanDeleteRequestsAsync(ClaimsPrincipal? principal)
        {
            if (principal?.Identity?.IsAuthenticated != true || !principal.IsInRole(AdminRoles.Admin))
                return false;

            var idValue = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            var stamp = principal.FindFirstValue(SecurityStampClaim);
            if (!long.TryParse(idValue, out var id) || string.IsNullOrEmpty(stamp))
                return false;

            await using var db = await _dbFactory.CreateDbContextAsync();
            return await db.AdminUsers.AsNoTracking().AnyAsync(u =>
                u.Id == id && u.Role == AdminRoles.Admin &&
                u.IsActive && u.SecurityStamp == stamp);
        }

		public static string? ValidatePasswordStrength(string? password)
		{
			if (string.IsNullOrEmpty(password) || password.Length < MinPasswordLength)
				return $"Пароль должен быть не короче {MinPasswordLength} символов.";
			if (!password.Any(char.IsLetter) || !password.Any(char.IsDigit))
				return "Пароль должен содержать буквы и цифры.";
			return null;
		}

		public string HashPassword(AdminUser user, string password) => _hasher.HashPassword(user, password);

		public bool VerifyPassword(AdminUser user, string password) =>
			_hasher.VerifyHashedPassword(user, user.PasswordHash, password) != PasswordVerificationResult.Failed;
	}
}
