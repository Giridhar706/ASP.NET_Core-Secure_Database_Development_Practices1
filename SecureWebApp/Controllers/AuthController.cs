using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureWebApp.Models.DTOs;
using SecureWebApp.Services;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SecureWebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ISecureLogger<AuthController> _logger;
        private readonly IAntiforgery _antiforgery;

        public AuthController(IUserService userService, ISecureLogger<AuthController> logger, IAntiforgery antiforgery)
        {
            _userService = userService;
            _logger = logger;
            _antiforgery = antiforgery;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        // In a real API using cookies, you might enforce AntiForgery even for registration, but usually not strictly needed if there's no active session.
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userService.RegisterUserAsync(dto.Email, dto.Password, dto.Role);
            if (user == null)
            {
                return BadRequest(new { Message = "Registration failed." });
            }

            return Ok(new { Message = "Registration successful." });
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userService.AuthenticateAsync(dto.Email, dto.Password);
            if (user == null)
            {
                return Unauthorized(new { Message = "Invalid credentials." });
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));

            _logger.LogInformation($"User {user.Email} logged in successfully and session cookie created.");

            return Ok(new { Message = "Login successful." });
        }

        [HttpPost("logout")]
        [Authorize]
        [ValidateAntiForgeryToken] // Protect state-changing logout with Anti-Forgery
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(new { Message = "Logged out successfully." });
        }

        [HttpGet("csrf-token")]
        [AllowAnonymous]
        public IActionResult GetCsrfToken()
        {
            var tokens = _antiforgery.GetAndStoreTokens(HttpContext);
            return Ok(new { token = tokens.RequestToken });
        }
    }
}
