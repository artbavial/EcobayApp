using System.ComponentModel.DataAnnotations;

namespace EcobayApp.Models
{
	/// <summary>
	/// Настройки главной страницы «Вся Беларусь» (открывается по адресу "/",
	/// когда пользователь не выбрал город или его города нет в списке).
	/// В таблице хранится одна запись.
	/// </summary>
	public class MainPageSettings
	{
		[Key]
		public long Id { get; set; }

		// ---------- SEO ----------

		[Required]
		[MaxLength(160)]
		public string Title { get; set; } = "Вывоз техники в наших городах и их районах";

		[MaxLength(300)]
		public string? Description { get; set; }

		[MaxLength(300)]
		public string? KeyWords { get; set; }

		// ---------- Первый экран ----------

		/// <summary>Заголовок первого экрана (H1).</summary>
		[MaxLength(200)]
		public string? HeroTitle { get; set; }

		/// <summary>Текст под заголовком первого экрана. Если пусто — стандартный текст.</summary>
		[MaxLength(600)]
		public string? HeroText { get; set; }

		/// <summary>Показывать на первом экране список городов, где мы работаем.</summary>
		public bool IsCityListShown { get; set; } = true;

		// ---------- Контакты и цены ----------

		[MaxLength(50)]
		public string? Phone { get; set; }

		/// <summary>Цена «от», коп./кг.</summary>
		public decimal? Price { get; set; }

		/// <summary>Цена «до», коп./кг.</summary>
		public int? PriceToKopeks { get; set; }

		/// <summary>Режим «цена зависит от способа вывоза» (как у страниц городов).</summary>
		public bool IsDeliveryPriceMode { get; set; } = false;

		// ---------- Окно выбора города ----------

		/// <summary>Показывать окно выбора города при открытии сайта.</summary>
		public bool IsCitySelectorEnabled { get; set; } = true;

		/// <summary>
		/// Запоминать выбор пользователя: при повторном визите на "/" сразу открывать его город.
		/// </summary>
		public bool IsRememberCityEnabled { get; set; } = true;

		[MaxLength(160)]
		public string? CitySelectorTitle { get; set; }

		[MaxLength(400)]
		public string? CitySelectorText { get; set; }

		// ---------- Промо ----------

		/// <summary>
		/// Показывать общее промо (промо без города) и на страницах городов,
		/// у которых нет собственного активного промо.
		/// </summary>
		public bool IsGlobalPromoShownOnCityPages { get; set; } = true;

		public DateTime UpdatedAt { get; set; } = DateTime.Now;
	}
}
