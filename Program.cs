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

// NOTE: Convert Render PostgreSQL URL format to standard Npgsql format (if needed)
// NOTA BENE: This allows both local format (Host=...;Port=...) and Render URL format (postgresql://...)
var finalConnectionString = connectionString;
if (connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase) ||
    connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase))
{
    // IMPORTANT: Parse PostgreSQL URL format (postgresql://user:pass@host:port/db)
    var uri = new Uri(connectionString);
    var userInfo = uri.UserInfo.Split(':');
    var username = Uri.UnescapeDataString(userInfo[0]);
    var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";
    
    // NOTE: Use default PostgreSQL port (5432) if port is not specified in URL
    var port = uri.Port != -1 ? uri.Port : 5432;
    var database = uri.LocalPath.TrimStart('/');
    
    finalConnectionString = $"Host={uri.Host};Port={port};Database={database};Username={username};Password={password}";
    
    // NOTE: Add SSL mode for Render (required for production databases)
    if (!finalConnectionString.Contains("SslMode"))
    {
        finalConnectionString += ";SslMode=Require";
    }
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(finalConnectionString));

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

// NOTE: Register HttpClientFactory for email service
builder.Services.AddHttpClient();

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
