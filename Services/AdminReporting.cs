using EcobayApp.Models;
using Microsoft.EntityFrameworkCore;

namespace EcobayApp.Services;

/// <summary>Shared SQL projections and interval rules. Dates use the existing DateCreate server-local convention.</summary>
public static class AdminReporting
{
    public static string? ValidateRange(DateTime from, DateTime to)
    {
        if (to.Date < from.Date) return "Дата окончания не может быть раньше даты начала.";
        if ((to.Date - from.Date).Days >= 30) return "Выберите диапазон не более 30 дней включительно.";
        return null;
    }

    public static IQueryable<AdminRequestRow> Rows(IQueryable<Request> requests) =>
        requests.Select(r => new AdminRequestRow
        {
            Id = r.Id,
            Date = r.DateCreate,
            Name = r.Client.Name,
            Phone = r.Client.Phone,
            Message = r.Message,
            CityId = r.PageCity != null ? (long?)r.PageCity.CityId : r.Client.CityId,
            City = r.PageCity != null ? r.PageCity.City.Name
                : r.Client.City != null ? r.Client.City.Name : "Город не указан"
        });

    // Keep the legacy public formula explicit; InactiveRequests is NOT a stored percentage.
    public static long Completed(Goal goal) => (long)goal.ActualClients + goal.InactiveRequests;
    public static decimal Percent(long completed, int target) =>
        target > 0 ? Math.Round(100m * completed / target, 1) : 0m;

    public static string Change(int current, int previous)
    {
        var delta = current - previous;
        var signed = delta > 0 ? $"+{delta}" : delta.ToString();
        if (previous == 0)
            return current == 0 ? "Без изменений; в предыдущем периоде 0" : $"{signed}; в предыдущем периоде 0";
        var percent = Math.Round(100m * delta / previous, 1);
        return $"{signed} ({(percent > 0 ? "+" : "")}{percent:0.#}%) к предыдущему периоду";
    }
}

public sealed class AdminRequestRow
{
    public long Id { get; set; }
    public DateTime Date { get; set; }
    public string Name { get; set; } = "";
    public string Phone { get; set; } = "";
    public string? Message { get; set; }
    public long? CityId { get; set; }
    public string City { get; set; } = "";
}
