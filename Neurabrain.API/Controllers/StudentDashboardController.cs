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
    [Authorize(Roles = "Student")] // Αυστηρός έλεγχος: Μόνο μαθητές μπαίνουν εδώ!
    public class StudentDashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public StudentDashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("my-assignments")]
        public async Task<ActionResult<IEnumerable<StudentAssignmentDto>>> GetMyAssignments()
        {
            // Παίρνουμε το ID του συνδεδεμένου μαθητή από το Token
            var studentId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            // 1. Βρίσκουμε τα IDs των τάξεων στις οποίες τον έχει βάλει ο καθηγητής
            var myClassroomIds = await _context.ClassroomStudents
                .Where(cs => cs.StudentId == studentId)
                .Select(cs => cs.ClassroomId)
                .ToListAsync();

            if (!myClassroomIds.Any())
            {
                // Αν δεν ανήκει πουθενά, επιστρέφουμε άδεια λίστα (χωρίς σφάλμα)
                return Ok(new List<StudentAssignmentDto>());
            }

            // 2. Φέρνουμε τις Αναθέσεις (Assignments) που αφορούν αυτές τις τάξεις
            var assignments = await _context.Assignments
                .Include(a => a.Classroom)
                .Include(a => a.GeneratedExercise)
                    .ThenInclude(ge => ge.SourceMaterial) // Για να πάρουμε τον τίτλο του κειμένου
                .Where(a => myClassroomIds.Contains(a.ClassroomId))
                .OrderByDescending(a => a.AssignedAt)
                .Select(a => new StudentAssignmentDto
                {
                    AssignmentId = a.Id,
                    ExerciseId = a.GeneratedExerciseId,
                    Title = a.GeneratedExercise.SourceMaterial.Title,
                    ClassroomName = a.Classroom.Name,
                    AssignedAt = a.AssignedAt,
                    DueDate = a.DueDate
                })
                .ToListAsync();

            return Ok(assignments);
        }

        [HttpGet("play/{assignmentId}")]
        public async Task<ActionResult<PlayAssignmentDto>> GetAssignmentToPlay(Guid assignmentId)
        {
            var studentId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            // Φέρνουμε την ανάθεση μαζί με την Τάξη και τους Μαθητές της (για ασφάλεια)
            var assignment = await _context.Assignments
                .Include(a => a.Classroom)
                    .ThenInclude(c => c.ClassroomStudents)
                .Include(a => a.GeneratedExercise)
                    .ThenInclude(ge => ge.SourceMaterial)
                .FirstOrDefaultAsync(a => a.Id == assignmentId);

            if (assignment == null) return NotFound("Η αποστολή δεν βρέθηκε.");

            // ΑΣΦΑΛΕΙΑ: Ελέγχουμε αν ο μαθητής ανήκει σε αυτή την τάξη
            if (!assignment.Classroom.ClassroomStudents.Any(cs => cs.StudentId == studentId))
            {
                return Forbid();
            }

            return Ok(new PlayAssignmentDto
            {
                AssignmentId = assignment.Id,
                Title = assignment.GeneratedExercise.SourceMaterial.Title,
                AIContentJson = assignment.GeneratedExercise.AIContentJson
            });
        }

        [HttpPost("submit-result")]
        public async Task<IActionResult> SubmitResult(SubmitExerciseResultDto dto)
        {
            // 1. Βρίσκουμε με ασφάλεια ποιος είναι ο μαθητής από το Token
            var studentId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            // 2. Ελέγχουμε αν η ανάθεση υπάρχει ΚΑΙ αν ο μαθητής έχει δικαίωμα να τη λύσει (ανήκει στην τάξη)
            var assignment = await _context.Assignments
                .Include(a => a.Classroom)
                    .ThenInclude(c => c.ClassroomStudents)
                .FirstOrDefaultAsync(a => a.Id == dto.AssignmentId);

            if (assignment == null) return NotFound("Η αποστολή δεν βρέθηκε.");

            if (!assignment.Classroom.ClassroomStudents.Any(cs => cs.StudentId == studentId))
            {
                return Forbid(); // Ο μαθητής δεν ανήκει σε αυτή την τάξη!
            }

            // 3. Ελέγχουμε αν έχει ήδη λύσει την άσκηση στο παρελθόν
            var existingResult = await _context.ExerciseResults
                .FirstOrDefaultAsync(er => er.AssignmentId == dto.AssignmentId && er.StudentId == studentId);

            if (existingResult != null)
            {
                // Αν την ξαναλύσει, απλά ενημερώνουμε το σκορ του (κρατάμε την τελευταία προσπάθεια)
                existingResult.Score = dto.Score;
                existingResult.TotalQuestions = dto.TotalQuestions;
                existingResult.TimeSpentSeconds = dto.TimeSpentSeconds;
                existingResult.CompletedAt = DateTime.UtcNow;
            }
            else
            {
                // Νέα καταχώρηση
                var newResult = new ExerciseResult
                {
                    StudentId = studentId,
                    AssignmentId = dto.AssignmentId,
                    Score = dto.Score,
                    TotalQuestions = dto.TotalQuestions,
                    TimeSpentSeconds = dto.TimeSpentSeconds,
                    CompletedAt = DateTime.UtcNow
                };
                _context.ExerciseResults.Add(newResult);
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("my-stats")]
        public async Task<ActionResult<StudentStatsDto>> GetMyStats()
        {
            var studentId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            // Ψάχνουμε όλα τα αποτελέσματα αυτού του μαθητή
            var results = await _context.ExerciseResults
                .Where(er => er.StudentId == studentId)
                .ToListAsync();

            if (!results.Any())
            {
                // Αν δεν έχει παίξει ακόμα, επιστρέφουμε μηδενικά στατιστικά
                return Ok(new StudentStatsDto { TotalScore = 0, CompletedMissions = 0 });
            }

            // Υπολογίζουμε το άθροισμα των πόντων και το πλήθος των ολοκληρωμένων ασκήσεων
            var stats = new StudentStatsDto
            {
                TotalScore = results.Sum(er => er.Score),
                CompletedMissions = results.Count
            };

            return Ok(stats);
        }
    }
}