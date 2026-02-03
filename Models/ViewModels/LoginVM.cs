using System.ComponentModel.DataAnnotations;

namespace UserManagement.Models.ViewModels;

/// <summary>
/// IMPORTANT: View model for user login.
/// NOTE: Only email and password are required.
/// </summary>
public class LoginVM
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// NOTA BENE: Any non-empty password is valid (even 1 character).
    /// </summary>
    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;
}


