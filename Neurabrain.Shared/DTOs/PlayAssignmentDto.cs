using System;

namespace Neurabrain.Shared.DTOs
{
    public class PlayAssignmentDto
    {
        public Guid AssignmentId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string AIContentJson { get; set; } = string.Empty; // Το JSON με τις ερωτήσεις
    }
}