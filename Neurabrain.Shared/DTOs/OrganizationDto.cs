using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neurabrain.Shared.DTOs
{
    public class OrganizationDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // Το ΑΦΜ του φροντιστηρίου (χρήσιμο για μελλοντική τιμολόγηση)
        public string TaxId { get; set; } = string.Empty;

        // Το όριο των μαθητών που επιτρέπεται να έχει βάσει της συνδρομής του
        public int MaxStudents { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
