using System.ComponentModel.DataAnnotations;

namespace EcobayApp.Models
{
	public static class AdminRoles
	{
		public const string Admin = "Admin";
		public const string Driver = "Driver";
		public const string AdminOrDriver = Admin + "," + Driver;

		public static string Title(string? role) => role == Driver ? "Водитель" : "Администратор";
	}

	/// <summary>Пользователь панели управления.</summary>
	public class AdminUser
	{
		[Key]
		public long Id { get; set; }

		/// <summary>Логин (уникальный, хранится в нижнем регистре).</summary>
		[Required]
		[MaxLength(64)]
		public string Login { get; set; } = null!;

		[MaxLength(100)]
		public string? DisplayName { get; set; }

		/// <summary>Хеш пароля (PBKDF2, ASP.NET Core PasswordHasher). Сам пароль нигде не хранится.</summary>
		[Required]
		[MaxLength(500)]
		public string PasswordHash { get; set; } = null!;

		/// <summary>
		/// Метка безопасности. Меняется при смене пароля / блокировке —
		/// все ранее выданные сессии (cookie) этого пользователя становятся недействительными.
		/// </summary>
		[Required]
		[MaxLength(64)]
		public string SecurityStamp { get; set; } = Guid.NewGuid().ToString("N");

		public bool IsActive { get; set; } = true;

		/// <summary>Роль: Admin — вся панель; Driver — только обработка заявок.</summary>
		[Required]
		[MaxLength(20)]
		public string Role { get; set; } = AdminRoles.Admin;

		/// <summary>Требовать смену пароля (выставляется для учётки, созданной из конфигурации).</summary>
		public bool MustChangePassword { get; set; } = false;

		/// <summary>Неудачные попытки входа подряд.</summary>
		public int FailedLoginCount { get; set; }

		/// <summary>До какого момента вход заблокирован после серии неудачных попыток.</summary>
		public DateTime? LockoutUntil { get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.Now;

		public DateTime? LastLoginAt { get; set; }
	}
}
