using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Neurabrain.Domain.Entities;
using Neurabrain.Infrastructure.Data;
using Neurabrain.Shared.DTOs;
using System.Security.Claims;

namespace Neurabrain.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SourceMaterialsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SourceMaterialsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/sourcematerials
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SourceMaterialDto>>> GetMyMaterials()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (!Guid.TryParse(userIdString, out Guid userId)) return Unauthorized();

            var query = _context.SourceMaterials.AsQueryable();

            // Αν είναι Καθηγητής, βλέπει ΜΟΝΟ τα δικά του. 
            // (Οι Admins θεωρητικά τα βλέπουν όλα για λόγους moderation).
            if (userRole == "Teacher")
            {
                query = query.Where(sm => sm.OwnerUserId == userId);
            }

            var materials = await query
                .OrderByDescending(sm => sm.UploadedAt)
                .Select(sm => new SourceMaterialDto
                {
                    Id = sm.Id,
                    Title = sm.Title,
                    RawContent = sm.RawContent,
                    OwnerUserId = sm.OwnerUserId,
                    UploadedAt = sm.UploadedAt
                })
                .ToListAsync();

            return Ok(materials);
        }

        // GET: api/sourcematerials/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<SourceMaterialDto>> GetMaterial(Guid id)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var sm = await _context.SourceMaterials.FindAsync(id);

            if (sm == null) return NotFound();

            // Απαγορεύουμε την πρόσβαση αν ο καθηγητής ζητάει υλικό άλλου
            if (userRole == "Teacher" && sm.OwnerUserId != userId)
            {
                return Forbid();
            }

            return new SourceMaterialDto
            {
                Id = sm.Id,
                Title = sm.Title,
                RawContent = sm.RawContent,
                OwnerUserId = sm.OwnerUserId,
                UploadedAt = sm.UploadedAt
            };
        }

        // POST: api/sourcematerials
        [HttpPost]
        [Authorize(Roles = "Teacher")] // ΜΟΝΟ οι καθηγητές δημιουργούν υλικό
        public async Task<ActionResult<SourceMaterialDto>> CreateMaterial(SourceMaterialDto dto)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var entity = new SourceMaterial
            {
                Title = dto.Title,
                RawContent = dto.RawContent,
                OwnerUserId = userId, // Το παίρνουμε με ασφάλεια από το Cookie, όχι από το DTO του UI!
                UploadedAt = DateTime.UtcNow
            };

            _context.SourceMaterials.Add(entity);
            await _context.SaveChangesAsync();

            dto.Id = entity.Id;
            dto.OwnerUserId = entity.OwnerUserId;
            dto.UploadedAt = entity.UploadedAt;

            return CreatedAtAction(nameof(GetMaterial), new { id = entity.Id }, dto);
        }

        // PUT: api/sourcematerials/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> UpdateMaterial(Guid id, SourceMaterialDto dto)
        {
            if (id != dto.Id) return BadRequest("Το ID δεν ταιριάζει.");

            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var entity = await _context.SourceMaterials.FindAsync(id);

            if (entity == null) return NotFound();

            // Έλεγχος Ασφαλείας: Μόνο ο ιδιοκτήτης μπορεί να το κάνει update
            if (entity.OwnerUserId != userId) return Forbid();

            entity.Title = dto.Title;
            entity.RawContent = dto.RawContent;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/sourcematerials/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> DeleteMaterial(Guid id)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var entity = await _context.SourceMaterials.FindAsync(id);

            if (entity == null) return NotFound();

            // Έλεγχος Ασφαλείας: Μόνο ο ιδιοκτήτης μπορεί να το διαγράψει
            if (entity.OwnerUserId != userId) return Forbid();

            _context.SourceMaterials.Remove(entity);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}