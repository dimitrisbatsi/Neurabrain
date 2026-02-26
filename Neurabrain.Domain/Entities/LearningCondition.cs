using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neurabrain.Domain.Entities
{
    public class LearningCondition
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string AiPromptInstruction { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties (Ποιοι μαθητές/ασκήσεις χρησιμοποιούν αυτό το prompt)
        public ICollection<StudentProfile> StudentProfiles { get; set; } = new List<StudentProfile>();
        public ICollection<GeneratedExercise> GeneratedExercises { get; set; } = new List<GeneratedExercise>();
    }
}
