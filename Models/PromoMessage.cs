using System.ComponentModel.DataAnnotations;

namespace EcobayApp.Models
{
	public class PromoMessage
	{
		[Key]
		public long Id { get; set; }

		/// <summary>Заголовок промо.</summary>
		[Required]
		[MaxLength(160)]
		public string Title { get; set; } = null!;

		/// <summary>Текст промо.</summary>
		[Required]
		[MaxLength(500)]
		public string Text { get; set; } = null!;

		/// <summary>Активно ли промо.</summary>
		public bool IsActive { get; set; } = true;

		/// <summary>Если null — промо для всех городов; иначе только для конкретного города.</summary>
		public long? CityId { get; set; }
		public City? City { get; set; }
	}
}
