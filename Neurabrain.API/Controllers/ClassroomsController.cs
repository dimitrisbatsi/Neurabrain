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
    [Authorize(Roles = "Teacher")] // Μόνο οι καθηγητές διαχειρίζονται τις δικές τους τάξεις
    public class ClassroomsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ClassroomsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClassroomDto>>> GetMyClassrooms()
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var classrooms = await _context.Classrooms
                .Include(c => c.ClassroomStudents)
                    .ThenInclude(cs => cs.Student)
                .Include(c => c.Assignments)                   // <-- ΝΕΟ
                    .ThenInclude(a => a.GeneratedExercise)     // <-- ΝΕΟ
                        .ThenInclude(ge => ge.SourceMaterial)  // <-- ΝΕΟ (για να πάρουμε τον τίτλο)
                .Where(c => c.TeacherId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new ClassroomDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    OrganizationId = c.OrganizationId,
                    StudentIds = c.ClassroomStudents.Select(cs => cs.StudentId).ToList(),
                    StudentNames = c.ClassroomStudents.Select(cs => cs.Student.FullName).ToList(),
                    // <-- ΝΕΟ: Μαζεύουμε τους τίτλους των ασκήσεων
                    AssignedExerciseTitles = c.Assignments.Select(a => a.GeneratedExercise.SourceMaterial.Title).ToList()
                })
                .ToListAsync();

            return Ok(classrooms);
        }

        [HttpPost]
        public async Task<ActionResult<ClassroomDto>> CreateClassroom(ClassroomDto dto)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            // Έλεγχος Ασφαλείας: Ανήκει ο Καθηγητής σε αυτό το Φροντιστήριο;
            var isTeacherInOrg = await _context.UserOrganizations
                .AnyAsync(uo => uo.UserId == userId && uo.OrganizationId == dto.OrganizationId);

            if (!isTeacherInOrg) return Forbid();

            var classroom = new Classroom
            {
                Name = dto.Name,
                Description = dto.Description,
                TeacherId = userId,
                OrganizationId = dto.OrganizationId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Classrooms.Add(classroom);

            // Προσθήκη Μαθητών στον ενδιάμεσο πίνακα
            if (dto.StudentIds != null && dto.StudentIds.Any())
            {
                foreach (var studentId in dto.StudentIds)
                {
                    _context.ClassroomStudents.Add(new ClassroomStudent
                    {
                        Classroom = classroom,
                        StudentId = studentId
                    });
                }
            }

            await _context.SaveChangesAsync();
            dto.Id = classroom.Id;
            return CreatedAtAction(nameof(GetMyClassrooms), new { id = classroom.Id }, dto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateClassroom(Guid id, ClassroomDto dto)
        {
            if (id != dto.Id) return BadRequest();

            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var classroom = await _context.Classrooms
                .Include(c => c.ClassroomStudents)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (classroom == null) return NotFound();
            if (classroom.TeacherId != userId) return Forbid(); // Μόνο ο ιδιοκτήτης κάνει update!

            classroom.Name = dto.Name;
            classroom.Description = dto.Description;

            // Ενημέρωση Μαθητών: Διαγράφουμε τις παλιές συνδέσεις και βάζουμε τις νέες
            _context.ClassroomStudents.RemoveRange(classroom.ClassroomStudents);

            if (dto.StudentIds != null && dto.StudentIds.Any())
            {
                foreach (var studentId in dto.StudentIds)
                {
                    _context.ClassroomStudents.Add(new ClassroomStudent
                    {
                        ClassroomId = classroom.Id,
                        StudentId = studentId
                    });
                }
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClassroom(Guid id)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var classroom = await _context.Classrooms.FindAsync(id);

            if (classroom == null) return NotFound();
            if (classroom.TeacherId != userId) return Forbid();

            _context.Classrooms.Remove(classroom);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}