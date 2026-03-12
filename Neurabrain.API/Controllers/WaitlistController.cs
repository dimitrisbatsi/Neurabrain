using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Neurabrain.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous] // Απαραίτητο, αφού όσοι γράφονται δεν έχουν κάνει login!
    public class WaitlistController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public WaitlistController(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        [HttpPost]
        public async Task<IActionResult> JoinWaitlist([FromBody] WaitlistRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest("Το email είναι υποχρεωτικό.");
            }

            // 1. Παίρνουμε το API Key του MailerLite (πρέπει να το βάλεις στο appsettings.json)
            var apiKey = _configuration["MailerLite:ApiKey"];

            if (string.IsNullOrEmpty(apiKey))
            {
                // Αν δεν έχεις σετάρει ακόμα το κλειδί, απλά κάνουμε simulate την επιτυχία για τώρα
                return Ok(new { message = "Simulated success. MailerLite key not configured." });
            }

            try
            {
                // 2. Φτιάχνουμε το αίτημα για το νέο MailerLite API
                var mailerLiteUrl = "https://connect.mailerlite.com/api/subscribers";

                var payload = new
                {
                    email = request.Email,
                    // Προαιρετικά: Αν φτιάξεις ένα Group στο MailerLite για το "Early Access", βάζεις εδώ το ID του
                    // groups = new[] { "123456789" } 
                };

                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                // Προσθέτουμε το Authorization Header
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                // 3. Στέλνουμε το email στο MailerLite
                var response = await _httpClient.PostAsync(mailerLiteUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    return Ok(new { message = "Επιτυχής εγγραφή στη λίστα." });
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    // Log the error in a real app
                    return StatusCode(500, "Προέκυψε σφάλμα κατά την επικοινωνία με τον πάροχο email.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Εσωτερικό σφάλμα διακομιστή.");
            }
        }
    }

    public class WaitlistRequest
    {
        public string Email { get; set; } = string.Empty;
    }
}