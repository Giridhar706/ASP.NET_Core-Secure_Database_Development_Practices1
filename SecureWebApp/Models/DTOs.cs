using System.ComponentModel.DataAnnotations;

namespace SecureWebApp.Models.DTOs
{
    public class RegisterDto
    {
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long.")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Role { get; set; } = "User";
    }

    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class AddFinancialDto
    {
        [Required]
        [StringLength(16, MinimumLength = 16)]
        [RegularExpression(@"^\d{16}$", ErrorMessage = "Card number must be exactly 16 digits.")]
        public string CardNumber { get; set; } = string.Empty;

        [Required]
        [Range(0, 99999999.99, ErrorMessage = "Balance must be a positive value.")]
        public decimal Balance { get; set; }
    }
}
