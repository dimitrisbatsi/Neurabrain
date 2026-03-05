using System;

namespace Neurabrain.Domain.Entities
{
    public class Assignment
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // Ποια άσκηση αναθέτουμε;
        public Guid GeneratedExerciseId { get; set; }
        public GeneratedExercise GeneratedExercise { get; set; } = null!;

        // Σε ποια τάξη την αναθέτουμε; (Οι μαθητές θα τη δουν επειδή ανήκουν στην τάξη)
        public Guid ClassroomId { get; set; }
        public Classroom Classroom { get; set; } = null!;

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        // Προαιρετική ημερομηνία λήξης (για να ξέρει ο μαθητής πότε πρέπει να την παραδώσει)
        public DateTime? DueDate { get; set; }
    }
}