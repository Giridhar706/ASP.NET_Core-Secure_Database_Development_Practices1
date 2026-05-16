using System.ComponentModel.DataAnnotations;

namespace SecureWebApp.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Role { get; set; } = "User"; // E.g., Admin, User, Auditor

        [Required]
        public string HMAC { get; set; } = string.Empty;
    }
}
