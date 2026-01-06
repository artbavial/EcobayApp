using System.ComponentModel.DataAnnotations;

namespace EcobayApp.Models
{
	public class RequestFormModel
	{
		public string? Name { get; set; }

		[Required(ErrorMessage = "Укажите телефон")]
		[MinLength(7, ErrorMessage = "Телефон слишком короткий")]
		[MaxLength(15, ErrorMessage = "Телефон слишком длинный")]
		public string? Phone { get; set; }

		public string? Comment { get; set; }
	}

}
