using System.ComponentModel.DataAnnotations;

namespace EcobayApp.Models
{
	public class PageCity
	{
		[Key]
		public long Id { get; set; }

		[Required]
		[MaxLength(160)]
		public string Title { get; set; } = null!;

		[MaxLength(300)]
		public string? Description { get; set; }

		[MaxLength(300)]
		public string? KeyWords { get; set; }

		[Required]
		public long CityId { get; set; }
		public City City { get; set; } = null!;

		/// <summary>Optional city-specific first-screen heading and useful local details. Plain text only.</summary>
		[MaxLength(200)]
		public string? HeroTitle { get; set; }

		[MaxLength(1500)]
		public string? LocalIntro { get; set; }

		/// <summary>Actual covered towns/villages of this city's district; do not list unsupported places.</summary>
		[MaxLength(1500)]
		public string? CoverageText { get; set; }

		/// <summary>Real dispatch schedule, pickup conditions, preparation or self-delivery details.</summary>
		[MaxLength(1500)]
		public string? PickupDetails { get; set; }

		/// <summary>
		/// Если true, диапазон цены трактуется как:
		/// ОТ — мы сами приезжаем и вывозим,
		/// ДО — клиент сам привозит технику в передвижной пункт.
		/// </summary>
		public bool IsDeliveryPriceMode { get; set; } = false;

		public decimal? Price { get; set; }

		public int? PriceToKopeks { get; set; }

		[MaxLength(50)]
		public string? PhoneCity { get; set; }

		public bool IsShowPromo { get; set; } = false;

		[MaxLength(500)]
		public string? PromoText { get; set; }
	}
}
