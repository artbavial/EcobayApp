using System.ComponentModel.DataAnnotations;

namespace EcobayApp.Models
{
	public class Client
	{
		[Key]
		public long Id { get; set; }

		[Required]
		[MaxLength(100)]
		public string Name { get; set; } = null!;

		/// <summary>Город клиента (откуда заявка).</summary>
		public long? CityId { get; set; }
		public City? City { get; set; }

		[Required]
		[MaxLength(50)]
		public string Phone { get; set; } = null!;

		/// <summary>Комментарий из формы.</summary>
		[MaxLength(500)]
		public string? Comment { get; set; }

		/// <summary>Связанные заявки.</summary>
		public ICollection<Request> Requests { get; set; } = new List<Request>();


		/// <summary>Создать клиента из полей формы.</summary>
		public static Client Create(string name, string phone, string? comment = null, City? city = null)
		{
			return new Client
			{
				Name = name,
				Phone = phone,
				Comment = comment,
				City = city,
				CityId = city?.Id
			};
		}
	}
}
