using EcobayApp.Models;
using Microsoft.EntityFrameworkCore;

namespace EcobayApp.Contex
{
	public class AppDBContext : DbContext
	{
		public AppDBContext(DbContextOptions<AppDBContext> options)
			: base(options)
		{

		}

		public DbSet<City> Cities { get; set; } = null!;
		public DbSet<PageCity> PageCities { get; set; } = null!;
		public DbSet<Client> Clients { get; set; } = null!;
		public DbSet<Request> Requests { get; set; } = null!;
		public DbSet<PromoMessage> PromoMessages { get; set; } = null!;
		public DbSet<Goal> Goals { get; set; } = null!;
		public DbSet<MainPageSettings> MainPageSettings { get; set; } = null!;
		public DbSet<AdminUser> AdminUsers { get; set; } = null!;

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			// Важно: для City.IsActive не задаём HasDefaultValue — иначе EF не сможет
			// вставить город со значением false. Значение по умолчанию (true) для уже
			// существующих строк проставляется в миграции AddCityActiveMainPageAdminAuth.

			modelBuilder.Entity<AdminUser>()
				.HasIndex(u => u.Login)
				.IsUnique();

			// Filter recent requests and page them in a deterministic order.
			modelBuilder.Entity<Request>().HasIndex(r => new { r.DateCreate, r.Id });
		}
	}
}
