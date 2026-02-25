using Microsoft.AspNetCore.Mvc;
using Neurabrain.Shared.DTOs;

namespace Neurabrain.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExercisesController : ControllerBase
    {
        // Εδώ αργότερα θα κάνουμε Inject το DbContext και το AiService
        public ExercisesController()
        {
        }

        [HttpPost("generate")]
        public async Task<ActionResult<ExerciseResponse>> GenerateExercise([FromBody] CreateExerciseRequest request)
        {
            // 1. Εδώ το DTO (CreateExerciseRequest) έρχεται αυτόματα επικυρωμένο από το Blazor!
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 2. Εδώ θα καλούσαμε το GeminiAiService (το αφήνουμε mock για την ώρα)
            // var aiContent = await _aiService.GenerateAsync(request.RawText, request.TargetCondition, request.NumberOfQuestions);

            // 3. Φτιάχνουμε μια εικονική (Mock) απάντηση για να δούμε ότι δουλεύει
            var response = new ExerciseResponse
            {
                ExerciseId = Guid.NewGuid(),
                TargetCondition = request.TargetCondition,
                AIContentJson = "{ \"message\": \"Η AI απάντηση θα μπει εδώ!\" }",
                GeneratedAt = DateTime.UtcNow
            };

            // Επιστρέφουμε HTTP 200 OK μαζί με το αποτέλεσμα (ExerciseResponse DTO)
            return Ok(response);
        }
    }
}