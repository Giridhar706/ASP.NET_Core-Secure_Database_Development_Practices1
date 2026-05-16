using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SecureWebApp.Models
{
    public class FinancialDetail
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }

        [Required]
        [StringLength(16, MinimumLength = 16)]
        [RegularExpression(@"^\d{16}$", ErrorMessage = "Card number must be exactly 16 digits.")]
        public string CardNumber { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Balance { get; set; }

        [Required]
        public string HMAC { get; set; } = string.Empty;
    }
}
