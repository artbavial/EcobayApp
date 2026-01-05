using System.ComponentModel.DataAnnotations;

namespace EcobayApp.Models
{
	public class Request
	{
		[Key]
		public long Id { get; set; }

		/// <summary>Клиент, который оставил заявку.</summary>
		[Required]
		public long ClientId { get; set; }
		public Client Client { get; set; } = null!;

		/// <summary>Посадочная страница, с которой пришла заявка (города/URL).</summary>
		public long? PageCityId { get; set; }
		public PageCity? PageCity { get; set; }

		/// <summary>Дата и время создания.</summary>
		public DateTime DateCreate { get; set; } = DateTime.Now;

		/// <summary>Источник/реферер (если захочешь логировать).</summary>
		[MaxLength(300)]
		public string? Source { get; set; }

		/// <summary>Создать заявку для клиента.</summary>
		public static Request Create(Client client, PageCity? pageCity = null, string? source = null)
		{
			return new Request
			{
				Client = client,
				ClientId = client.Id,
				PageCity = pageCity,
				PageCityId = pageCity?.Id,
				DateCreate = DateTime.Now,
				Source = source
			};
		}
	}
}
