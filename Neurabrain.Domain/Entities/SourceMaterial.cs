using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neurabrain.Domain.Entities
{
    public class SourceMaterial
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = string.Empty;
        public string RawContent { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        // Foreign Key
        public Guid UploadedByUserId { get; set; }
        public User UploadedByUser { get; set; } = null!;

        // Navigation Property
        public ICollection<GeneratedExercise> GeneratedExercises { get; set; } = new List<GeneratedExercise>();
    }
}
