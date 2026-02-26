using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Neurabrain.Infrastructure.Data;
using Neurabrain.Shared.DTOs;
using System.Security.Claims;

namespace Neurabrain.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto request)
        {
            // 1. Βρίσκουμε τον χρήστη από το Email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            // 2. Ελέγχουμε αν υπάρχει και αν ο κωδικός (hashed) ταιριάζει
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return Unauthorized("Λάθος email ή κωδικός πρόσβασης.");
            }

            // 3. Φτιάχνουμε την "Ταυτότητα" του χρήστη (Claims)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            // 4. Δίνουμε το Cookie στον Browser
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return Ok();
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok();
        }

        [HttpGet("me")]
        public IActionResult GetCurrentUser()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return Unauthorized();
            }

            // Επιστρέφουμε τα βασικά στοιχεία στο UI
            return Ok(new
            {
                Id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                Name = User.Identity.Name,
                Email = User.FindFirst(ClaimTypes.Email)?.Value,
                Role = User.FindFirst(ClaimTypes.Role)?.Value
            });
        }
    }
}