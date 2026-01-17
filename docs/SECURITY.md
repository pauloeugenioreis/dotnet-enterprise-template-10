# Security Best Practices

## 🔐 Default Credentials

### Changing Default Admin Password

The system creates a default admin user on first run with these credentials:

```text
Username: admin
Password: Admin@2026!Secure
Email:    admin@projecttemplate.com
```

**⚠️ CRITICAL: Change this password immediately in production!**

### Option 1: Via API (Recommended)

```bash
# 1. Login with default credentials
curl -X POST https://your-api.com/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "usernameOrEmail": "admin",
    "password": "Admin@2026!Secure"
  }'

# 2. Use the accessToken to change password
curl -X POST https://your-api.com/api/auth/change-password \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN" \
  -d '{
    "currentPassword": "Admin@2026!Secure",
    "newPassword": "YourNewStrongPassword123!",
    "confirmPassword": "YourNewStrongPassword123!"
  }'
```sql
### Option 2: Via Database

Update directly in the database (requires password hashing):

```sql
-- Generate hash for new password using C#:
-- using var sha256 = System.Security.Cryptography.SHA256.Create();
-- var bytes = System.Text.Encoding.UTF8.GetBytes("YourNewPassword");
-- var hash = sha256.ComputeHash(bytes);
-- var hashString = Convert.ToBase64String(hash);

UPDATE Users 
SET PasswordHash = 'YOUR_NEW_PASSWORD_HASH_HERE'
WHERE Username = 'admin';
```markdown
### Option 3: Modify DbSeeder

Edit `src/Data/Seeders/DbSeeder.cs` before first deployment:

```csharp
private async Task SeedRolesAndAdminUserAsync()
{
    // ...
    
    // Change this password before deploying to production
    var passwordHash = HashPassword("YourProductionPassword123!");
    
    var adminUser = new User
    {
        Username = "admin",
        Email = "admin@yourcompany.com", // Change email too
        PasswordHash = passwordHash,
        // ...
    };
    
    // ...
}
```markdown
## 🛡️ JWT Secret Key

### Change JWT Secret in Production

The default JWT secret in `appsettings.json` is for development only:

```json
{
  "AppSettings": {
    "Authentication": {
      "JwtSettings": {
        "Secret": "your-256-bit-secret-key-change-this-in-production-needs-at-least-32-characters"
      }
    }
  }
}
```text
**Generate a secure random secret:**

```bash
# Linux/macOS
openssl rand -base64 64

# Windows PowerShell
[Convert]::ToBase64String((1..64 | ForEach-Object { Get-Random -Minimum 0 -Maximum 256 }))

# C#
using System.Security.Cryptography;
var bytes = new byte[64];
RandomNumberGenerator.Fill(bytes);
var secret = Convert.ToBase64String(bytes);
```markdown
### Use Environment Variables (Best Practice)

Instead of hardcoding secrets in `appsettings.json`, use environment variables:

```json
{
  "AppSettings": {
    "Authentication": {
      "JwtSettings": {
        "Secret": "${JWT_SECRET}"
      }
    }
  }
}
```text
Set environment variable:

```bash
# Linux/macOS
export JWT_SECRET="your-generated-secret-here"

# Windows PowerShell
$env:JWT_SECRET="your-generated-secret-here"

# Docker
docker run -e JWT_SECRET="your-generated-secret-here" your-image

# Kubernetes Secret
kubectl create secret generic jwt-secret --from-literal=JWT_SECRET='your-generated-secret-here'
```markdown
## 🔒 Connection Strings

### Secure Database Credentials

Never commit real database credentials to version control.

**Bad:**
```json
{
  "Infrastructure": {
    "Database": {
      "ConnectionString": "Server=prod-server;Database=MyDb;User Id=sa;Password=RealPassword123!;"
    }
  }
}
```text
**Good (Environment Variables):**
```json
{
  "Infrastructure": {
    "Database": {
      "ConnectionString": "${DB_CONNECTION_STRING}"
    }
  }
}
```

```bash
export DB_CONNECTION_STRING="Server=prod-server;Database=MyDb;User Id=sa;Password=RealPassword123!;"
```text
**Better (Azure Key Vault / AWS Secrets Manager):**

```csharp
// Program.cs
builder.Configuration.AddAzureKeyVault(
    new Uri(builder.Configuration["KeyVault:Endpoint"]!),
    new DefaultAzureCredential());
```markdown
## 📝 Security Checklist

Before deploying to production:

- [ ] Change default admin password
- [ ] Generate new JWT secret (min 256 bits)
- [ ] Store secrets in environment variables or secret manager
- [ ] Enable HTTPS only (`RequireHttpsMetadata: true`)
- [ ] Set strong password policy
- [ ] Enable two-factor authentication (if implemented)
- [ ] Configure CORS properly (don't use `AllowAnyOrigin` in production)
- [ ] Enable rate limiting
- [ ] Review and configure lockout policy
- [ ] Enable SQL Server encryption (TDE)
- [ ] Use encrypted connections (`Encrypt=True;TrustServerCertificate=False`)
- [ ] Rotate refresh tokens regularly
- [ ] Set appropriate token expiration times
- [ ] Enable audit logging
- [ ] Review OAuth2 redirect URIs
- [ ] Implement IP whitelisting if needed
- [ ] Configure proper CORS origins
- [ ] Disable detailed error messages in production
- [ ] Set up security headers (HSTS, CSP, X-Frame-Options, etc.)

## 🔍 Audit Logging

The system uses Event Sourcing with Marten to track all important events:

```csharp
// All authentication events are automatically logged:
// - User login attempts (success/failure)
// - Password changes
// - Token refreshes
// - Token revocations
// - Role assignments

// Query audit logs:
GET /api/audit/events?userId=1&eventType=UserLoggedIn&startDate=2026-01-01
```markdown
## 🚨 Security Monitoring

### Failed Login Attempts

Monitor for brute force attacks:

```sql
SELECT UserId, COUNT(*) as FailedAttempts, MAX(Timestamp) as LastAttempt
FROM DomainEvents
WHERE EventType = 'UserLoginFailed'
  AND Timestamp > DATEADD(hour, -1, GETUTCDATE())
GROUP BY UserId
HAVING COUNT(*) > 5;
```markdown
### Token Anomalies

Monitor for unusual token usage:

```sql
-- Multiple IPs using same refresh token
SELECT Token, COUNT(DISTINCT CreatedByIp) as IpCount
FROM RefreshTokens
WHERE CreatedAt > DATEADD(day, -7, GETUTCDATE())
GROUP BY Token
HAVING COUNT(DISTINCT CreatedByIp) > 3;
```

## 📚 Additional Resources

- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [OWASP API Security Top 10](https://owasp.org/www-project-api-security/)
- [JWT Best Practices](https://tools.ietf.org/html/rfc8725)
- [Microsoft Security Best Practices](https://docs.microsoft.com/en-us/aspnet/core/security/)
- [NIST Password Guidelines](https://pages.nist.gov/800-63-3/sp800-63b.html)
