using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Neurabrain.Domain.Entities;
using Neurabrain.Domain.Interfaces;
using Neurabrain.Infrastructure.Data;
using Neurabrain.Shared.DTOs;
using System.Security.Claims;

namespace Neurabrain.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Teacher")]
    public class GeneratedExercisesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IAiService _aiService;

        public GeneratedExercisesController(ApplicationDbContext context, IAiService aiService)
        {
            _context = context;
            _aiService = aiService;
        }

        [HttpPost("generate")]
        public async Task<ActionResult<GeneratedExerciseDto>> GenerateExercise(GenerateExerciseRequestDto request)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            // 1. Φέρνουμε το κείμενο και ελέγχουμε αν ανήκει στον Καθηγητή
            var sourceMaterial = await _context.SourceMaterials.FindAsync(request.SourceMaterialId);
            if (sourceMaterial == null) return NotFound("Το κείμενο δεν βρέθηκε.");
            if (sourceMaterial.OwnerUserId != userId) return Forbid(); // Ασφάλεια!

            // 2. Φέρνουμε την Πάθηση / Προφίλ (Υποθέτω ότι το Entity σου έχει Name & Prompt/Description)
            var condition = await _context.LearningConditions.FindAsync(request.LearningConditionId);
            if (condition == null) return NotFound("Το μαθησιακό προφίλ δεν βρέθηκε.");

            // 3. Καλούμε το Gemini AI Service
            // Σημείωση: Αν το πεδίο οδηγιών σου λέγεται αλλιώς (π.χ. Description), άλλαξέ το εδώ
            var aiJsonResult = await _aiService.GenerateExercisesAsync(
                sourceMaterial.RawContent,
                condition.Name,
                condition.Description ?? "", // Το prompt του admin
                request.NumberOfQuestions);

            // 4. Αποθηκεύουμε στη Βάση
            var exercise = new GeneratedExercise
            {
                SourceMaterialId = sourceMaterial.Id,
                LearningConditionId = condition.Id,
                AIContentJson = aiJsonResult,
                GeneratedAt = DateTime.UtcNow
            };

            _context.GeneratedExercises.Add(exercise);
            await _context.SaveChangesAsync();

            // 5. Επιστρέφουμε το αποτέλεσμα στο UI
            return Ok(new GeneratedExerciseDto
            {
                Id = exercise.Id,
                SourceMaterialId = exercise.SourceMaterialId,
                SourceMaterialTitle = sourceMaterial.Title,
                LearningConditionId = exercise.LearningConditionId,
                LearningConditionName = condition.Name,
                AIContentJson = exercise.AIContentJson,
                GeneratedAt = exercise.GeneratedAt
            });
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<GeneratedExerciseDto>>> GetMyExercises()
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var exercises = await _context.GeneratedExercises
                .Include(e => e.SourceMaterial)
                .Include(e => e.LearningCondition)
                .Include(e => e.Assignments)          // <-- ΝΕΟ
                    .ThenInclude(a => a.Classroom)    // <-- ΝΕΟ
                .Where(e => e.SourceMaterial.OwnerUserId == userId)
                .OrderByDescending(e => e.GeneratedAt)
                .Select(e => new GeneratedExerciseDto
                {
                    Id = e.Id,
                    SourceMaterialId = e.SourceMaterialId,
                    SourceMaterialTitle = e.SourceMaterial.Title,
                    LearningConditionId = e.LearningConditionId,
                    LearningConditionName = e.LearningCondition.Name,
                    AIContentJson = e.AIContentJson,
                    GeneratedAt = e.GeneratedAt,
                    AssignedClassrooms = e.Assignments.Select(a => new ExerciseAssignmentDto
                    {
                        ClassroomId = a.ClassroomId,
                        ClassroomName = a.Classroom.Name
                    }).ToList()
                })
                .ToListAsync();

            return Ok(exercises);
        }
    }
}