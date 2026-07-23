namespace ProtectBroker.Core.Entities;

/// <summary>
/// Represents an application user with role-based access control.
/// </summary>
public class User
{
    public Guid Id { get; set; }
    
    public string Email { get; set; } = string.Empty;
    
    public string DisplayName { get; set; } = string.Empty;
    
    public string NormalizedEmail { get; set; } = string.Empty;
    
    /// <summary>
    /// Hashed password (managed by ASP.NET Identity)
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;
    
    public UserRole Role { get; set; } = UserRole.Operator;
    
    public bool IsActive { get; set; } = true;
    
    public bool EmailConfirmed { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? LastLoginAt { get; set; }
    
    public DateTime? LastModifiedAt { get; set; }
    
    public int FailedLoginAttempts { get; set; } = 0;
    
    public DateTime? LockoutUntil { get; set; }
}

/// <summary>
/// User role enumeration with hierarchical permissions.
/// </summary>
public enum UserRole
{
    /// <summary>Full system access - manage users, settings, and all features</summary>
    Administrator = 0,
    
    /// <summary>Can control relays and manage automation rules</summary>
    Manager = 1,
    
    /// <summary>Can view sensors and relay status</summary>
    Operator = 2,
    
    /// <summary>Read-only access to dashboard and logs</summary>
    Guest = 3
}
