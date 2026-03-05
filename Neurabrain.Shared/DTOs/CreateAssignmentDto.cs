using System;
using System.ComponentModel.DataAnnotations;

namespace Neurabrain.Shared.DTOs
{
    public class CreateAssignmentDto
    {
        [Required]
        public Guid GeneratedExerciseId { get; set; }

        [Required(ErrorMessage = "Παρακαλώ επιλέξτε τάξη.")]
        public Guid ClassroomId { get; set; }

        public DateTime? DueDate { get; set; }
    }
}