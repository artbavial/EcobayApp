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
		public DbSet<RequestEvent> RequestEvents { get; set; } = null!;
		public DbSet<Feedback> Feedbacks { get; set; } = null!;

        public DbSet<RequestSubmission> RequestSubmissions { get; set; } = null!;
        public DbSet<RequestEmailOutbox> RequestEmailOutbox { get; set; } = null!;
        public DbSet<PhoneClick> PhoneClicks { get; set; } = null!;
        public DbSet<AnalyticsVisit> AnalyticsVisits { get; set; } = null!;
        public DbSet<AnalyticsEvent> AnalyticsEvents { get; set; } = null!;
        public DbSet<AnalyticsState> AnalyticsStates { get; set; } = null!;
        public DbSet<RequestContactEvent> RequestContactEvents { get; set; } = null!;

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
            base.OnModelCreating(modelBuilder);
            AnalyticsSchema.Configure(modelBuilder);
            modelBuilder.Entity<Request>().Property(r => r.WeightKg).HasPrecision(12, 2);
            modelBuilder.Entity<RequestEvent>().Property(e => e.WeightKg).HasPrecision(12, 2);
            modelBuilder.Entity<Request>().HasIndex(r => new { r.Status, r.ProcessedAt });
            modelBuilder.Entity<RequestContactEvent>().HasOne<Request>().WithMany()
                .HasForeignKey(e => e.RequestId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<RequestContactEvent>().HasIndex(e => new { e.RequestId, e.CreatedAtUtc });
            modelBuilder.Entity<RequestSubmission>().HasIndex(x => new { x.Fingerprint, x.CreatedAtUtc });
            modelBuilder.Entity<RequestEmailOutbox>().HasIndex(x => x.RequestId).IsUnique();
            modelBuilder.Entity<RequestEmailOutbox>().HasIndex(x => new { x.SentAtUtc, x.AvailableAtUtc });
            modelBuilder.Entity<PhoneClick>().HasIndex(x => new { x.CreatedAtUtc, x.CityId });


			// Важно: для City.IsActive не задаём HasDefaultValue — иначе EF не сможет
			// вставить город со значением false. Значение по умолчанию (true) для уже
			// существующих строк проставляется в миграции AddCityActiveMainPageAdminAuth.

			modelBuilder.Entity<AdminUser>()
				.HasIndex(u => u.Login)
				.IsUnique();

			// Filter recent requests and page them in a deterministic order.
			modelBuilder.Entity<Request>().HasIndex(r => new { r.DateCreate, r.Id });
			modelBuilder.Entity<Request>().HasIndex(r => new { r.Status, r.DateCreate });
			modelBuilder.Entity<Request>().HasIndex(r => new { r.RouteDriverId, r.RouteDate });
			modelBuilder.Entity<Request>().Property(r => r.PaidAmount).HasPrecision(10, 2);

			modelBuilder.Entity<RequestEvent>().Property(e => e.PaidAmount).HasPrecision(10, 2);
			modelBuilder.Entity<RequestEvent>()
				.HasOne(e => e.Request).WithMany(r => r.Events)
				.HasForeignKey(e => e.RequestId).OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<Feedback>().HasIndex(f => f.CreatedAt);
		}
	}
}
