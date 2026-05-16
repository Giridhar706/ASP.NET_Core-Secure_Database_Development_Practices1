using System.Security.Cryptography;
using System.Text;
using BCrypt.Net;

namespace SecureWebApp.Security
{
    public interface ICryptoService
    {
        string HashPassword(string password);
        bool VerifyPassword(string password, string hash);
        string GenerateHMAC(string data);
    }

    public class CryptoService : ICryptoService
    {
        private readonly string _hmacKey;

        // In a real application, the HMAC key MUST be stored securely (e.g., Azure Key Vault, Environment Variables).
        // It must never be hardcoded. Here we will load it from configuration.
        public CryptoService(Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            _hmacKey = configuration["Security:HMACKey"] ?? throw new System.ArgumentNullException("HMACKey is missing in configuration.");
        }

        public string HashPassword(string password)
        {
            // Generates a hash using BCrypt with a default work factor (11 is common)
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }

        public string GenerateHMAC(string data)
        {
            var keyBytes = Encoding.UTF8.GetBytes(_hmacKey);
            using var hmac = new HMACSHA256(keyBytes);
            var dataBytes = Encoding.UTF8.GetBytes(data);
            var hashBytes = hmac.ComputeHash(dataBytes);
            return System.Convert.ToBase64String(hashBytes);
        }
    }
}
