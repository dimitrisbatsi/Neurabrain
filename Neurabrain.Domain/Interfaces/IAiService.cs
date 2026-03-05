using System.Threading.Tasks;

namespace Neurabrain.Domain.Interfaces
{
    public interface IAiService
    {
        Task<string> GenerateExercisesAsync(string rawText, string conditionName, string conditionGuidelines, int numberOfQuestions);
    }
}