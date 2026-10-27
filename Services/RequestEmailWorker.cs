using System.Text.Json;
using EcobayApp.Contex;
using EcobayApp.Models;
using Microsoft.EntityFrameworkCore;
namespace EcobayApp.Services;

public sealed class RequestEmailWorker(IServiceScopeFactory scopes, ILogger<RequestEmailWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (await SendOneAsync(stoppingToken)) continue;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception ex) { logger.LogError(ex, "Request email queue failed"); }
            try { await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken); }
            catch (OperationCanceledException) { break; }
        }
    }
    private async Task<bool> SendOneAsync(CancellationToken ct)
    {
        await using var scope = scopes.CreateAsyncScope();
        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDBContext>>();
        var sender = scope.ServiceProvider.GetRequiredService<IEmailSender>();
        await using var db = await factory.CreateDbContextAsync(ct);
        RequestEmailOutbox? item;
        var now = DateTime.UtcNow;
        await using (var tx = await db.Database.BeginTransactionAsync(ct))
        {
            // Take a durable lease; another worker skips this row after the transaction commits.
            var candidates = await db.RequestEmailOutbox.FromSqlInterpolated($@"
                SELECT TOP (1) * FROM [RequestEmailOutbox] WITH (UPDLOCK, READPAST, READCOMMITTEDLOCK)
                WHERE [SentAtUtc] IS NULL AND [AvailableAtUtc] <= {now}
                  AND ([LeaseUntilUtc] IS NULL OR [LeaseUntilUtc] < {now})
                ORDER BY [Id]").ToListAsync(ct);
            item = candidates.FirstOrDefault();
            if (item is null) return false;
            item.LeaseUntilUtc = now.AddMinutes(10); item.Attempts++;
            await db.SaveChangesAsync(ct); await tx.CommitAsync(ct);
        }
        var ownedLease = item.LeaseUntilUtc;
        try
        {
            var payload = JsonSerializer.Deserialize<RequestMailPayload>(item.PayloadJson)
                ?? throw new InvalidOperationException("Empty mail payload");
            // A deleted application should not generate a stale notification.
            if (await db.Requests.AnyAsync(x => x.Id == item.RequestId, ct))
            {
                var client = new Client { Name = payload.Name, Phone = payload.Phone };
                var request = new Request { Id = item.RequestId, Message = payload.Message, Source = payload.Source };
                var receipt = await db.RequestSubmissions.AsNoTracking().Where(x => x.RequestId == item.RequestId)
                    .OrderBy(x => x.CreatedAtUtc).Select(x => (DateTime?)x.CreatedAtUtc).FirstOrDefaultAsync(ct);
                request.DateCreate = receipt?.ToLocalTime() ?? DateTime.Now;
                City? city = payload.CityName is null ? null : new City { Name = payload.CityName };
                await sender.SendRequestCreatedAsync(client, request, city, null);
            }
            await db.RequestEmailOutbox.Where(x => x.Id == item.Id && x.LeaseUntilUtc == ownedLease)
                .ExecuteUpdateAsync(u => u.SetProperty(x => x.SentAtUtc, DateTime.UtcNow)
                    .SetProperty(x => x.LeaseUntilUtc, (DateTime?)null).SetProperty(x => x.LastError, (string?)null), ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Request {RequestId} email attempt {Attempt} failed", item.RequestId, item.Attempts);
            var retryAt = DateTime.UtcNow.AddMinutes(Math.Min(60, Math.Pow(2, Math.Min(item.Attempts, 6))));
            await db.RequestEmailOutbox.Where(x => x.Id == item.Id && x.LeaseUntilUtc == ownedLease)
                .ExecuteUpdateAsync(u => u.SetProperty(x => x.AvailableAtUtc, retryAt)
                    .SetProperty(x => x.LeaseUntilUtc, (DateTime?)null)
                    .SetProperty(x => x.LastError, "SMTP send failed; see server log."), ct);
        }
        return true;
    }
}
