using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserManagement.Data;
using UserManagement.Helpers;
using UserManagement.Models;
using UserManagement.Models.ViewModels;
using UserManagement.Services;
using BC = BCrypt.Net.BCrypt;

namespace UserManagement.Controllers;

/// <summary>
/// IMPORTANT: Controller for user authentication (login, register, logout).
/// NOTA BENE: AllowAnonymous attribute bypasses the CheckUserStatus filter.
/// </summary>
[AllowAnonymous]
public class AccountController : Controller
{
    private readonly AppDbContext _db;
    private readonly IEmailService _emailService;
    private readonly ILogger<AccountController> _logger;
    
    public AccountController(
        AppDbContext db, 
        IEmailService emailService,
        ILogger<AccountController> logger)
    {
        _db = db;
        _emailService = emailService;
        _logger = logger;
    }
    
    /// <summary>
    /// NOTE: Display login form.
    /// </summary>
    [HttpGet]
    public IActionResult Login()
    {
        // NOTA BENE: If already authenticated, redirect to users list
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Users");
        }
        return View();
    }
    
    /// <summary>
    /// IMPORTANT: Process login form submission.
    /// NOTE: Validates credentials and creates authentication cookie.
    /// NOTA BENE: Updates LastLoginTime on successful login.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginVM model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        
        // NOTE: Find user by email (case-insensitive)
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == model.Email.ToLower());
        
        if (user == null)
        {
            ModelState.AddModelError("", "Invalid email or password.");
            return View(model);
        }
        
        // IMPORTANT: Check if user is blocked
        if (user.Status == UserStatus.Blocked)
        {
            ModelState.AddModelError("", "Your account has been blocked.");
            return View(model);
        }
        
        // NOTE: Verify password hash
        if (!BC.Verify(model.Password, user.PasswordHash))
        {
            ModelState.AddModelError("", "Invalid email or password.");
            return View(model);
        }
        
        // IMPORTANT: Update last login time
        user.LastLoginTime = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        
        // NOTE: Create authentication claims
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email)
        };
        
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        
        // NOTA BENE: Sign in with cookie authentication
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme, 
            principal);
        
        _logger.LogInformation("User {Email} logged in successfully", user.Email);
        
        return RedirectToAction("Index", "Users");
    }
    
    /// <summary>
    /// NOTE: Display registration form.
    /// </summary>
    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Users");
        }
        return View();
    }
    
    /// <summary>
    /// IMPORTANT: Process registration form submission.
    /// NOTE: Creates new user with Unverified status.
    /// NOTA BENE: Sends confirmation email asynchronously (fire and forget).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterVM model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        
        // IMPORTANT: Generate confirmation token using helper function
        var confirmationToken = IdHelper.GetUniqIdValue();
        
        // NOTE: Create new user
        var user = new User
        {
            Name = model.Name,
            Email = model.Email,
            PasswordHash = BC.HashPassword(model.Password),
            Status = UserStatus.Unverified,
            RegistrationTime = DateTime.UtcNow,
            EmailConfirmationToken = confirmationToken
        };
        
        try
        {
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            // NOTA BENE: Check for unique constraint violation (PostgreSQL error code 23505)
            var innerMessage = ex.InnerException?.Message ?? "";
            var isUniqueViolation = innerMessage.Contains("duplicate", StringComparison.OrdinalIgnoreCase) ||
                                    innerMessage.Contains("unique", StringComparison.OrdinalIgnoreCase) ||
                                    innerMessage.Contains("23505"); // PostgreSQL unique violation code
            
            if (isUniqueViolation)
            {
                // IMPORTANT: Unique index violation - email already exists
                ModelState.AddModelError("Email", "This email is already registered.");
                return View(model);
            }
            
            // NOTE: Re-throw if it's a different database error
            throw;
        }
        
        // IMPORTANT: Send confirmation email asynchronously (fire and forget)
        var confirmLink = Url.Action(
            "ConfirmEmail", 
            "Account", 
            new { token = confirmationToken }, 
            Request.Scheme);
        
        // NOTE: Don't await - let it run in background
        _ = _emailService.SendConfirmationEmailAsync(user.Email, user.Name, confirmLink!);
        
        _logger.LogInformation("User {Email} registered successfully", user.Email);
        
        TempData["Success"] = "Registration successful! Please check your email to confirm your account.";
        return RedirectToAction("Login");
    }
    
    /// <summary>
    /// IMPORTANT: Confirm user email via token link.
    /// NOTE: Changes status from Unverified to Active.
    /// NOTA BENE: Blocked status stays Blocked even after confirmation.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ConfirmEmail(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            TempData["Error"] = "Invalid confirmation link.";
            return RedirectToAction("Login");
        }
        
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.EmailConfirmationToken == token);
        
        if (user == null)
        {
            TempData["Error"] = "Invalid or expired confirmation link.";
            return RedirectToAction("Login");
        }
        
        // IMPORTANT: Only change Unverified to Active (Blocked stays Blocked)
        if (user.Status == UserStatus.Unverified)
        {
            user.Status = UserStatus.Active;
        }
        
        // NOTE: Clear token after use
        user.EmailConfirmationToken = null;
        await _db.SaveChangesAsync();
        
        _logger.LogInformation("User {Email} confirmed email", user.Email);
        
        TempData["Success"] = "Email confirmed successfully! You can now login.";
        return RedirectToAction("Login");
    }
    
    /// <summary>
    /// IMPORTANT: Logout current user.
    /// NOTE: Clears authentication cookie.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }
}


