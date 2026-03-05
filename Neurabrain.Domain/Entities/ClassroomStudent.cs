using System;

namespace Neurabrain.Domain.Entities
{
    public class ClassroomStudent
    {
        public Guid ClassroomId { get; set; }
        public Classroom Classroom { get; set; } = null!;

        public Guid StudentId { get; set; }
        public User Student { get; set; } = null!;

        public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
    }
}