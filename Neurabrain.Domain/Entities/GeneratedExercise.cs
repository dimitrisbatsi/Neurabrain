using Neurabrain.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neurabrain.Domain.Entities
{
    public class GeneratedExercise
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // Foreign Key
        public Guid SourceMaterialId { get; set; }
        public SourceMaterial SourceMaterial { get; set; } = null!;

        public ConditionType TargetCondition { get; set; }
        public string AIContentJson { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }
}
