using System.ComponentModel.DataAnnotations;

namespace EcobayApp.Models
{
	public class EmailSettings
	{
		[Key]
		public long Id { get; set; }

		[MaxLength(200)]
		public string SmtpHost { get; set; } = null!;

		public int SmtpPort { get; set; }

		[MaxLength(200)]
		public string SmtpUser { get; set; } = null!;

		[MaxLength(200)]
		public string SmtpPass { get; set; } = null!;

		[MaxLength(200)]
		public string From { get; set; } = null!;

		[MaxLength(200)]
		public string To { get; set; } = null!;
	}
}
