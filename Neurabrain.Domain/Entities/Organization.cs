using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neurabrain.Domain.Entities
{
    public class Organization
    {
        public Guid Id { get; set; } = Guid.NewGuid(); // Αυτό είναι το TenantId μας!
        public string Name { get; set; } = string.Empty;
        public string TaxId { get; set; } = string.Empty; // ΑΦΜ
        public int MaxStudents { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public ICollection<UserOrganization> UserOrganizations { get; set; } = new List<UserOrganization>();
        public ICollection<StudentProfile> StudentProfiles { get; set; } = new List<StudentProfile>();
    }
}
