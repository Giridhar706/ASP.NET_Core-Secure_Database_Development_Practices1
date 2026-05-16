using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace SecureWebApp.Services
{
    public interface ISecureLogger<T>
    {
        void LogInformation(string message, params object[] args);
        void LogWarning(string message, params object[] args);
        void LogError(System.Exception exception, string message, params object[] args);
    }

    public class SecureLogger<T> : ISecureLogger<T>
    {
        private readonly ILogger<T> _logger;

        public SecureLogger(ILogger<T> logger)
        {
            _logger = logger;
        }

        private string SanitizeMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return message;

            // Simple sanitization: mask 16-digit credit card numbers
            var sanitized = Regex.Replace(message, @"\b\d{16}\b", "****-****-****-****");

            // Mask potential passwords (if logged accidentally in format like password=xyz)
            sanitized = Regex.Replace(sanitized, @"(?i)(password|pwd)\s*=\s*\S+", "$1=********");

            return sanitized;
        }

        public void LogInformation(string message, params object[] args)
        {
            _logger.LogInformation(SanitizeMessage(message), args);
        }

        public void LogWarning(string message, params object[] args)
        {
            _logger.LogWarning(SanitizeMessage(message), args);
        }

        public void LogError(System.Exception exception, string message, params object[] args)
        {
            _logger.LogError(exception, SanitizeMessage(message), args);
        }
    }
}
