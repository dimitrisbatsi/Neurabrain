using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Neurabrain.Shared.DTOs
{
    public class ClassroomDto
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Το όνομα της τάξης είναι υποχρεωτικό.")]
        [StringLength(100, ErrorMessage = "Το όνομα δεν μπορεί να ξεπερνά τους 100 χαρακτήρες.")]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public Guid OrganizationId { get; set; } // Για να ξέρουμε σε ποιο φροντιστήριο φτιάχτηκε

        // Για την αποθήκευση: Στέλνουμε στο API τα IDs των μαθητών που επιλέξαμε
        public List<Guid> StudentIds { get; set; } = new List<Guid>();

        // Για την εμφάνιση στο UI: Το API μας επιστρέφει τα ονόματά τους έτοιμα, για να μην κάνουμε έξτρα κλήσεις!
        public List<string> StudentNames { get; set; } = new List<string>();
        public List<string> AssignedExerciseTitles { get; set; } = new List<string>();
    }
}