using System.ComponentModel.DataAnnotations;

namespace EcobayApp.Models
{
	public class PageCity
	{
		[Key]
		public long Id { get; set; }

		/// <summary>Заголовок страницы (Title).</summary>
		[Required]
		[MaxLength(160)]
		public string Title { get; set; } = null!;

		/// <summary>Meta Description.</summary>
		[MaxLength(300)]
		public string? Description { get; set; }

		/// <summary>Meta Keywords.</summary>
		[MaxLength(300)]
		public string? KeyWords { get; set; }

		/// <summary>Город (навигация).</summary>
		[Required]
		public long CityId { get; set; }
		public City City { get; set; } = null!;

		/// <summary>Цена за кг для этой страницы (если null — брать из City.DefaultPrice).</summary>
		public decimal? Price { get; set; }

		/// <summary>Телефон для этой страницы (если null — брать City.DefaultPhone).</summary>
		[MaxLength(50)]
		public string? PhoneCity { get; set; }

		/// <summary>Показывать промо сообщение?</summary>
		public bool IsShowPromo { get; set; } = false;

		/// <summary>Текст промо (если нужно).</summary>
		[MaxLength(500)]
		public string? PromoText { get; set; }
	}
}
