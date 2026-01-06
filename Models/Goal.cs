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
	}
}
