using Microsoft.EntityFrameworkCore;
using UserManagement.Models;

namespace UserManagement.Data;

/// <summary>
/// IMPORTANT: Application database context for Entity Framework Core.
/// NOTE: Contains configuration for unique index on User.Email.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    
    public DbSet<User> Users { get; set; } = null!;
    
    /// <summary>
    /// IMPORTANT: Configure database model including unique index.
    /// NOTA BENE: The unique index on Email guarantees email uniqueness
    /// at the database level, independent of application code.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<User>(entity =>
        {
            // IMPORTANT: Create unique index on Email column
            // NOTE: This is NOT a primary key, it's a separate unique index
            // NOTA BENE: Database will reject duplicate emails automatically
            entity.HasIndex(u => u.Email)
                  .IsUnique()
                  .HasDatabaseName("IX_Users_Email_Unique");
            
            // NOTE: Configure string lengths for PostgreSQL
            entity.Property(u => u.Name).HasMaxLength(100);
            entity.Property(u => u.Email).HasMaxLength(255);
            entity.Property(u => u.PasswordHash).HasMaxLength(255);
            entity.Property(u => u.EmailConfirmationToken).HasMaxLength(100);
        });
    }
}



