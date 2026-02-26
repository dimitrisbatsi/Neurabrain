using Neurabrain.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neurabrain.Domain.Interfaces
{
    public interface IAiService
    {
        // Ζητάμε το αρχικό κείμενο, τη μαθησιακή δυσκολία, και τον αριθμό ερωτήσεων.
        // Επιστρέφει ένα JSON string με τις έτοιμες ασκήσεις.
        Task<string> GenerateExercisesAsync(string rawText, ConditionType condition, int numberOfQuestions);
    }
}
