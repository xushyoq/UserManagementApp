using System.Net.Http.Json;
using System.Text.Json;

namespace UserManagement.Services;

/// <summary>
/// IMPORTANT: Email service implementation using Resend API.
/// NOTE: Uses Resend REST API instead of SMTP (works on Render which blocks SMTP ports).
/// NOTA BENE: Configuration comes from appsettings.json or Environment Variables.
/// </summary>
public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;
    private readonly HttpClient _httpClient;
    
    public EmailService(IConfiguration config, ILogger<EmailService> logger, IHttpClientFactory httpClientFactory)
    {
        _config = config;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
    }
    
    /// <summary>
    /// IMPORTANT: Sends confirmation email asynchronously via Resend API.
    /// NOTE: Uses fire-and-forget pattern to not block registration.
    /// NOTA BENE: Logs errors but doesn't throw to avoid breaking registration.
    /// </summary>
    public async Task SendConfirmationEmailAsync(string toEmail, string userName, string confirmLink)
    {
        try
        {
            // NOTE: Get Resend API configuration
            var apiKey = _config["Email:ResendApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogWarning("Resend API key not configured. Confirmation link: {Link}", confirmLink);
                return;
            }
            
            var fromEmail = _config["Email:FromEmail"] ?? "onboarding@resend.dev";
            var fromName = _config["Email:FromName"] ?? "User Management App";
            
            // IMPORTANT: Prepare Resend API request
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            
            var requestBody = new
            {
                from = $"{fromName} <{fromEmail}>",
                to = new[] { toEmail },
                subject = "Confirm your email",
                html = $@"<p>Hello, {userName}!</p>
                    <p>Thank you for registering. Please confirm your email by clicking the link below:</p>
                    <p><a href=""{confirmLink}"">{confirmLink}</a></p>
                    <p>If you didn't register, please ignore this email.</p>
                    <p>Best regards,<br/>User Management App</p>"
            };
            
            // NOTE: Send email via Resend API
            var response = await _httpClient.PostAsJsonAsync(
                "https://api.resend.com/emails", 
                requestBody);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<JsonElement>();
                var emailId = result.GetProperty("id").GetString();
                _logger.LogInformation("Confirmation email sent to {Email} via Resend. Email ID: {Id}", 
                    toEmail, emailId);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Resend API error: {StatusCode} - {Error}", 
                    response.StatusCode, errorContent);
            }
        }
        catch (Exception ex)
        {
            // NOTA BENE: Log error but don't throw - email failure shouldn't break registration
            _logger.LogError(ex, "Failed to send confirmation email to {Email}", toEmail);
        }
    }
}


