using Neurabrain.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neurabrain.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public ICollection<UserOrganization> UserOrganizations { get; set; } = new List<UserOrganization>();

        // Ο καθηγητής είναι "Ιδιοκτήτης" του υλικού του, ανεξάρτητα από το φροντιστήριο
        public ICollection<SourceMaterial> OwnedMaterials { get; set; } = new List<SourceMaterial>();

        // Ένας μαθητής (ως User) μπορεί να έχει προφίλ σε διαφορετικά φροντιστήρια
        public ICollection<StudentProfile> StudentProfiles { get; set; } = new List<StudentProfile>();
    }
}
