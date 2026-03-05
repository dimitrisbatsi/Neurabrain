using System;
using System.Collections.Generic;

namespace Neurabrain.Domain.Entities
{
    public class Classroom
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty; // π.χ. "Α' Λυκείου - Ιστορία"
        public string? Description { get; set; } // Προαιρετική περιγραφή
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Ο Καθηγητής που διαχειρίζεται την τάξη
        public Guid TeacherId { get; set; }
        public User Teacher { get; set; } = null!;

        // Το Φροντιστήριο στο οποίο ανήκει η τάξη
        public Guid OrganizationId { get; set; }
        public Organization Organization { get; set; } = null!;

        // Λίστα με τους Μαθητές που ανήκουν στην τάξη (μέσω του ενδιάμεσου πίνακα)
        public ICollection<ClassroomStudent> ClassroomStudents { get; set; } = new List<ClassroomStudent>();
        // Λίστα με τις Αναθέσεις που έχουν γίνει σε αυτή την τάξη
        public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    }
}