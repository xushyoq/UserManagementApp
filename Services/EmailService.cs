using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace UserManagement.Services;

/// <summary>
/// IMPORTANT: Email service implementation using MailKit.
/// NOTE: Uses SMTP (Gmail or other provider) to send emails.
/// NOTA BENE: Configuration comes from appsettings.json.
/// </summary>
public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;
    
    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }
    
    /// <summary>
    /// IMPORTANT: Sends confirmation email asynchronously.
    /// NOTE: Uses fire-and-forget pattern to not block registration.
    /// NOTA BENE: Logs errors but doesn't throw to avoid breaking registration.
    /// </summary>
    public async Task SendConfirmationEmailAsync(string toEmail, string userName, string confirmLink)
    {
        try
        {
            // NOTE: Get SMTP configuration
            var smtpHost = _config["Email:SmtpHost"] ?? "smtp.gmail.com";
            var smtpPort = int.Parse(_config["Email:SmtpPort"] ?? "587");
            var smtpUser = _config["Email:SmtpUser"];
            var smtpPass = _config["Email:SmtpPass"];
            var fromName = _config["Email:FromName"] ?? "User Management App";
            
            // NOTA BENE: If email is not configured, log and skip
            if (string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPass))
            {
                _logger.LogWarning("Email not configured. Confirmation link: {Link}", confirmLink);
                return;
            }
            
            // IMPORTANT: Create email message
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, smtpUser));
            message.To.Add(new MailboxAddress(userName, toEmail));
            message.Subject = "Confirm your email";
            
            // NOTE: Create email body
            var bodyBuilder = new BodyBuilder
            {
                TextBody = $@"Hello, {userName}!

Thank you for registering. Please confirm your email by clicking the link below:

{confirmLink}

If you didn't register, please ignore this email.

Best regards,
User Management App"
            };
            message.Body = bodyBuilder.ToMessageBody();
            
            // IMPORTANT: Send email via SMTP
            using var client = new SmtpClient();
            await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(smtpUser, smtpPass);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
            
            _logger.LogInformation("Confirmation email sent to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            // NOTA BENE: Log error but don't throw - email failure shouldn't break registration
            _logger.LogError(ex, "Failed to send confirmation email to {Email}", toEmail);
        }
    }
}


