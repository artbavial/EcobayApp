using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;

namespace EcobayApp.Services
{
	/// <summary>
	/// Для интерактивных (SignalR) страниц: раз в несколько минут перепроверяет,
	/// что сессия администратора ещё действительна. Если пароль сменили или
	/// пользователя отключили — открытая панель управления потеряет доступ.
	/// </summary>
	public class AdminRevalidatingAuthStateProvider : RevalidatingServerAuthenticationStateProvider
	{
		private readonly IServiceScopeFactory _scopeFactory;

		public AdminRevalidatingAuthStateProvider(ILoggerFactory loggerFactory, IServiceScopeFactory scopeFactory)
			: base(loggerFactory)
		{
			_scopeFactory = scopeFactory;
		}

		protected override TimeSpan RevalidationInterval => TimeSpan.FromMinutes(5);

		protected override async Task<bool> ValidateAuthenticationStateAsync(
			AuthenticationState authenticationState, CancellationToken cancellationToken)
		{
			var user = authenticationState.User;
			if (user.Identity?.IsAuthenticated != true)
				return true; // анонимного пользователя перепроверять не нужно

			await using var scope = _scopeFactory.CreateAsyncScope();
			var auth = scope.ServiceProvider.GetRequiredService<AdminAuthService>();
			return await auth.IsPrincipalValidAsync(user);
		}
	}
}
