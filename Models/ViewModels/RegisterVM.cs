using System.ComponentModel.DataAnnotations;

namespace UserManagement.Models.ViewModels;

/// <summary>
/// IMPORTANT: View model for user registration.
/// NOTE: Name, email, and password are required.
/// </summary>
public class RegisterVM
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    [Display(Name = "Name")]
    public string Name { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// NOTA BENE: Any non-empty password is valid (even 1 character).
    /// No minimum length requirement per task specification.
    /// </summary>
    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;
}



