using System.ComponentModel.DataAnnotations;

namespace EcobayApp.Models
{
	public class Goal
	{
		[Key]
		public long Id { get; set; }

		/// <summary>Год, для которого ставится цель.</summary>
		public int Year { get; set; }

		/// <summary>Цель по количеству клиентов за год.</summary>
		public int TargetClients { get; set; }

		/// <summary>Цель по количеству единиц техники.</summary>
		public int TargetItems { get; set; }

		/// <summary>Фактическое количество клиентов (будем считать по заявкам/клиентам).</summary>
		public int ActualClients { get; set; }

		/// <summary>Фактическое количество единиц техники (можно считать отдельно, пока 1:1 к заявке).</summary>
		public int ActualItems { get; set; }

		/// <summary>Количество неактуальных заявок (отменённые, спам и т.п.).</summary>
		public int InactiveRequests { get; set; }

		/// <summary>Дата последнего сброса / старта (например, 5 января).</summary>
		public DateTime StartDate { get; set; }

		/// <summary>Используем ли эту цель сейчас.</summary>
		public bool IsActive { get; set; } = true;

		// ---------- Автодобавка к прогрессу (фоновая задача на сервере) ----------

		/// <summary>Каждый час автоматически прибавлять AutoIncrementPerHour к InactiveRequests.</summary>
		public bool AutoIncrementEnabled { get; set; }

		/// <summary>Сколько добавлять за каждый час.</summary>
		public int AutoIncrementPerHour { get; set; } = 1;

		/// <summary>С какого часа (включительно) начислять, 0–23.</summary>
		public int AutoIncrementFromHour { get; set; } = 8;

		/// <summary>По какой час (включительно) начислять, 0–23.</summary>
		public int AutoIncrementToHour { get; set; } = 21;

		/// <summary>Последняя учтённая отметка часа. Нужна, чтобы после перезапуска сервера досчитать пропущенные часы.</summary>
		public DateTime? AutoIncrementLastAt { get; set; }
	}
}
