using EcobayApp.Contex;
using EcobayApp.Models;
using Microsoft.EntityFrameworkCore;

namespace EcobayApp.Services;

/// <summary>Смена статусов заявки водителем/администратором с записью в историю.</summary>
public sealed class RequestWorkflow
{
    public const decimal MaxPaid = 1_000_000m;

    private readonly IDbContextFactory<AppDBContext> _dbFactory;
    private readonly SitePermissions _permissions;

    public RequestWorkflow(IDbContextFactory<AppDBContext> dbFactory, SitePermissions permissions)
    { _dbFactory = dbFactory; _permissions = permissions; }

    public static string StatusTitle(RequestStatus s) => s switch
    {
        RequestStatus.Done => "Отработана",
        RequestStatus.Cancelled => "Отменена",
        _ => "Новая"
    };

    /// <summary>Отработать: сумма выплаты обязательна (0 — если ничего не платили).</summary>
    public Task<string?> CompleteAsync(long id, decimal? paid, string? comment, long userId, string userName) =>
        ChangeAsync(id, RequestStatus.Done, paid, comment, userId, userName);

    /// <summary>Отменить: комментарий обязателен (причина).</summary>
    public Task<string?> CancelAsync(long id, string? comment, long userId, string userName) =>
        ChangeAsync(id, RequestStatus.Cancelled, null, comment, userId, userName);

    /// <summary>Вернуть в работу (ошибочная отмена/отработка).</summary>
    public Task<string?> ReopenAsync(long id, string? comment, long userId, string userName) =>
        ChangeAsync(id, RequestStatus.New, null, comment, userId, userName);

    private async Task<string?> ChangeAsync(long id, RequestStatus to, decimal? paid, string? comment, long userId, string userName)
    {
        var actor = await _permissions.CurrentAsync();
        if (actor is null) return "Недействительная сессия. Войдите заново.";
        userId = actor.Id; userName = actor.DisplayName ?? actor.Login;
        comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
        if (comment?.Length > 1000) return "Комментарий слишком длинный (максимум 1000 символов).";

        if (to == RequestStatus.Done)
        {
            if (paid is null) return "Укажите, сколько заплатили клиенту (0, если не платили).";
            if (paid < 0 || paid > MaxPaid) return "Сумма должна быть от 0 до 1 000 000 руб.";
            paid = Math.Round(paid.Value, 2);
        }
        if (to == RequestStatus.Cancelled && comment is null) return "Укажите причину отмены.";
        if (to == RequestStatus.New && comment is null) return "Укажите, почему заявка возвращается в работу.";

        await using var db = await _dbFactory.CreateDbContextAsync();
        var r = await db.Requests.FirstOrDefaultAsync(x => x.Id == id);
        if (r is null) return "Заявка не найдена (возможно, удалена).";
        if (actor.Role == AdminRoles.Driver && to == RequestStatus.New && r.Status == RequestStatus.Done)
            return "Возврат отработанной заявки доступен администратору или руководителю.";
        if (r.Status == to) return $"Заявка уже в статусе «{StatusTitle(to)}». Обновите список.";
        if (to == RequestStatus.Done && r.Status == RequestStatus.Cancelled)
            return "Отменённую заявку сначала верните в работу.";

        var now = DateTime.Now;
        db.RequestEvents.Add(new RequestEvent
        {
            RequestId = r.Id, FromStatus = r.Status, ToStatus = to, Comment = comment,
            PaidAmount = to == RequestStatus.Done ? paid : null,
            UserId = userId, UserName = userName, CreatedAt = now
        });

        r.Status = to;
        r.DriverComment = comment;
        if (to == RequestStatus.New)
        {
            r.PaidAmount = null;
            r.ProcessedAt = null;
            r.ProcessedByUserId = null;
            r.ProcessedByName = null;
        }
        else
        {
            if (to == RequestStatus.Done) r.PaidAmount = paid;
            r.ProcessedAt = now;
            r.ProcessedByUserId = userId;
            r.ProcessedByName = userName;
        }

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            return "Заявку одновременно изменил другой пользователь. Обновите список.";
        }
        return null;
    }

    public static string Duration(DateTime from, DateTime to)
    {
        var d = to - from;
        if (d.TotalMinutes < 1) return "меньше минуты";
        if (d.TotalHours < 1) return $"{(int)d.TotalMinutes} мин";
        if (d.TotalDays < 1) return $"{(int)d.TotalHours} ч {d.Minutes} мин";
        return $"{(int)d.TotalDays} д {d.Hours} ч";
    }
}
