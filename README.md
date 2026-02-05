# User Management Application

ASP.NET Core MVC application for user registration, authentication, and management.

## Features

- User registration with email confirmation
- Cookie-based authentication
- User management panel (Block, Unblock, Delete)
- PostgreSQL database with unique email index
- Bootstrap 5 responsive UI

## Requirements

- .NET 8.0 SDK
- PostgreSQL database

## Local Development

### 1. Configure Database

**Important**: The repository includes example configuration files (`appsettings.example.json` and `appsettings.Development.example.json`). Copy them and fill in your actual values:

```bash
cp appsettings.example.json appsettings.json
cp appsettings.Development.example.json appsettings.Development.json
```

Then update `appsettings.Development.json` with your PostgreSQL connection:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=usermanagement;Username=postgres;Password=yourpassword"
  }
}
```

### 2. Configure Email (Optional)

For email confirmation, configure Resend API in `appsettings.json`:

```json
{
  "Email": {
    "ResendApiKey": "your-resend-api-key",
    "FromEmail": "onboarding@resend.dev",
    "FromName": "User Management App"
  }
}
```

**Note**: The application uses Resend API instead of SMTP because many hosting providers (like Render) block SMTP ports. Resend API works via HTTPS and doesn't require open SMTP ports.

To get a Resend API key:
1. Sign up at [resend.com](https://resend.com)
2. Create an API key in your dashboard
3. Use the API key in configuration

### 3. Run Migrations

```bash
dotnet ef database update
```

### 4. Run Application

```bash
dotnet run
```

Open https://localhost:5001 or http://localhost:5000

## Deployment on Render

### 1. Create PostgreSQL Database

1. Go to Render Dashboard
2. New → PostgreSQL
3. Copy the "Internal Database URL"

### 2. Create Web Service

1. New → Web Service
2. Connect your GitHub repository
3. Configure:
   - **Environment**: Docker
   - **Branch**: main

### 3. Set Environment Variables

```
ConnectionStrings__DefaultConnection=<your-postgres-internal-url>
Email__ResendApiKey=<your-resend-api-key>
Email__FromEmail=onboarding@resend.dev
Email__FromName=User Management App
ASPNETCORE_ENVIRONMENT=Production
```

**Important**: Use double underscore `__` between `Email` and `ResendApiKey` in Environment Variables.

**Note**: The connection string from Render PostgreSQL may be in URL format (`postgresql://...`). The application automatically converts it to standard Npgsql format if needed.

### 4. Deploy

Render will automatically build and deploy your application.

## Database Schema

The application creates a `Users` table with:

- `Id` (Primary Key)
- `Name`
- `Email` (Unique Index - IX_Users_Email_Unique)
- `PasswordHash`
- `Status` (Unverified, Active, Blocked)
- `LastLoginTime`
- `RegistrationTime`
- `EmailConfirmationToken`

**IMPORTANT**: Email uniqueness is enforced at the database level via unique index, not application code.

## Architecture

```
/Controllers
  AccountController.cs  - Login, Register, Logout, ConfirmEmail
  UsersController.cs    - User management (Index, Block, Unblock, Delete)

/Filters
  CheckUserStatusAttribute.cs - Checks user status before each request

/Services
  EmailService.cs - Sends confirmation emails via Resend API
  IEmailService.cs - Email service interface

/Models
  User.cs - User entity
  ViewModels/ - Login and Register view models

/Views
  Account/ - Login and Register forms
  Users/ - User management table
```

## Key Requirements Met

1. ✅ Unique index on Email (database-level)
2. ✅ Table with toolbar (no row buttons)
3. ✅ Sorted by LastLoginTime (default), with column sorting support
4. ✅ Checkbox selection with Select All
5. ✅ User status check before each request (single filter)
6. ✅ Bootstrap CSS framework
7. ✅ Email confirmation (async via Resend API)
8. ✅ Any non-empty password accepted
9. ✅ Real-time client-side filtering by Name and Email
10. ✅ Blocked users styling (grayed out, struck through)
11. ✅ Delete unverified users functionality

## Live Deployment

The application is deployed on Render:
- **URL**: https://user-management-app-je14.onrender.com
- **Database**: PostgreSQL on Render
- **Email Service**: Resend API

## Technology Stack

- **Backend**: ASP.NET Core 8.0 MVC
- **Database**: PostgreSQL with Entity Framework Core
- **Authentication**: Cookie-based authentication
- **Email**: Resend API (REST API, works on Render)
- **Frontend**: Razor Views with Bootstrap 5
- **Password Hashing**: BCrypt.Net-Next


