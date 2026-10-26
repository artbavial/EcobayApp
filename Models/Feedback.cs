using System.ComponentModel.DataAnnotations;

namespace EcobayApp.Models
{
	public enum FeedbackType
	{
		Review = 0,
		Complaint = 1,
		Suggestion = 2
	}

	/// <summary>Отзыв, жалоба или предложение с сайта. На сайте не публикуется — только для администрации.</summary>
	public class Feedback
	{
		[Key]
		public long Id { get; set; }

		public FeedbackType Type { get; set; }

		public long? CityId { get; set; }
		public City? City { get; set; }

		[MaxLength(100)]
		public string? Name { get; set; }

		[MaxLength(30)]
		public string? Phone { get; set; }

		[Required]
		[MaxLength(2000)]
		public string Text { get; set; } = "";

		[MaxLength(300)]
		public string? SourcePage { get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.Now;

		public bool IsProcessed { get; set; }

		[MaxLength(1000)]
		public string? AdminComment { get; set; }

		public DateTime? ProcessedAt { get; set; }
	}
}
