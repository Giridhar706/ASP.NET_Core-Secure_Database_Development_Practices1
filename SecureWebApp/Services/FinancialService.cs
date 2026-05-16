using Microsoft.EntityFrameworkCore;
using SecureWebApp.Data;
using SecureWebApp.Models;
using SecureWebApp.Security;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SecureWebApp.Services
{
    public interface IFinancialService
    {
        Task<FinancialDetail?> AddFinancialDetailAsync(int userId, string cardNumber, decimal balance);
        Task<List<FinancialDetail>> GetUserFinancialDetailsAsync(int userId);
    }

    public class FinancialService : IFinancialService
    {
        private readonly SecureAppDbContext _context;
        private readonly ICryptoService _cryptoService;
        private readonly ISecureLogger<FinancialService> _logger;

        public FinancialService(SecureAppDbContext context, ICryptoService cryptoService, ISecureLogger<FinancialService> logger)
        {
            _context = context;
            _cryptoService = cryptoService;
            _logger = logger;
        }

        public async Task<FinancialDetail?> AddFinancialDetailAsync(int userId, string cardNumber, decimal balance)
        {
            _logger.LogInformation($"Adding financial detail for userId {userId} with card {cardNumber}");

            var detail = new FinancialDetail
            {
                UserId = userId,
                CardNumber = cardNumber,
                Balance = balance
            };

            // Data integrity check via HMAC
            var dataToMac = $"{detail.UserId}:{detail.Balance}";
            detail.HMAC = _cryptoService.GenerateHMAC(dataToMac);

            _context.FinancialDetails.Add(detail);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Financial detail successfully added for userId {userId}");
            return detail;
        }

        public async Task<List<FinancialDetail>> GetUserFinancialDetailsAsync(int userId)
        {
            _logger.LogInformation($"Retrieving financial details for userId {userId}");

            var details = await _context.FinancialDetails
                .Where(f => f.UserId == userId)
                .ToListAsync();

            var validDetails = new List<FinancialDetail>();

            foreach (var detail in details)
            {
                var dataToMac = $"{detail.UserId}:{detail.Balance}";
                var expectedHmac = _cryptoService.GenerateHMAC(dataToMac);

                if (detail.HMAC != expectedHmac)
                {
                    _logger.LogError(null, $"Data Integrity violation detected for financial record ID {detail.Id}!");
                    // Do not return tampered data
                    continue;
                }

                validDetails.Add(detail);
            }

            return validDetails;
        }
    }
}
