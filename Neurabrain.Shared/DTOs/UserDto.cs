using Neurabrain.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neurabrain.Shared.DTOs
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        // Χρησιμοποιούμε string για τον ρόλο ώστε να περνάει εύκολα μέσω JSON
        public string Role { get; set; } = "Teacher";

        // Ο κωδικός πρόσβασης (θα τον στέλνουμε μόνο κατά τη δημιουργία/επεξεργασία)
        public string Password { get; set; } = string.Empty;

        // Η λίστα με τα ID των φροντιστηρίων στα οποία εργάζεται
        public List<Guid> OrganizationIds { get; set; } = new();

        public DateTime CreatedAt { get; set; }
    }
}
