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
    public class OrganizationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrganizationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/organizations
        [HttpGet]
        [Authorize(Roles = "SuperAdmin, Admin, Organization, Teacher")]
        public async Task<ActionResult<IEnumerable<OrganizationDto>>> GetOrganizations()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var query = _context.Organizations.AsQueryable();

            if (userRole != "SuperAdmin" && userRole != "Admin")
            {
                if (Guid.TryParse(userIdString, out Guid userId))
                {
                    // Φέρε μόνο τα φροντιστήρια στα οποία ο χρήστης ανήκει (μέσω του UserOrganizations)
                    query = query.Where(o => o.UserOrganizations.Any(uo => uo.UserId == userId));
                }
                else
                {
                    return Unauthorized(); // Αν για κάποιο λόγο δεν έχει ID, ρίξε πόρτα.
                }
            }

            var orgs = await query
                .OrderBy(o => o.Name)
                .Select(o => new OrganizationDto
                {
                    Id = o.Id,
                    Name = o.Name,
                    TaxId = o.TaxId,
                    MaxStudents = o.MaxStudents,
                    CreatedAt = o.CreatedAt
                })
                .ToListAsync();

            return Ok(orgs);
        }

        // POST: api/organizations
        [HttpPost]
        [Authorize(Roles = "SuperAdmin, Admin")]
        public async Task<ActionResult<OrganizationDto>> CreateOrganization(OrganizationDto dto)
        {
            var entity = new Organization
            {
                Name = dto.Name,
                TaxId = dto.TaxId,
                MaxStudents = dto.MaxStudents,
                CreatedAt = DateTime.UtcNow
            };

            _context.Organizations.Add(entity);
            await _context.SaveChangesAsync();

            dto.Id = entity.Id;
            dto.CreatedAt = entity.CreatedAt;
            return CreatedAtAction(nameof(GetOrganizations), new { id = entity.Id }, dto);
        }

        // PUT: api/organizations/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin, Admin, Organization")]
        public async Task<IActionResult> UpdateOrganization(Guid id, OrganizationDto dto)
        {
            if (id != dto.Id) return BadRequest("Το ID δεν ταιριάζει.");

            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var entity = await _context.Organizations
                .Include(o => o.UserOrganizations)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (entity == null) return NotFound("Ο οργανισμός δεν βρέθηκε.");

            if (userRole != "SuperAdmin" && userRole != "Admin")
            {
                var userId = Guid.Parse(userIdString!);

                // Αν ο χρήστης που κάνει το request ΔΕΝ ανήκει σε αυτό το φροντιστήριο... Hacker Alert!
                if (!entity.UserOrganizations.Any(uo => uo.UserId == userId))
                {
                    return Forbid(); // HTTP 403: Απαγορεύεται η πρόσβαση
                }
            }

            entity.Name = dto.Name;
            entity.TaxId = dto.TaxId;

            if (userRole == "SuperAdmin" || userRole == "Admin")
            {
                entity.MaxStudents = dto.MaxStudents;
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/organizations/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin, Admin")]

        public async Task<IActionResult> DeleteOrganization(Guid id)
        {
            var entity = await _context.Organizations.FindAsync(id);
            if (entity == null) return NotFound();

            try
            {
                _context.Organizations.Remove(entity);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateException)
            {
                // Πιάνουμε το error σε περίπτωση που υπάρχουν ήδη μαθητές/τάξεις συνδεδεμένοι με το φροντιστήριο
                return BadRequest("Δεν μπορείτε να διαγράψετε αυτό το φροντιστήριο διότι υπάρχουν εγγεγραμμένοι μαθητές ή δεδομένα που εξαρτώνται από αυτό.");
            }
        }
    }
}