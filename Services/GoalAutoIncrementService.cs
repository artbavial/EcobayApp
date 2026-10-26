using EcobayApp.Contex;
using Microsoft.EntityFrameworkCore;

namespace EcobayApp.Services;

/// <summary>
/// Фоновая задача: каждый час прибавляет к ручной добавке цели (Goal.InactiveRequests)
/// заданное количество. Работает внутри приложения на сервере.
///
/// Если сервер/пул IIS «спал», при следующем запуске пропущенные часы досчитываются
/// (по полю AutoIncrementLastAt), но не более чем за 30 дней.
/// </summary>
public sealed class GoalAutoIncrementService : BackgroundService
{
    private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(5);
    private const int MaxCatchUpDays = 30;

    private readonly IDbContextFactory<AppDBContext> _dbFactory;
    private readonly ILogger<GoalAutoIncrementService> _logger;

    public GoalAutoIncrementService(IDbContextFactory<AppDBContext> dbFactory, ILogger<GoalAutoIncrementService> logger)
    {
        _dbFactory = dbFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Небольшая пауза, чтобы не мешать старту приложения.
        try { await Task.Delay(TimeSpan.FromSeconds(20), stoppingToken); } catch (OperationCanceledException) { return; }

        using var timer = new PeriodicTimer(CheckInterval);
        do
        {
            try { await TickAsync(DateTime.Now, stoppingToken); }
            catch (OperationCanceledException) { break; }
            catch (Exception ex) { _logger.LogError(ex, "Goal auto-increment failed"); }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    public async Task TickAsync(DateTime now, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var goals = await db.Goals.AsNoTracking()
            .Where(g => g.IsActive && g.AutoIncrementEnabled && g.AutoIncrementPerHour > 0)
            .ToListAsync(ct);

        foreach (var g in goals)
        {
            var last = g.AutoIncrementLastAt;
            var floorNow = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0);

            if (last is null)
            {
                // Первый запуск после включения: отсчёт с текущего часа, без начислений «задним числом».
                await db.Goals.Where(x => x.Id == g.Id && x.AutoIncrementLastAt == null)
                    .ExecuteUpdateAsync(s => s.SetProperty(x => x.AutoIncrementLastAt, floorNow), ct);
                continue;
            }

            var from = last.Value < now.AddDays(-MaxCatchUpDays) ? now.AddDays(-MaxCatchUpDays) : last.Value;
            var hours = CountHourMarks(from, now, g.AutoIncrementFromHour, g.AutoIncrementToHour);
            if (hours <= 0)
                continue;

            var add = (long)hours * g.AutoIncrementPerHour;
            var addInt = (int)Math.Min(add, int.MaxValue - (long)g.InactiveRequests);

            // Условие по AutoIncrementLastAt защищает от двойного начисления (например, два экземпляра приложения).
            var updated = await db.Goals
                .Where(x => x.Id == g.Id && x.AutoIncrementLastAt == last)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.InactiveRequests, x => x.InactiveRequests + addInt)
                    .SetProperty(x => x.AutoIncrementLastAt, floorNow), ct);

            if (updated > 0)
                _logger.LogInformation("Goal {GoalId}: +{Add} ({Hours} h x {Per})", g.Id, addInt, hours, g.AutoIncrementPerHour);
        }
    }

    /// <summary>
    /// Сколько отметок «ровно HH:00» прошло в интервале (after, until] с часом в диапазоне [fromHour; toHour].
    /// Если fromHour > toHour — диапазон через полночь (например, 22–6).
    /// </summary>
    public static int CountHourMarks(DateTime after, DateTime until, int fromHour, int toHour)
    {
        if (until <= after) return 0;
        fromHour = Math.Clamp(fromHour, 0, 23);
        toHour = Math.Clamp(toHour, 0, 23);

        var mark = new DateTime(after.Year, after.Month, after.Day, after.Hour, 0, 0).AddHours(1);
        var count = 0;
        while (mark <= until)
        {
            var h = mark.Hour;
            var inWindow = fromHour <= toHour ? h >= fromHour && h <= toHour : h >= fromHour || h <= toHour;
            if (inWindow) count++;
            mark = mark.AddHours(1);
        }
        return count;
    }
}
