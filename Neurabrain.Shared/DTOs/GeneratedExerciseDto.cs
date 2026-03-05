using System;

namespace Neurabrain.Shared.DTOs
{
    public class GeneratedExerciseDto
    {
        public Guid Id { get; set; }
        public Guid SourceMaterialId { get; set; }
        public string SourceMaterialTitle { get; set; } = string.Empty;
        public Guid LearningConditionId { get; set; }
        public string LearningConditionName { get; set; } = string.Empty;
        public string AIContentJson { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; }
        //public List<string> AssignedClassroomNames { get; set; } = new List<string>();
        public List<ExerciseAssignmentDto> AssignedClassrooms { get; set; } = new();
    }

    public class ExerciseAssignmentDto
    {
        public Guid ClassroomId { get; set; }
        public string ClassroomName { get; set; } = string.Empty;
    }
}