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

		public decimal? Price { get; set; }

		public int? PriceToKopeks { get; set; }

		[MaxLength(50)]
		public string? PhoneCity { get; set; }

		public bool IsShowPromo { get; set; } = false;

		[MaxLength(500)]
		public string? PromoText { get; set; }
	}
}