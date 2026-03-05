using System;
using System.ComponentModel.DataAnnotations;

namespace Neurabrain.Shared.DTOs
{
    public class SourceMaterialDto
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Ο τίτλος είναι υποχρεωτικός.")]
        [StringLength(200, ErrorMessage = "Ο τίτλος δεν μπορεί να ξεπερνά τους 200 χαρακτήρες.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Το περιεχόμενο (κείμενο) είναι υποχρεωτικό.")]
        public string RawContent { get; set; } = string.Empty;

        public Guid OwnerUserId { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}