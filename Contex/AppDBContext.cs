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
	}
}
