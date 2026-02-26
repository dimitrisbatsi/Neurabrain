using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Neurabrain.Domain.Entities;
using Neurabrain.Infrastructure.Data;
using Neurabrain.Shared.DTOs;

namespace Neurabrain.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "SuperAdmin")]
    public class SettingsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SettingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<SystemSettingsDto>> GetSettings()
        {
            var geminiKeySetting = await _context.SystemSettings
                .FirstOrDefaultAsync(s => s.Key == "GeminiApiKey");

            var dto = new SystemSettingsDto
            {
                GeminiApiKey = geminiKeySetting?.Value ?? string.Empty
            };

            return Ok(dto);
        }

        [HttpPost]
        public async Task<IActionResult> SaveSettings([FromBody] SystemSettingsDto dto)
        {
            var geminiKeySetting = await _context.SystemSettings
                .FirstOrDefaultAsync(s => s.Key == "GeminiApiKey");

            if (geminiKeySetting == null)
            {
                // Αν δεν υπάρχει, το δημιουργούμε
                _context.SystemSettings.Add(new SystemSetting
                {
                    Key = "GeminiApiKey",
                    Value = dto.GeminiApiKey,
                    LastUpdated = DateTime.UtcNow
                });
            }
            else
            {
                // Αν υπάρχει, απλώς το ενημερώνουμε
                geminiKeySetting.Value = dto.GeminiApiKey;
                geminiKeySetting.LastUpdated = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
