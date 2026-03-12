using System;
using System.ComponentModel.DataAnnotations;

namespace Neurabrain.Shared.DTOs
{
    public class SubmitExerciseResultDto
    {
        [Required]
        public Guid AssignmentId { get; set; }

        public int Score { get; set; }
        public int TotalQuestions { get; set; }

        // Για τα Analytics μας: πόσο χρόνο έκανε το παιδί να λύσει την άσκηση
        public int TimeSpentSeconds { get; set; }
    }
}