using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using EcobayApp.Contex;
using EcobayApp.Models;
using Microsoft.EntityFrameworkCore;
namespace EcobayApp.Services;

public sealed record PublicRequestInput(Guid SubmissionId, string? Name, string? Phone, string? Message, long? PageCityId, Guid? AnalyticsVisitId = null);
public sealed record PublicRequestResult(long RequestId, bool Reused);
public sealed record RequestMailPayload(string Name, string Phone, string? Message, string Source, string? CityName);

public sealed class PublicRequestService(IDbContextFactory<AppDBContext> factory)
{
    public async Task<PublicRequestResult> SubmitAsync(PublicRequestInput input)
    {
        var phone = PhoneUtil.Normalize(input.Phone);
        if (!PhoneUtil.IsValid(phone)) throw new ArgumentException("Укажите корректный телефон (9–15 цифр).");
        var name = (input.Name ?? "").Trim();
        var message = string.IsNullOrWhiteSpace(input.Message) ? null : input.Message.Trim();
        if (name.Length > 100 || message?.Length > 500) throw new ArgumentException("Имя — до 100 символов, описание — до 500.");
        if (input.SubmissionId == Guid.Empty) throw new ArgumentException("Обновите форму и повторите отправку.");

        var key = JsonSerializer.Serialize(new { Phone = phone, Name = name, Message = message, PageCityId = input.PageCityId });
        var fingerprint = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(key)));

        await using var db = await factory.CreateDbContextAsync();
        await using var tx = await db.Database.BeginTransactionAsync();
        // Database-owned locks also work across separate IIS processes/servers.
        await LockAsync(db, "ecobay:submission:" + input.SubmissionId.ToString("N"));
        var receipt = await db.RequestSubmissions.AsNoTracking().SingleOrDefaultAsync(x => x.Id == input.SubmissionId);
        if (receipt is not null)
        {
            if (receipt.Fingerprint != fingerprint)
                throw new ArgumentException("Эта форма уже была отправлена с другими данными. Обновите страницу для нового обращения.");
            return new(receipt.RequestId, true);
        }
        await LockAsync(db, "ecobay:phone:" + phone);

        var page = input.PageCityId.HasValue
            ? await db.PageCities.AsNoTracking().Include(x => x.City).SingleOrDefaultAsync(x => x.Id == input.PageCityId)
            : null;
        if (input.PageCityId.HasValue && (page is null || !page.City.IsActive))
            throw new ArgumentException("Выбранный город недоступен. Обновите страницу.");
        var source = page is null ? "/" : "/" + page.City.Slug;
        var now = DateTime.UtcNow;
        var cutoff = now.AddMinutes(-2);
        // Independent tabs/reloads can have different submission IDs but identical content.
        var recent = await db.RequestSubmissions.AsNoTracking()
            .Where(x => x.Fingerprint == fingerprint && x.CreatedAtUtc >= cutoff)
            .OrderByDescending(x => x.CreatedAtUtc).Select(x => new { x.RequestId, x.CreatedAtUtc }).FirstOrDefaultAsync();
        if (recent is not null && await db.Requests.AnyAsync(r => r.Id == recent.RequestId))
        {
            db.RequestSubmissions.Add(new() { Id = input.SubmissionId, RequestId = recent.RequestId, Fingerprint = fingerprint, CreatedAtUtc = recent.CreatedAtUtc });
            await db.SaveChangesAsync(); await tx.CommitAsync();
            return new(recent.RequestId, true);
        }

        var client = await ClientPhoneLookup.ByPhone(db, phone).OrderBy(x => x.Id).FirstOrDefaultAsync();
        if (client is null)
        {
            client = new Client { Name = name, Phone = phone, Comment = message, CityId = page?.CityId };
            db.Clients.Add(client);
        }
        // Anonymous submissions must not overwrite an existing client's identity or notes.
        var request = new Request { Client = client, PageCityId = page?.Id, Message = message, Source = source,
            DateCreate = DateTime.Now, AnalyticsVisitId = input.AnalyticsVisitId };
        db.Requests.Add(request);
        await db.SaveChangesAsync();
        db.RequestSubmissions.Add(new() { Id = input.SubmissionId, RequestId = request.Id, Fingerprint = fingerprint, CreatedAtUtc = now });
        db.RequestEmailOutbox.Add(new() {
            RequestId = request.Id, AvailableAtUtc = now,
            PayloadJson = JsonSerializer.Serialize(new RequestMailPayload(name, phone, message, source, page?.City.Name))
        });
        var year = DateTime.Now.Year;
        var goalId = await db.Goals.Where(g => g.IsActive && g.Year == year)
            .OrderByDescending(g => g.StartDate).ThenByDescending(g => g.Id).Select(g => (long?)g.Id).FirstOrDefaultAsync();
        if (goalId.HasValue)
            await db.Goals.Where(g => g.Id == goalId.Value && g.IsActive).ExecuteUpdateAsync(u => u
                .SetProperty(g => g.ActualClients, g => g.ActualClients + 1)
                .SetProperty(g => g.ActualItems, g => g.ActualItems + 1));
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return new(request.Id, false);
    }

    private static Task LockAsync(AppDBContext db, string resource) => db.Database.ExecuteSqlInterpolatedAsync($@"
        DECLARE @result int;
        EXEC @result = sys.sp_getapplock @Resource={resource}, @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=10000;
        IF @result < 0 THROW 51000, 'Request is busy. Retry.', 1;");
}
