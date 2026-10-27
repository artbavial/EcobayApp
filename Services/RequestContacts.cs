using EcobayApp.Contex;
using EcobayApp.Models;
using Microsoft.EntityFrameworkCore;

namespace EcobayApp.Services;

public sealed record CallMarkResult(string? Error, DateTime? CalledAtUtc = null, string? UserName = null);

public sealed class RequestContacts(IDbContextFactory<AppDBContext> factory, SitePermissions permissions)
{
    public async Task<CallMarkResult> ToggleAsync(long id, DateTime? expectedCalledAtUtc)
    {
        var actor = await permissions.CurrentAsync();
        if (actor is null) return new("Недействительная сессия. Войдите заново.");
        await using var db = await factory.CreateDbContextAsync();
        await using var tx = await db.Database.BeginTransactionAsync();
        await RequestLocks.AcquireAsync(db, id);
        var request = await db.Requests.SingleOrDefaultAsync(r => r.Id == id);
        if (request is null) return new("Заявка не найдена.");
        // Explicit expected state: a stale retry must not undo another employee's mark.
        if (request.CustomerCalledAtUtc != expectedCalledAtUtc)
            return new("Отметку уже изменили. Данные обновлены; проверьте их перед повторным нажатием.");
        var now = DateTime.UtcNow;
        var isCalled = !request.CustomerCalledAtUtc.HasValue;
        db.RequestContactEvents.Add(new RequestContactEvent {
            RequestId = id, IsCalled = isCalled, CreatedAtUtc = now,
            CallAtUtc = request.CustomerCalledAtUtc ?? now, UserId = actor.Id,
            UserName = actor.DisplayName ?? actor.Login
        });
        request.CustomerCalledAtUtc = isCalled ? now : null;
        request.CustomerCalledByUserId = isCalled ? actor.Id : null;
        request.CustomerCalledByName = isCalled ? actor.DisplayName ?? actor.Login : null;
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return new(null, request.CustomerCalledAtUtc, request.CustomerCalledByName);
    }
}
