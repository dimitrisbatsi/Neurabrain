using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Neurabrain.Domain.Interfaces;
using Neurabrain.Infrastructure.Data;

namespace Neurabrain.Infrastructure.Services
{
    public class AiService : IAiService
    {
        private readonly HttpClient _httpClient;
        private readonly ApplicationDbContext _dbContext;

        public AiService(HttpClient httpClient, ApplicationDbContext dbContext)
        {
            _httpClient = httpClient;
            _dbContext = dbContext;
        }

        public async Task<string> GenerateExercisesAsync(string rawText, string conditionName, string conditionGuidelines, int numberOfQuestions)
        {
            var apiKeySetting = await _dbContext.SystemSettings.FirstOrDefaultAsync(s => s.Key == "GeminiApiKey");
            if (string.IsNullOrEmpty(apiKeySetting?.Value))
            {
                throw new Exception("Το API Key του AI δεν έχει ρυθμιστεί. Παρακαλώ δηλώστε το στις ρυθμίσεις συστήματος.");
            }

            // Βελτιωμένο Prompt που ενσωματώνει τις δυναμικές οδηγίες της πάθησης
            var prompt = $@"
Είσαι ένας εξειδικευμένος Ειδικός Παιδαγωγός. 
Έχεις το παρακάτω ακαδημαϊκό/σχολικό κείμενο:
'{rawText}'

Ο μαθητής έχει το εξής μαθησιακό προφίλ: {conditionName}.
Ακολούθησε ΑΥΣΤΗΡΑ αυτές τις κατευθυντήριες οδηγίες για τον σχεδιασμό των ασκήσεων: 
{conditionGuidelines}

Δημιούργησε ακριβώς {numberOfQuestions} διαδραστικές ερωτήσεις/ασκήσεις προσαρμοσμένες στις ανάγκες του.
Η επιστροφή πρέπει να είναι ΑΥΣΤΗΡΑ σε μορφή JSON, με ένα array από objects που θα έχουν τα πεδία 'question' και 'answer'. Μην γράψεις κανένα άλλο κείμενο εκτός από το JSON.";

            var requestBody = new
            {
                contents = new[]
                {
                    new { parts = new[] { new { text = prompt } } }
                }
            };

            var requestUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKeySetting.Value}";

            var response = await _httpClient.PostAsJsonAsync(requestUrl, requestBody);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();

            using var document = JsonDocument.Parse(jsonResponse);
            var generatedText = document.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text").GetString();

            if (!string.IsNullOrEmpty(generatedText))
            {
                generatedText = generatedText.Replace("```json", "").Replace("```", "").Trim();
            }

            return generatedText ?? "[]";
        }
    }
}