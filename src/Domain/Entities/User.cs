namespace ProjectTemplate.Domain.Entities;

/// <summary>
/// User entity for authentication and authorization
/// </summary>
public class User : EntityBase
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public new bool IsActive { get; set; } = true;  // Override EntityBase.IsActive
    public bool EmailConfirmed { get; set; } = false;
    public bool TwoFactorEnabled { get; set; } = false;
    public DateTime? LastLoginAt { get; set; }
    public string? ProfileImageUrl { get; set; }
    
    // OAuth2 properties
    public string? ExternalProvider { get; set; } // "Google", "Microsoft", "GitHub"
    public string? ExternalId { get; set; }
    
    // Navigation properties
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    
    public string FullName => $"{FirstName} {LastName}".Trim();
}

/// <summary>
/// Role entity for authorization
/// </summary>
public class Role : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystemRole { get; set; } = false;
    
    // Navigation properties
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}

/// <summary>
/// Many-to-many relationship between User and Role
/// </summary>
public class UserRole
{
    public long UserId { get; set; }
    public User User { get; set; } = null!;
    
    public long RoleId { get; set; }
    public Role Role { get; set; } = null!;
    
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Refresh token for JWT token renewal
/// </summary>
public class RefreshToken : EntityBase
{
    public long UserId { get; set; }
    public User User { get; set; } = null!;
    
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; } = false;
    public DateTime? RevokedAt { get; set; }
    public string? RevokedByIp { get; set; }
    public string? ReplacedByToken { get; set; }
    public string CreatedByIp { get; set; } = string.Empty;
    
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public new bool IsActive => !IsRevoked && !IsExpired;  // Override EntityBase.IsActive
}
