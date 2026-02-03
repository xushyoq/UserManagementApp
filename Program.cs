using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using UserManagement.Data;
using UserManagement.Filters;
using UserManagement.Services;

/// <summary>
/// IMPORTANT: Application entry point and configuration.
/// NOTE: Configures services, authentication, and middleware pipeline.
/// </summary>

var builder = WebApplication.CreateBuilder(args);

// IMPORTANT: Configure PostgreSQL database connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// IMPORTANT: Configure cookie authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // NOTE: Redirect to login page when not authenticated
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/Login";
        
        // NOTA BENE: Cookie settings
        options.Cookie.Name = "UserManagement.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    });

// NOTE: Register email service
builder.Services.AddScoped<IEmailService, EmailService>();

// NOTE: Register the CheckUserStatus filter for DI
builder.Services.AddScoped<CheckUserStatusAttribute>();

// IMPORTANT: Add MVC with views
builder.Services.AddControllersWithViews();

var app = builder.Build();

// IMPORTANT: Apply database migrations automatically on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// NOTE: Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    // IMPORTANT: Only redirect to HTTPS in production
    app.UseHttpsRedirection();
}
else
{
    // NOTE: Skip HTTPS redirect in Development to avoid warning
    // NOTA BENE: Can enable if using HTTPS profile in launchSettings.json
}

app.UseStaticFiles();

app.UseRouting();

// IMPORTANT: Authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// NOTE: Default route configuration
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
