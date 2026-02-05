using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserManagement.Data;
using UserManagement.Filters;
using UserManagement.Models;

namespace UserManagement.Controllers;

/// <summary>
/// IMPORTANT: Controller for user management (admin panel).
/// NOTE: Requires authentication.
/// NOTA BENE: CheckUserStatus filter runs before each action.
/// </summary>
[Authorize]
[TypeFilter(typeof(CheckUserStatusAttribute))]
public class UsersController : Controller
{
    private readonly AppDbContext _db;
    private readonly ILogger<UsersController> _logger;
    
    public UsersController(AppDbContext db, ILogger<UsersController> logger)
    {
        _db = db;
        _logger = logger;
    }
    
    /// <summary>
    /// IMPORTANT: Display list of all users with sorting support.
    /// NOTE: Default sort by LastLoginTime descending (most recent first).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(string sortBy = "LastLoginTime", string sortOrder = "desc")
    {
        var query = _db.Users.AsQueryable();
        
        // IMPORTANT: Apply sorting based on column and order
        // NOTE: Filtering is done client-side for real-time performance
        query = (sortBy?.ToLower(), sortOrder?.ToLower()) switch
        {
            ("name", "asc") => query.OrderBy(u => u.Name),
            ("name", "desc") => query.OrderByDescending(u => u.Name),
            ("email", "asc") => query.OrderBy(u => u.Email),
            ("email", "desc") => query.OrderByDescending(u => u.Email),
            ("status", "asc") => query.OrderBy(u => u.Status),
            ("status", "desc") => query.OrderByDescending(u => u.Status),
            ("lastlogintime", "asc") or ("lastseen", "asc") => query.OrderBy(u => u.LastLoginTime),
            ("lastlogintime", "desc") or ("lastseen", "desc") => query.OrderByDescending(u => u.LastLoginTime),
            _ => query.OrderByDescending(u => u.LastLoginTime) // Default
        };
        
        var users = await query.ToListAsync();
        
        // NOTE: Pass sorting info to view
        ViewBag.SortBy = sortBy;
        ViewBag.SortOrder = sortOrder;
        
        return View(users);
    }
    
    /// <summary>
    /// IMPORTANT: Block selected users.
    /// NOTE: Changes status to Blocked for all selected user IDs.
    /// NOTA BENE: User can block themselves.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Block(int[] selectedIds)
    {
        if (selectedIds == null || selectedIds.Length == 0)
        {
            TempData["Error"] = "No users selected.";
            return RedirectToAction("Index");
        }
        
        var users = await _db.Users
            .Where(u => selectedIds.Contains(u.Id))
            .ToListAsync();
        
        foreach (var user in users)
        {
            user.Status = UserStatus.Blocked;
        }
        
        await _db.SaveChangesAsync();
        
        _logger.LogInformation("Blocked {Count} users: {Ids}", 
            users.Count, string.Join(", ", selectedIds));
        
        TempData["Success"] = $"Successfully blocked {users.Count} user(s).";
        return RedirectToAction("Index");
    }
    
    /// <summary>
    /// IMPORTANT: Unblock selected users.
    /// NOTE: Restores user to their previous status based on email confirmation.
    /// NOTA BENE: If email was confirmed (token is null) → Active, otherwise → Unverified.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unblock(int[] selectedIds)
    {
        if (selectedIds == null || selectedIds.Length == 0)
        {
            TempData["Error"] = "No users selected.";
            return RedirectToAction("Index");
        }
        
        var users = await _db.Users
            .Where(u => selectedIds.Contains(u.Id))
            .ToListAsync();
        
        int unblockedCount = 0;
        foreach (var user in users)
        {
            // IMPORTANT: Only unblock Blocked users
            if (user.Status == UserStatus.Blocked)
            {
                // NOTE: Restore to previous status based on email confirmation
                // If email was confirmed (token is null) → Active
                // If email was not confirmed (token exists) → Unverified
                user.Status = user.EmailConfirmationToken == null 
                    ? UserStatus.Active 
                    : UserStatus.Unverified;
                unblockedCount++;
            }
        }
        
        await _db.SaveChangesAsync();
        
        _logger.LogInformation("Unblocked {Count} users: {Ids}", 
            unblockedCount, string.Join(", ", selectedIds));
        
        if (unblockedCount == 0)
        {
            TempData["Warning"] = "No blocked users were selected to unblock.";
        }
        else
        {
            TempData["Success"] = $"Successfully unblocked {unblockedCount} user(s).";
        }
        
        return RedirectToAction("Index");
    }
    
    /// <summary>
    /// IMPORTANT: Delete selected users permanently.
    /// NOTE: Users are actually deleted, not just marked.
    /// NOTA BENE: User can delete themselves.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int[] selectedIds)
    {
        if (selectedIds == null || selectedIds.Length == 0)
        {
            TempData["Error"] = "No users selected.";
            return RedirectToAction("Index");
        }
        
        var users = await _db.Users
            .Where(u => selectedIds.Contains(u.Id))
            .ToListAsync();
        
        // IMPORTANT: Actually delete users (not mark as deleted)
        _db.Users.RemoveRange(users);
        await _db.SaveChangesAsync();
        
        _logger.LogInformation("Deleted {Count} users: {Ids}", 
            users.Count, string.Join(", ", selectedIds));
        
        TempData["Success"] = $"Successfully deleted {users.Count} user(s).";
        return RedirectToAction("Index");
    }
    
    /// <summary>
    /// IMPORTANT: Delete all unverified users permanently.
    /// NOTE: Deletes ALL users with Unverified status (no selection required).
    /// NOTA BENE: This action works independently of user selection.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUnverified()
    {
        // IMPORTANT: Delete ALL users with Unverified status
        var unverifiedUsers = await _db.Users
            .Where(u => u.Status == UserStatus.Unverified)
            .ToListAsync();
        
        if (unverifiedUsers.Count == 0)
        {
            TempData["Warning"] = "No unverified users found.";
            return RedirectToAction("Index");
        }
        
        // NOTE: Actually delete all unverified users
        _db.Users.RemoveRange(unverifiedUsers);
        await _db.SaveChangesAsync();
        
        _logger.LogInformation("Deleted {Count} unverified users: {Ids}", 
            unverifiedUsers.Count, string.Join(", ", unverifiedUsers.Select(u => u.Id)));
        
        TempData["Success"] = $"Successfully deleted {unverifiedUsers.Count} unverified user(s).";
        return RedirectToAction("Index");
    }
}


