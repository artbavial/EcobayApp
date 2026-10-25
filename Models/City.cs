using System.ComponentModel.DataAnnotations;

namespace EcobayApp.Models
{
	public class City
	{
		[Key]
		public long Id { get; set; }

		/// <summary>Название города (Гомель, Бобруйск и т.д.).</summary>
		[Required]
		[MaxLength(100)]
		public string Name { get; set; } = null!;

		/// <summary>Название в предложном падеже — в Гомеле, в Бобруйске.</summary>
		[Required]
		[MaxLength(120)]
		public string NameIn { get; set; } = null!;

		/// <summary>Код/slug для URL, например "gomel".</summary>
		[Required]
		[MaxLength(50)]
		public string Slug { get; set; } = null!;

		/// <summary>Телефон по умолчанию для города.</summary>
		[MaxLength(50)]
		public string? DefaultPhone { get; set; }

		/// <summary>Базовая цена по городу (если нет переопределения на PageCity).</summary>
		public decimal? DefaultPrice { get; set; }

		/// <summary>
		/// Город активирован: показывается в окне выбора города, в футере и доступен по своему URL.
		/// Неактивный город скрыт с сайта, его URL перенаправляет на страницу «Вся Беларусь».
		/// </summary>
		public bool IsActive { get; set; } = true;

		/// <summary>Порядок сортировки в списках (меньше — выше). При равенстве — по названию.</summary>
		public int SortOrder { get; set; } = 0;

		/// <summary>Страницы города (посадочные).</summary>
		public ICollection<PageCity> Pages { get; set; } = new List<PageCity>();

		/// <summary>Клиенты из этого города.</summary>
		public ICollection<Client> Clients { get; set; } = new List<Client>();
	}
}
