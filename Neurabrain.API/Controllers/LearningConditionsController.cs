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
    [Authorize]
    public class LearningConditionsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LearningConditionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/learningconditions (Φέρνει όλη τη λίστα)
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<LearningConditionDto>>> GetLearningConditions()
        {
            var conditions = await _context.LearningConditions
                .OrderBy(c => c.Name)
                .Select(c => new LearningConditionDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    AiPromptInstruction = c.AiPromptInstruction,
                    IsActive = c.IsActive
                })
                .ToListAsync();

            return Ok(conditions);
        }

        // POST: api/learningconditions (Δημιουργεί μια νέα εγγραφή)
        [HttpPost]
        [Authorize(Roles = "SuperAdmin, Admin")]
        public async Task<ActionResult<LearningConditionDto>> CreateLearningCondition(LearningConditionDto dto)
        {
            var entity = new LearningCondition
            {
                Name = dto.Name,
                Description = dto.Description,
                AiPromptInstruction = dto.AiPromptInstruction,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _context.LearningConditions.Add(entity);
            await _context.SaveChangesAsync();

            dto.Id = entity.Id; // Επιστρέφουμε το νέο ID που πήρε από τη βάση
            return CreatedAtAction(nameof(GetLearningConditions), new { id = entity.Id }, dto);
        }

        // PUT: api/learningconditions/{id} (Ενημερώνει μια εγγραφή)
        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin, Admin")]
        public async Task<IActionResult> UpdateLearningCondition(Guid id, LearningConditionDto dto)
        {
            if (id != dto.Id) return BadRequest("Το ID δεν ταιριάζει.");

            var entity = await _context.LearningConditions.FindAsync(id);
            if (entity == null) return NotFound("Η εγγραφή δεν βρέθηκε.");

            entity.Name = dto.Name;
            entity.Description = dto.Description;
            entity.AiPromptInstruction = dto.AiPromptInstruction;
            entity.IsActive = dto.IsActive;
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/learningconditions/{id} (Διαγράφει μια εγγραφή)
        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin, Admin")]
        public async Task<IActionResult> DeleteLearningCondition(Guid id)
        {
            var entity = await _context.LearningConditions.FindAsync(id);
            if (entity == null) return NotFound();

            try
            {
                _context.LearningConditions.Remove(entity);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateException)
            {
                // Αν η διαγραφή "σκάσει" λόγω του DeleteBehavior.Restrict που βάλαμε!
                return BadRequest("Δεν μπορείτε να διαγράψετε αυτή την κατηγορία διότι υπάρχουν μαθητές ή ασκήσεις που την χρησιμοποιούν. Προτιμήστε να την κάνετε 'Ανενεργή'.");
            }
        }
    }
}