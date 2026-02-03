using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using UserManagement.Data;
using UserManagement.Models;

namespace UserManagement.Filters;

/// <summary>
/// IMPORTANT: Action filter that checks user status before EVERY request.
/// NOTE: This is the SINGLE place where user blocking/deletion is checked.
/// NOTA BENE: If user is blocked or deleted, they are redirected to login.
/// </summary>
public class CheckUserStatusAttribute : ActionFilterAttribute
{
    /// <summary>
    /// IMPORTANT: Runs before each controller action.
    /// NOTE: Checks if current user exists and is not blocked.
    /// NOTA BENE: Redirects to login with message if user is invalid.
    /// </summary>
    public override async Task OnActionExecutionAsync(
        ActionExecutingContext context, 
        ActionExecutionDelegate next)
    {
        // NOTE: Get user ID from authentication claims
        var userIdClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
        
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
        {
            // IMPORTANT: Get database context from DI
            var dbContext = context.HttpContext.RequestServices
                .GetRequiredService<AppDbContext>();
            
            // NOTE: Check if user exists in database
            var user = await dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId);
            
            if (user == null)
            {
                // NOTA BENE: User was deleted - sign out and redirect
                await SignOutAndRedirect(context, "Your account has been deleted.");
                return;
            }
            
            if (user.Status == UserStatus.Blocked)
            {
                // NOTA BENE: User was blocked - sign out and redirect
                await SignOutAndRedirect(context, "Your account has been blocked.");
                return;
            }
        }
        
        // NOTE: User is valid, proceed to controller action
        await next();
    }
    
    /// <summary>
    /// IMPORTANT: Signs out user and redirects to login page.
    /// NOTE: Stores error message in TempData for display.
    /// </summary>
    private async Task SignOutAndRedirect(ActionExecutingContext context, string message)
    {
        // NOTE: Sign out the user
        await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        
        // NOTA BENE: Store message for display on login page
        var tempDataFactory = context.HttpContext.RequestServices
            .GetRequiredService<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataDictionaryFactory>();
        var tempData = tempDataFactory.GetTempData(context.HttpContext);
        tempData["Error"] = message;
        
        // NOTE: Redirect to login page
        context.Result = new RedirectToActionResult("Login", "Account", null);
    }
}

