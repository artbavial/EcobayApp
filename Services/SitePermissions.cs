using System.Security.Claims;
using EcobayApp.Contex;
using EcobayApp.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
namespace EcobayApp.Services;

/// <summary>Fresh server-side permission checks. No authority is derived from component parameters.</summary>
public sealed class SitePermissions(AuthenticationStateProvider state, IDbContextFactory<AppDBContext> factory)
{
    public async Task<AdminUser?> CurrentAsync()
    {
        var principal = (await state.GetAuthenticationStateAsync()).User;
        if (principal.Identity?.IsAuthenticated != true ||
            !long.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var id)) return null;
        var stamp = principal.FindFirstValue(AdminAuthService.SecurityStampClaim);
        var role = principal.FindFirstValue(ClaimTypes.Role);
        if (string.IsNullOrEmpty(stamp) || !AdminRoles.IsKnown(role)) return null;
        await using var db = await factory.CreateDbContextAsync();
        return await db.AdminUsers.AsNoTracking().SingleOrDefaultAsync(u =>
            u.Id == id && u.IsActive && u.SecurityStamp == stamp && u.Role == role);
    }
    public async Task<AdminUser> RequireStaffAsync()
    {
        var user = await CurrentAsync();
        if (user?.Role is not (AdminRoles.Admin or AdminRoles.Manager)) throw new UnauthorizedAccessException("Недостаточно прав или сессия завершена.");
        return user;
    }
    public async Task<AdminUser> RequireAdminAsync()
    {
        var user = await CurrentAsync();
        if (user?.Role != AdminRoles.Admin) throw new UnauthorizedAccessException("Действие доступно только администратору.");
        return user;
    }
}
