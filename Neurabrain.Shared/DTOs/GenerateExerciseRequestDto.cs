using System;

namespace Neurabrain.Shared.DTOs
{
    public class GenerateExerciseRequestDto
    {
        public Guid SourceMaterialId { get; set; }
        public Guid LearningConditionId { get; set; }
        public int NumberOfQuestions { get; set; } = 5;
    }
}