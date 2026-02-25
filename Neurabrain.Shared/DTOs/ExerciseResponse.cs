using Neurabrain.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neurabrain.Shared.DTOs
{
    public class ExerciseResponse
    {
        public Guid ExerciseId { get; set; }
        public ConditionType TargetCondition { get; set; }

        // Το τελικό JSON που θα κάνει parse το Blazor για να φτιάξει τα UI components
        public string AIContentJson { get; set; } = string.Empty;

        public DateTime GeneratedAt { get; set; }
    }
}
