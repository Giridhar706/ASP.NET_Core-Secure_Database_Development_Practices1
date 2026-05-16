using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SecureWebApp.Models;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SecureWebApp.Data
{
    public class AuditInterceptor : SaveChangesInterceptor
    {
        // In a real app, you might inject IHttpContextAccessor to get the current user ID.
        // For simplicity, we are passing it or we can resolve it if available.
        // But since this is a singleton or scoped interceptor, we should be careful with dependencies.
        
        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            BeforeSaveChanges(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            BeforeSaveChanges(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void BeforeSaveChanges(DbContext? context)
        {
            if (context == null) return;

            context.ChangeTracker.DetectChanges();

            var auditEntries = context.ChangeTracker.Entries()
                .Where(e => e.Entity is not AuditLog && 
                           (e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted))
                .ToList();

            foreach (var entry in auditEntries)
            {
                var auditLog = new AuditLog
                {
                    TableName = entry.Metadata.GetTableName() ?? entry.Entity.GetType().Name,
                    Action = entry.State.ToString(),
                    Timestamp = DateTime.UtcNow,
                    // Note: In production, inject IHttpContextAccessor to retrieve the logged-in User's ID
                    UserId = "System", // Placeholder
                    PrimaryKey = GetPrimaryKey(entry),
                    Changes = entry.State == EntityState.Modified ? "Data modified" : null // Simple tracking
                };

                context.Add(auditLog);
            }
        }

        private string GetPrimaryKey(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
        {
            var keyName = entry.Metadata.FindPrimaryKey()?.Properties.Select(x => x.Name).Single();
            if (keyName != null)
            {
                return entry.Property(keyName).CurrentValue?.ToString() ?? "Unknown";
            }
            return "Unknown";
        }
    }
}
