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
    [Authorize(Roles = "Teacher")]
    public class AssignmentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AssignmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateAssignment(CreateAssignmentDto dto)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            // 1. Έλεγχος: Η τάξη ανήκει σε αυτόν τον καθηγητή;
            var classroom = await _context.Classrooms.FindAsync(dto.ClassroomId);
            if (classroom == null) return NotFound("Η τάξη δεν βρέθηκε.");
            if (classroom.TeacherId != userId) return Forbid();

            // 2. Έλεγχος: Η άσκηση ανήκει σε αυτόν τον καθηγητή;
            var exercise = await _context.GeneratedExercises
                .Include(e => e.SourceMaterial)
                .FirstOrDefaultAsync(e => e.Id == dto.GeneratedExerciseId);

            if (exercise == null) return NotFound("Η άσκηση δεν βρέθηκε.");
            if (exercise.SourceMaterial.OwnerUserId != userId) return Forbid();

            // 3. Δημιουργία της Ανάθεσης
            var assignment = new Assignment
            {
                GeneratedExerciseId = dto.GeneratedExerciseId,
                ClassroomId = dto.ClassroomId,
                DueDate = dto.DueDate?.ToUniversalTime(), // Πάντα UTC στη βάση
                AssignedAt = DateTime.UtcNow
            };

            _context.Assignments.Add(assignment);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("exercise/{exerciseId}/classroom/{classroomId}")]
        public async Task<IActionResult> RemoveAssignment(Guid exerciseId, Guid classroomId)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var assignment = await _context.Assignments
                .Include(a => a.Classroom)
                .FirstOrDefaultAsync(a => a.GeneratedExerciseId == exerciseId && a.ClassroomId == classroomId);

            if (assignment == null) return NotFound();

            // Ασφάλεια: Μόνο ο καθηγητής της τάξης μπορεί να αναιρέσει την ανάθεση
            if (assignment.Classroom.TeacherId != userId) return Forbid();

            _context.Assignments.Remove(assignment);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}