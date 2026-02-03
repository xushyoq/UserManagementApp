using System.ComponentModel.DataAnnotations;

namespace UserManagement.Models;

/// <summary>
/// IMPORTANT: User entity representing application users.
/// NOTE: Email has a unique index in the database (see AppDbContext).
/// NOTA BENE: Status determines user access - Blocked users cannot login.
/// </summary>
public class User
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// IMPORTANT: Email must be unique - enforced by database unique index.
    /// </summary>
    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public string PasswordHash { get; set; } = string.Empty;
    
    /// <summary>
    /// NOTE: Default status is Unverified until email confirmation.
    /// </summary>
    public UserStatus Status { get; set; } = UserStatus.Unverified;
    
    /// <summary>
    /// NOTA BENE: Updated on each successful login.
    /// </summary>
    public DateTime? LastLoginTime { get; set; }
    
    public DateTime RegistrationTime { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// NOTE: Token for email confirmation link. Null after confirmation.
    /// </summary>
    public string? EmailConfirmationToken { get; set; }
}

/// <summary>
/// IMPORTANT: User status enumeration.
/// NOTE: Unverified - email not confirmed yet (can still login).
/// NOTE: Active - email confirmed.
/// NOTA BENE: Blocked - user cannot login or perform any actions.
/// </summary>
public enum UserStatus
{
    Unverified = 0,
    Active = 1,
    Blocked = 2
}


