using System;

namespace Neurabrain.Domain.Entities
{
    public class ExerciseResult
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // Ποιος μαθητής έλυσε την άσκηση;
        public Guid StudentId { get; set; }
        public User Student { get; set; } = null!;

        // Ποια συγκεκριμένη Ανάθεση (Assignment) έλυσε;
        public Guid AssignmentId { get; set; }
        public Assignment Assignment { get; set; } = null!;

        // Βαθμολογία
        public int Score { get; set; }
        public int TotalQuestions { get; set; }

        // Προαιρετικά: Χρόνος που χρειάστηκε σε δευτερόλεπτα (πολύτιμο για τα Analytics του Καθηγητή/Γονιού)
        public int TimeSpentSeconds { get; set; }

        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
    }
}