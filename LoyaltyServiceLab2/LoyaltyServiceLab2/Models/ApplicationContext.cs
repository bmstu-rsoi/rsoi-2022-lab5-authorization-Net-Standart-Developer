using Microsoft.EntityFrameworkCore;

namespace LoyaltyServiceLab2.Models
{
    public class ApplicationContext : DbContext
    {
        public DbSet<Loyalty> Loyalties { get; set; }

        public ApplicationContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string host = Environment.GetEnvironmentVariable("DBHOST") ?? "localhost";
            string port = Environment.GetEnvironmentVariable("DBPORT") ?? "54321";
            string db = Environment.GetEnvironmentVariable("DATABASE") ?? "loyalties";
            string user = "program";
            string password = Environment.GetEnvironmentVariable("PASSWORD") ?? "test";

            optionsBuilder.UseNpgsql($"Host={host};Port={port};Database={db};Username={user};Password={password}");
        }
    }
}
