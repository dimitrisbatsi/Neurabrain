using Neurabrain.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neurabrain.Domain.Entities
{
    public class GeneratedExercise
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid SourceMaterialId { get; set; }
        public SourceMaterial SourceMaterial { get; set; } = null!;

        // Foreign Key στη νέα μας βάση Μαθησιακών Δυσκολιών (αντί για Enum)
        public Guid LearningConditionId { get; set; }
        public LearningCondition LearningCondition { get; set; } = null!;

        public string AIContentJson { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        // Λίστα με τις Αναθέσεις που έχουν γίνει με αυτή την άσκηση
        public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    }
}
