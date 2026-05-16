using System;
using System.ComponentModel.DataAnnotations;

namespace SecureWebApp.Models
{
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Action { get; set; } = string.Empty; // e.g., Insert, Update, Delete

        [Required]
        [StringLength(100)]
        public string TableName { get; set; } = string.Empty;

        [Required]
        public DateTime Timestamp { get; set; }

        public string? PrimaryKey { get; set; }

        public string? UserId { get; set; } // The ID or Name of the user making the change
        
        public string? Changes { get; set; } // Optional: Store old vs new values securely
    }
}
