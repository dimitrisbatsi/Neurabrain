using System;

namespace Neurabrain.Shared.DTOs
{
    public class StudentAssignmentDto
    {
        public Guid AssignmentId { get; set; }
        public Guid ExerciseId { get; set; }
        public string Title { get; set; } = string.Empty; // Ο τίτλος της άσκησης/κειμένου
        public string ClassroomName { get; set; } = string.Empty;
        public DateTime AssignedAt { get; set; }
        public DateTime? DueDate { get; set; }
    }
}