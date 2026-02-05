namespace UserManagement.Services;

/// <summary>
/// IMPORTANT: Interface for email sending service.
/// NOTE: Abstraction allows for easy testing and swapping implementations.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// IMPORTANT: Sends email confirmation asynchronously.
    /// NOTE: Should not block registration process.
    /// </summary>
    /// <param name="toEmail">Recipient email address</param>
    /// <param name="userName">User's display name</param>
    /// <param name="confirmLink">Full URL for email confirmation</param>
    Task SendConfirmationEmailAsync(string toEmail, string userName, string confirmLink);
}



