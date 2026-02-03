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

Update `appsettings.Development.json` with your PostgreSQL connection:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=usermanagement;Username=postgres;Password=yourpassword"
  }
}
```

### 2. Configure Email (Optional)

For email confirmation, configure Gmail SMTP in `appsettings.json`:

```json
{
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": "587",
    "SmtpUser": "your.email@gmail.com",
    "SmtpPass": "your-app-password"
  }
}
```

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
Email__SmtpHost=smtp.gmail.com
Email__SmtpPort=587
Email__SmtpUser=your.email@gmail.com
Email__SmtpPass=your-app-password
ASPNETCORE_ENVIRONMENT=Production
```

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
  EmailService.cs - Sends confirmation emails via SMTP

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
3. ✅ Sorted by LastLoginTime
4. ✅ Checkbox selection with Select All
5. ✅ User status check before each request (single filter)
6. ✅ Bootstrap CSS framework
7. ✅ Email confirmation (async)
8. ✅ Any non-empty password accepted


