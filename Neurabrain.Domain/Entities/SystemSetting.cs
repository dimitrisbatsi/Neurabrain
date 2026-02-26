using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neurabrain.Domain.Entities
{
    public class SystemSetting
    {
        // Το Key θα είναι το Primary Key μας (π.χ. "GeminiApiKey")
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}
