using System.ComponentModel.DataAnnotations;

namespace EcobayApp.Models
{
	public enum RequestStatus
	{
		New = 0,
		Done = 1,
		Cancelled = 2
	}

	/// <summary>История обработки заявки: кто, когда и что сделал (включая возврат из отмены).</summary>
	public class RequestEvent
	{
		[Key]
		public long Id { get; set; }

		public long RequestId { get; set; }
		public Request Request { get; set; } = null!;

		public RequestStatus FromStatus { get; set; }
		public RequestStatus ToStatus { get; set; }

		[MaxLength(1000)]
		public string? Comment { get; set; }

		public decimal? PaidAmount { get; set; }
        public decimal? WeightKg { get; set; }

		public long? UserId { get; set; }

		[MaxLength(100)]
		public string? UserName { get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.Now;
	}
}
