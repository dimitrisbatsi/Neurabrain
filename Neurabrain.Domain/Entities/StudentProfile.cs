using Neurabrain.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neurabrain.Domain.Entities
{
    public class StudentProfile
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // Σε ποιον User ανήκει το προφίλ
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        // Το TenantId (Σε ποιο Φροντιστήριο δημιουργήθηκε/ανήκει αυτό το προφίλ)
        public Guid OrganizationId { get; set; }
        public Organization Organization { get; set; } = null!;

        // Foreign Key στη νέα μας βάση Μαθησιακών Δυσκολιών (αντί για Enum)
        public Guid LearningConditionId { get; set; }
        public LearningCondition LearningCondition { get; set; } = null!;

        public string UIPreferencesJson { get; set; } = "{}";
    }
}
