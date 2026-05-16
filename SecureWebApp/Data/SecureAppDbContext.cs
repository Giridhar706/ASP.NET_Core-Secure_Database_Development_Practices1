using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using SecureWebApp.Models;

namespace SecureWebApp.Data
{
    public class SecureAppDbContext : DbContext
    {
        private readonly IDataProtectionProvider _dataProtectionProvider;

        public SecureAppDbContext(DbContextOptions<SecureAppDbContext> options, IDataProtectionProvider dataProtectionProvider) 
            : base(options)
        {
            _dataProtectionProvider = dataProtectionProvider;
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<FinancialDetail> FinancialDetails { get; set; } = null!;
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Create a protector specific to financial data
            var protector = _dataProtectionProvider.CreateProtector("FinancialDataProtector");
            var converter = new EncryptionConverter(protector);

            // Apply column-level encryption to sensitive fields using Value Converters
            modelBuilder.Entity<FinancialDetail>()
                .Property(f => f.CardNumber)
                .HasConversion(converter);

            // Configure relationships if needed
            modelBuilder.Entity<FinancialDetail>()
                .HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
