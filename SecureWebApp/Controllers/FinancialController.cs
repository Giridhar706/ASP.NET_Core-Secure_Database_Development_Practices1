using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureWebApp.Models.DTOs;
using SecureWebApp.Services;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SecureWebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // Requires User or Admin role to access any endpoint in this controller
    [Authorize(Roles = "User, Admin")]
    public class FinancialController : ControllerBase
    {
        private readonly IFinancialService _financialService;
        private readonly ISecureLogger<FinancialController> _logger;

        public FinancialController(IFinancialService financialService, ISecureLogger<FinancialController> logger)
        {
            _financialService = financialService;
            _logger = logger;
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // CRITICAL: Protect against CSRF for state-changing requests
        public async Task<IActionResult> AddFinancialDetail([FromBody] AddFinancialDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out int userId))
            {
                return Unauthorized();
            }

            var detail = await _financialService.AddFinancialDetailAsync(userId, dto.CardNumber, dto.Balance);
            if (detail == null)
            {
                return StatusCode(500, new { Message = "An error occurred while saving financial data." });
            }

            return Ok(new { Message = "Financial detail added successfully." });
        }

        [HttpGet]
        // GET requests are safe and typically do not require AntiForgery validation
        public async Task<IActionResult> GetMyFinancialDetails()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out int userId))
            {
                return Unauthorized();
            }

            var details = await _financialService.GetUserFinancialDetailsAsync(userId);
            
            // Note: Returning sensitive data to the client. In a real scenario, you would map this to a safe DTO (e.g., masking the card number).
            // But per requirements, we are demonstrating it securely fetched from the encrypted DB.
            var safeDetails = details.ConvertAll(d => new 
            {
                d.Id,
                CardNumber = "****-****-****-" + d.CardNumber.Substring(12),
                d.Balance
            });

            return Ok(safeDetails);
        }
    }
}
