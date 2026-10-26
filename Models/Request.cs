using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

		/// <summary>Сообщение/комментарий из формы.</summary>
		[MaxLength(500)]
		public string? Message { get; set; }

		/// <summary>Дата и время создания.</summary>
		public DateTime DateCreate { get; set; } = DateTime.Now;

		/// <summary>Источник/реферер (если захочешь логировать).</summary>
		[MaxLength(300)]
		public string? Source { get; set; }

		// ---------- Обработка заявки ----------

		/// <summary>Статус: новая, отработана, отменена.</summary>
		public RequestStatus Status { get; set; } = RequestStatus.New;

		/// <summary>Адрес вывоза (для ручных заявок или уточнённый водителем).</summary>
		[MaxLength(300)]
		public string? Address { get; set; }

		/// <summary>Сколько заплатили клиенту, руб.</summary>
		public decimal? PaidAmount { get; set; }

		/// <summary>Комментарий водителя/администратора к последнему действию.</summary>
		[MaxLength(1000)]
		public string? DriverComment { get; set; }

		/// <summary>Когда заявку отработали или отменили.</summary>
		public DateTime? ProcessedAt { get; set; }

		public long? ProcessedByUserId { get; set; }

		[MaxLength(100)]
		public string? ProcessedByName { get; set; }

		/// <summary>Заявка добавлена вручную в панели управления.</summary>
		public bool IsManual { get; set; }

		public long? CreatedByUserId { get; set; }

		/// <summary>Путь к скриншоту относительно App_Data (не публичный каталог).</summary>
		[MaxLength(260)]
		public string? ScreenshotPath { get; set; }

		/// <summary>Когда забрать: дата (без времени).</summary>
		[Column(TypeName = "date")]
		public DateTime? PickupDate { get; set; }

		/// <summary>Когда забрать: время (необязательно).</summary>
		public TimeSpan? PickupTime { get; set; }

		/// <summary>Маршрутный лист: водитель, дата маршрута и порядковый номер точки.</summary>
		public long? RouteDriverId { get; set; }

		[Column(TypeName = "date")]
		public DateTime? RouteDate { get; set; }

		public int RouteOrder { get; set; }

		public List<RequestEvent> Events { get; set; } = new();

		// Удобный доступ к городу
		[NotMapped]
		public City? City => PageCity?.City ?? Client?.City;

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
