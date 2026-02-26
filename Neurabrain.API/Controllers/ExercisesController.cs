using Microsoft.AspNetCore.Mvc;
using Neurabrain.Domain.Interfaces;
using Neurabrain.Shared.DTOs;
using System.Text.Json;

namespace Neurabrain.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExercisesController : ControllerBase
    {
        private readonly IAiService _aiService;

        // Εδώ αργότερα θα κάνουμε Inject το DbContext και το AiService
        public ExercisesController(IAiService aiService)
        {
            _aiService = aiService;
        }

        [HttpPost("generate")]
        public async Task<ActionResult<ExerciseResponse>> GenerateExercise([FromBody] CreateExerciseRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // 1. Καλούμε το AI Service!
                var aiContentJson = await _aiService.GenerateExercisesAsync(
                    request.RawText,
                    request.TargetCondition,
                    request.NumberOfQuestions);

                var beautifiedJson = JsonSerializer.Deserialize<JsonElement>(aiContentJson);

                // 2. Φτιάχνουμε την απάντηση που θα πάει στο Blazor
                var response = new ExerciseResponse
                {
                    ExerciseId = Guid.NewGuid(), // Προς το παρόν βάζουμε ένα τυχαίο ID
                    TargetCondition = request.TargetCondition,
                    AIContentJson = beautifiedJson, // Το "καθαρό" JSON που μας έστειλε η AI
                    GeneratedAt = DateTime.UtcNow
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                // Αν κάτι πάει στραβά (π.χ. λάθος API Key ή πέσει ο server της Google)
                return StatusCode(500, new { message = $"Σφάλμα κατά την επικοινωνία με το AI: {ex.Message}" });
            }
        }
    }
}