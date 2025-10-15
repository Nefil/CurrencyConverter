using CurrencyConverter.Model;
using Microsoft.EntityFrameworkCore;

namespace CurrencyConverter.Data
{
    public class ConverterDbContext : DbContext
    {
        public DbSet<Rate> Rates { get; set; }

        private readonly string _connectionString;

        public ConverterDbContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(_connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Rate> (entity =>
            {
                entity.ToTable("Rates");
                entity.HasKey(r => r.Id);
                entity.Property(r => r.CurrencyCode).IsRequired();
                entity.Property(r => r.ExchangeRate).IsRequired();
                entity.Property(r => r.Timestamp).IsRequired();
            });
        }
           
    }
}
