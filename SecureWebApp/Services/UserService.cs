using Microsoft.EntityFrameworkCore;
using SecureWebApp.Data;
using SecureWebApp.Models;
using SecureWebApp.Security;
using System.Threading.Tasks;

namespace SecureWebApp.Services
{
    public interface IUserService
    {
        Task<User?> RegisterUserAsync(string email, string password, string role);
        Task<User?> AuthenticateAsync(string email, string password);
    }

    public class UserService : IUserService
    {
        private readonly SecureAppDbContext _context;
        private readonly ICryptoService _cryptoService;
        private readonly ISecureLogger<UserService> _logger;

        public UserService(SecureAppDbContext context, ICryptoService cryptoService, ISecureLogger<UserService> logger)
        {
            _context = context;
            _cryptoService = cryptoService;
            _logger = logger;
        }

        public async Task<User?> RegisterUserAsync(string email, string password, string role)
        {
            _logger.LogInformation($"Attempting to register user with email {email}");

            if (await _context.Users.AnyAsync(u => u.Email == email))
            {
                _logger.LogWarning($"Registration failed: Email {email} already exists.");
                return null;
            }

            var passwordHash = _cryptoService.HashPassword(password);
            
            var user = new User
            {
                Email = email,
                PasswordHash = passwordHash,
                Role = role
            };

            // Generate HMAC for data integrity check
            var dataToMac = $"{user.Email}:{user.Role}";
            user.HMAC = _cryptoService.GenerateHMAC(dataToMac);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"User {email} successfully registered.");
            return user;
        }

        public async Task<User?> AuthenticateAsync(string email, string password)
        {
            _logger.LogInformation($"Authentication attempt for email {email}");

            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                _logger.LogWarning($"Authentication failed: User {email} not found.");
                return null;
            }

            // Verify Password
            if (!_cryptoService.VerifyPassword(password, user.PasswordHash))
            {
                _logger.LogWarning($"Authentication failed: Invalid password for {email}");
                return null;
            }

            // Verify Data Integrity
            var dataToMac = $"{user.Email}:{user.Role}";
            var expectedHmac = _cryptoService.GenerateHMAC(dataToMac);
            if (user.HMAC != expectedHmac)
            {
                _logger.LogError(null, $"Data Integrity violation detected for user {email}!");
                return null;
            }

            _logger.LogInformation($"User {email} successfully authenticated.");
            return user;
        }
    }
}
