using Neurabrain.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neurabrain.Shared.DTOs
{
    public class CreateExerciseRequest
    {
        [Required(ErrorMessage = "Το κείμενο είναι υποχρεωτικό.")]
        [MinLength(50, ErrorMessage = "Το κείμενο πρέπει να έχει τουλάχιστον 50 χαρακτήρες.")]
        public string RawText { get; set; } = string.Empty;

        [Required]
        public ConditionType TargetCondition { get; set; }

        // Πόσες ερωτήσεις ζητάει ο καθηγητής (π.χ. 3, 5, 10)
        [Range(1, 20, ErrorMessage = "Ο αριθμός ερωτήσεων πρέπει να είναι από 1 έως 20.")]
        public int NumberOfQuestions { get; set; } = 5;
    }
}
