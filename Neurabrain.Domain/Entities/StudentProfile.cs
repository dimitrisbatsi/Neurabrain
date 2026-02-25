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

        // Foreign Key
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public ConditionType Condition { get; set; }

        // JSON string για UI preferences (π.χ. font, contrast)
        public string UIPreferencesJson { get; set; } = "{}";
    }
}
