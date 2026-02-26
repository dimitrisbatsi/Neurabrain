using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neurabrain.Domain.Entities
{
    public class UserOrganization
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public Guid OrganizationId { get; set; }
        public Organization Organization { get; set; } = null!;

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }
}
