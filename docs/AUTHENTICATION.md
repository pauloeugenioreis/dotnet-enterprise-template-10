# Authentication - JWT & OAuth2

## 📋 Índice

- [Overview](#overview)
- [Default Admin Credentials](#-default-admin-credentials)
- [Features](#features)
- [Architecture](#architecture)
- [Configuration](#configuration)
- [Quick Start](#-quick-start)
- [API Endpoints](#api-endpoints)
- [Usage Examples](#usage-examples)
- [Security Best Practices](#security-best-practices)
- [Database Schema](#database-schema)
- [OAuth2 Setup](#oauth2-setup)
- [Testing with Swagger](#testing-with-swagger)
- [Troubleshooting](#troubleshooting)
- [Migration from Basic Auth](#migration-from-basic-auth)
- [Performance Considerations](#performance-considerations)
- [Advanced Features (Future)](#advanced-features-future)
- [References](#references)
- [Support](#support)

---

## Overview

This template provides a comprehensive authentication system with JWT (JSON Web Tokens) and OAuth2 support. The implementation follows security best practices and is fully configurable via `appsettings.json`.

## 🔑 Default Admin Credentials

For development and testing, a default admin user is automatically created when seeding the database:

```text
Username: admin
Password: Admin@2026!Secure
Email:    admin@projecttemplate.com
Role:     Admin
```

> ⚠️ **IMPORTANT**: Change this password immediately in production environments!

To seed the database with the default admin user, run:
```bash
dotnet run --project src/Api
```sql
The seeder will automatically create:
- **Roles**: Admin, User, Manager
- **Default Admin User** with the credentials above

## Features

- ✅ **JWT Authentication** - Secure token-based authentication
- ✅ **Refresh Tokens** - Long-lived tokens for seamless token renewal
- ✅ **OAuth2 Providers** - Google, Microsoft, GitHub integration
- ✅ **Password Policy** - Configurable password requirements
- ✅ **Role-Based Authorization** - Assign roles to users
- ✅ **Token Revocation** - Logout and invalidate refresh tokens
- ✅ **IP Tracking** - Track login IPs for security auditing
- ✅ **Configurable** - Enable/disable via settings

## Architecture

```
┌─────────────────┐
│  AuthController │  ← REST API endpoints
└────────┬────────┘
         │
    ┌────▼────────┐
    │ AuthService │  ← Business logic
    └────┬────────┘
         │
    ┌────▼──────────────┐
    │ JwtTokenService   │  ← Token generation/validation
    └────┬──────────────┘
         │
    ┌────▼──────────────┐
    │ UserRepository    │  ← Data access
    └────┬──────────────┘
         │
    ┌────▼──────────────┐
    │ ApplicationDbContext │  ← EF Core
    └───────────────────┘
```markdown
## Configuration

### appsettings.json

```json
{
  "AppSettings": {
    "Authentication": {
      "Enabled": true,
      "JwtSettings": {
        "Secret": "your-256-bit-secret-key-change-this-in-production-needs-at-least-32-characters",
        "Issuer": "ProjectTemplate",
        "Audience": "ProjectTemplate",
        "ExpirationMinutes": 60,
        "RefreshTokenExpirationDays": 7,
        "ValidateIssuer": true,
        "ValidateAudience": true,
        "ValidateLifetime": true,
        "ValidateIssuerSigningKey": true
      },
      "OAuth2Settings": {
        "Enabled": false,
        "GoogleOAuthSettings": {
          "Enabled": false,
          "ClientId": "your-google-client-id",
          "ClientSecret": "your-google-client-secret"
        },
        "MicrosoftOAuthSettings": {
          "Enabled": false,
          "ClientId": "your-microsoft-client-id",
          "ClientSecret": "your-microsoft-client-secret",
          "TenantId": "common"
        },
        "GitHubOAuthSettings": {
          "Enabled": false,
          "ClientId": "your-github-client-id",
          "ClientSecret": "your-github-client-secret"
        }
      },
      "PasswordPolicySettings": {
        "MinimumLength": 8,
        "RequireDigit": true,
        "RequireLowercase": true,
        "RequireUppercase": true,
        "RequireNonAlphanumeric": true,
        "MaxFailedAccessAttempts": 5,
        "LockoutMinutes": 15
      },
      "RefreshTokenSettings": {
        "ExpirationDays": 7,
        "ReuseTokens": false
      }
    }
  }
}
```markdown
### Configuration Options

#### JwtSettings
- **Secret**: Secret key for signing JWTs (minimum 32 characters for HS256)
- **Issuer**: Token issuer (who created the token)
- **Audience**: Token audience (who can use the token)
- **ExpirationMinutes**: Access token lifetime (default: 60 minutes)
- **RefreshTokenExpirationDays**: Refresh token lifetime (default: 7 days)
- **Validate*** flags: Enable/disable JWT validation checks

#### PasswordPolicySettings
- **MinimumLength**: Minimum password length (default: 8)
- **RequireDigit**: Require at least one number
- **RequireLowercase**: Require at least one lowercase letter
- **RequireUppercase**: Require at least one uppercase letter
- **RequireNonAlphanumeric**: Require at least one special character
- **MaxFailedAccessAttempts**: Max login attempts before lockout
- **LockoutMinutes**: Account lockout duration

#### RefreshTokenSettings
- **ExpirationDays**: How long refresh tokens are valid
- **ReuseTokens**: Allow reusing the same refresh token (not recommended)

## 🚀 Quick Start

### 1. Run the Application

```bash
# Start the API
dotnet run --project src/Api

# API will be available at http://localhost:5000
# Swagger UI at http://localhost:5000/swagger
```markdown
### 2. Login with Admin Credentials

Use the default admin credentials to get a JWT token:

```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "usernameOrEmail": "admin",
    "password": "Admin@2026!Secure"
  }'
```text
**Response:**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "random-secure-token...",
  "expiresAt": "2026-01-14T15:00:00Z",
  "user": {
    "id": 1,
    "username": "admin",
    "email": "admin@projecttemplate.com",
    "firstName": "System",
    "lastName": "Administrator",
    "roles": ["Admin"]
  }
}
```markdown
### 3. Use the Access Token

Add the token to the `Authorization` header:

```bash
curl -X GET http://localhost:5000/api/auth/me \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```sql
### 4. Test in Swagger

1. Open http://localhost:5000/swagger
2. Click "Authorize" button (🔒)
3. Enter: `Bearer <your-access-token>`
4. Click "Authorize" and "Close"
5. All endpoints will now use your authentication

## API Endpoints

### 1. Register

Create a new user account.

**Endpoint:** `POST /api/auth/register`

**Request:**
```json
{
  "username": "john.doe",
  "email": "john.doe@example.com",
  "password": "P@ssw0rd123",
  "firstName": "John",
  "lastName": "Doe"
}
```text
**Response:**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "a1b2c3d4e5f6...",
  "expiresAt": "2024-01-01T13:00:00Z",
  "user": {
    "id": 1,
    "username": "john.doe",
    "email": "john.doe@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "roles": ["User"]
  }
}
```markdown
### 2. Login

Authenticate with username/email and password.

**Endpoint:** `POST /api/auth/login`

**Request:**
```json
{
  "usernameOrEmail": "john.doe",
  "password": "P@ssw0rd123"
}
```text
**Response:**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "a1b2c3d4e5f6...",
  "expiresAt": "2024-01-01T13:00:00Z",
  "user": {
    "id": 1,
    "username": "john.doe",
    "email": "john.doe@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "roles": ["User"]
  }
}
```csharp
### 3. Refresh Token

Get a new access token using a refresh token.

**Endpoint:** `POST /api/auth/refresh-token`

**Request:**
```json
{
  "refreshToken": "a1b2c3d4e5f6..."
}
```text
**Response:**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "g7h8i9j0k1l2...",
  "expiresAt": "2024-01-01T13:00:00Z",
  "user": {
    "id": 1,
    "username": "john.doe",
    "email": "john.doe@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "roles": ["User"]
  }
}
```markdown
### 4. Revoke Token (Logout)

Invalidate a refresh token.

**Endpoint:** `POST /api/auth/revoke-token`

**Request:**
```json
{
  "refreshToken": "a1b2c3d4e5f6..."
}
```markdown
**Response:** `204 No Content`

### 5. Get Current User

Get authenticated user information.

**Endpoint:** `GET /api/auth/me`

**Headers:**
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```text
**Response:**
```json
{
  "id": 1,
  "username": "john.doe",
  "email": "john.doe@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "roles": ["User"]
}
```markdown
### 6. Change Password

Change the current user's password.

**Endpoint:** `POST /api/auth/change-password`

**Headers:**
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```text
**Request:**
```json
{
  "currentPassword": "P@ssw0rd123",
  "newPassword": "NewP@ssw0rd456"
}
```sql
**Response:** `204 No Content`

### 7. Update Profile

Update user profile information.

**Endpoint:** `PUT /api/auth/profile`

**Headers:**
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```text
**Request:**
```json
{
  "firstName": "John",
  "lastName": "Smith",
  "phoneNumber": "+1234567890",
  "profileImageUrl": "https://example.com/avatar.jpg"
}
```text
**Response:**
```json
{
  "id": 1,
  "username": "john.doe",
  "email": "john.doe@example.com",
  "firstName": "John",
  "lastName": "Smith",
  "phoneNumber": "+1234567890",
  "profileImageUrl": "https://example.com/avatar.jpg",
  "roles": ["User"]
}
```markdown
### 8. OAuth2 Login (Coming Soon)

Login with external OAuth2 providers.

**Endpoint:** `POST /api/auth/oauth2/login`

**Request:**
```json
{
  "provider": "Google",
  "accessToken": "ya29.a0AfH6SMBx..."
}
```text
**Response:**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "a1b2c3d4e5f6...",
  "expiresAt": "2024-01-01T13:00:00Z",
  "user": {
    "id": 1,
    "username": "john.doe",
    "email": "john.doe@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "roles": ["User"]
  }
}
```markdown
## Usage Examples

### C# Client

```csharp
using System.Net.Http.Json;

var client = new HttpClient { BaseAddress = new Uri("http://localhost:5000") };

// Register
var registerDto = new
{
    Username = "john.doe",
    Email = "john.doe@example.com",
    Password = "P@ssw0rd123",
    FirstName = "John",
    LastName = "Doe"
};

var registerResponse = await client.PostAsJsonAsync("/api/auth/register", registerDto);
var authResponse = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();

// Login
var loginDto = new
{
    UsernameOrEmail = "john.doe",
    Password = "P@ssw0rd123"
};

var loginResponse = await client.PostAsJsonAsync("/api/auth/login", loginDto);
authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();

// Use access token
client.DefaultRequestHeaders.Authorization = 
    new AuthenticationHeaderValue("Bearer", authResponse.AccessToken);

// Get current user
var meResponse = await client.GetAsync("/api/auth/me");
var user = await meResponse.Content.ReadFromJsonAsync<UserDto>();

// Refresh token when expired
var refreshDto = new { RefreshToken = authResponse.RefreshToken };
var refreshResponse = await client.PostAsJsonAsync("/api/auth/refresh-token", refreshDto);
authResponse = await refreshResponse.Content.ReadFromJsonAsync<AuthResponse>();
```markdown
### JavaScript/TypeScript

```typescript
const API_URL = 'http://localhost:5000/api/auth';

// Register
async function register(username: string, email: string, password: string) {
  const response = await fetch(`${API_URL}/register`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      username,
      email,
      password,
      firstName: 'John',
      lastName: 'Doe'
    })
  });
  
  return await response.json();
}

// Login
async function login(usernameOrEmail: string, password: string) {
  const response = await fetch(`${API_URL}/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ usernameOrEmail, password })
  });
  
  const data = await response.json();
  localStorage.setItem('accessToken', data.accessToken);
  localStorage.setItem('refreshToken', data.refreshToken);
  return data;
}

// API call with authentication
async function getCurrentUser() {
  const token = localStorage.getItem('accessToken');
  
  const response = await fetch(`${API_URL}/me`, {
    headers: { 'Authorization': `Bearer ${token}` }
  });
  
  if (response.status === 401) {
    // Token expired, refresh it
    await refreshToken();
    return getCurrentUser(); // Retry
  }
  
  return await response.json();
}

// Refresh token
async function refreshToken() {
  const refreshToken = localStorage.getItem('refreshToken');
  
  const response = await fetch(`${API_URL}/refresh-token`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ refreshToken })
  });
  
  const data = await response.json();
  localStorage.setItem('accessToken', data.accessToken);
  localStorage.setItem('refreshToken', data.refreshToken);
}

// Logout
async function logout() {
  const refreshToken = localStorage.getItem('refreshToken');
  
  await fetch(`${API_URL}/revoke-token`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ refreshToken })
  });
  
  localStorage.removeItem('accessToken');
  localStorage.removeItem('refreshToken');
}
```markdown
### cURL

```bash
# Register
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "john.doe",
    "email": "john.doe@example.com",
    "password": "P@ssw0rd123",
    "firstName": "John",
    "lastName": "Doe"
  }'

# Login
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "usernameOrEmail": "john.doe",
    "password": "P@ssw0rd123"
  }'

# Get current user
curl -X GET http://localhost:5000/api/auth/me \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"

# Refresh token
curl -X POST http://localhost:5000/api/auth/refresh-token \
  -H "Content-Type: application/json" \
  -d '{
    "refreshToken": "YOUR_REFRESH_TOKEN"
  }'

# Logout
curl -X POST http://localhost:5000/api/auth/revoke-token \
  -H "Content-Type: application/json" \
  -d '{
    "refreshToken": "YOUR_REFRESH_TOKEN"
  }'
```markdown
## Security Best Practices

### 1. JWT Secret Key
- Use a strong, random secret key (minimum 32 characters for HS256)
- Store in environment variables or Azure Key Vault, **never commit to source control**
- Rotate keys periodically

```bash
# Generate a secure random key
openssl rand -base64 32
```markdown
### 2. HTTPS Only
- Always use HTTPS in production
- Set `RequireHttpsMetadata = true` in production

### 3. Token Storage
- **Client-side:**
  - Store access tokens in memory (variables)
  - Store refresh tokens in HttpOnly cookies (preferred) or secure storage
  - **Never store tokens in localStorage** (XSS vulnerable)
  
### 4. Token Expiration
- Keep access tokens short-lived (15-60 minutes)
- Use longer-lived refresh tokens (7-30 days)
- Implement token rotation (generate new refresh token on refresh)

### 5. Password Hashing
- Current implementation uses SHA256 (simplified)
- **Production recommendation:** Use BCrypt, Argon2, or PBKDF2
  
```csharp
// Example with BCrypt.Net
using BCrypt.Net;

// Hash password
var hashedPassword = BCrypt.HashPassword(password, workFactor: 12);

// Verify password
bool isValid = BCrypt.Verify(password, hashedPassword);
```markdown
### 6. Rate Limiting
- Enable rate limiting to prevent brute force attacks
- Limit login attempts per IP

### 7. Account Lockout
- Implement account lockout after failed login attempts
- Configure `MaxFailedAccessAttempts` and `LockoutMinutes`

### 8. Audit Logging
- Log authentication events (login, logout, failed attempts)
- Track IP addresses and user agents
- Monitor for suspicious activity

## Database Schema

### Users Table
```sql
CREATE TABLE Users (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) UNIQUE NOT NULL,
    Email NVARCHAR(100) UNIQUE NOT NULL,
    PasswordHash NVARCHAR(256) NOT NULL,
    FirstName NVARCHAR(50),
    LastName NVARCHAR(50),
    PhoneNumber NVARCHAR(20),
    IsActive BIT NOT NULL DEFAULT 1,
    EmailConfirmed BIT NOT NULL DEFAULT 0,
    TwoFactorEnabled BIT NOT NULL DEFAULT 0,
    LastLoginAt DATETIME2,
    ProfileImageUrl NVARCHAR(500),
    ExternalProvider NVARCHAR(50),
    ExternalId NVARCHAR(200),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2
);
```markdown
### Roles Table
```sql
CREATE TABLE Roles (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(50) UNIQUE NOT NULL,
    Description NVARCHAR(200),
    IsSystemRole BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);
```markdown
### UserRoles Table (Many-to-Many)
```sql
CREATE TABLE UserRoles (
    UserId BIGINT NOT NULL,
    RoleId BIGINT NOT NULL,
    AssignedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    PRIMARY KEY (UserId, RoleId),
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (RoleId) REFERENCES Roles(Id) ON DELETE CASCADE
);
```markdown
### RefreshTokens Table
```sql
CREATE TABLE RefreshTokens (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    UserId BIGINT NOT NULL,
    Token NVARCHAR(200) UNIQUE NOT NULL,
    ExpiresAt DATETIME2 NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedByIp NVARCHAR(50),
    IsRevoked BIT NOT NULL DEFAULT 0,
    RevokedAt DATETIME2,
    RevokedByIp NVARCHAR(50),
    ReplacedByToken NVARCHAR(200),
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);
```sql
## OAuth2 Setup

### Google OAuth2

1. **Create OAuth2 credentials:**
   - Go to [Google Cloud Console](https://console.cloud.google.com/)
   - Create a new project or select existing
   - Enable Google+ API
   - Create OAuth2 credentials (Web application)
   - Add authorized redirect URIs: `https://yourdomain.com/signin-google`

2. **Configure appsettings.json:**
```json
{
  "Authentication": {
    "OAuth2Settings": {
      "Enabled": true,
      "GoogleOAuthSettings": {
        "Enabled": true,
        "ClientId": "your-google-client-id.apps.googleusercontent.com",
        "ClientSecret": "your-google-client-secret"
      }
    }
  }
}
```markdown
### Microsoft OAuth2

1. **Register application:**
   - Go to [Azure Portal](https://portal.azure.com/)
   - Navigate to Azure Active Directory > App registrations
   - New registration
   - Add redirect URI: `https://yourdomain.com/signin-microsoft`

2. **Configure appsettings.json:**
```json
{
  "Authentication": {
    "OAuth2Settings": {
      "Enabled": true,
      "MicrosoftOAuthSettings": {
        "Enabled": true,
        "ClientId": "your-microsoft-client-id",
        "ClientSecret": "your-microsoft-client-secret",
        "TenantId": "common"
      }
    }
  }
}
```sql
### GitHub OAuth2

1. **Create OAuth App:**
   - Go to GitHub Settings > Developer settings > OAuth Apps
   - New OAuth App
   - Authorization callback URL: `https://yourdomain.com/signin-github`

2. **Configure appsettings.json:**
```json
{
  "Authentication": {
    "OAuth2Settings": {
      "Enabled": true,
      "GitHubOAuthSettings": {
        "Enabled": true,
        "ClientId": "your-github-client-id",
        "ClientSecret": "your-github-client-secret"
      }
    }
  }
}
```markdown
## Testing with Swagger

1. **Start the application:**
```bash
dotnet run --project src/Api/Api.csproj
```text
2. **Open Swagger UI:**
```
http://localhost:5000
```sql
3. **Register a new user:**
   - POST `/api/auth/register`
   - Copy the `accessToken` from response

4. **Authorize in Swagger:**
   - Click "Authorize" button (lock icon)
   - Enter: `Bearer YOUR_ACCESS_TOKEN`
   - Click "Authorize"

5. **Test authenticated endpoints:**
   - GET `/api/auth/me`
   - POST `/api/auth/change-password`
   - PUT `/api/auth/profile`

## Troubleshooting

### "Unauthorized" errors
- Check if token is expired
- Verify token format: `Bearer <token>`
- Check JWT secret matches in configuration

### "Invalid token" errors
- Verify issuer/audience match configuration
- Check token expiration time
- Ensure secret key is correct

### Password policy errors
- Check password meets all requirements
- Verify password policy settings in configuration

### Database errors
- Run migrations: `dotnet ef migrations add AddAuthentication`
- Update database: `dotnet ef database update`

## Migration from Basic Auth

If you're migrating from basic authentication:

1. **Create migration:**
```bash
dotnet ef migrations add AddAuthentication --project src/Data --startup-project src/Api
```sql
2. **Update database:**
```bash
dotnet ef database update --project src/Data --startup-project src/Api
```text
3. **Seed default roles:**
```csharp
// In DbSeeder.cs
if (!context.Roles.Any())
{
    context.Roles.AddRange(
        new Role { Name = "Admin", Description = "Administrator role", IsSystemRole = true },
        new Role { Name = "User", Description = "Default user role", IsSystemRole = true }
    );
    await context.SaveChangesAsync();
}
```markdown
## Performance Considerations

### Token Validation
- JWT validation is stateless and fast
- No database lookup required for access tokens
- Refresh tokens require database lookup

### Caching
- Cache user roles to reduce database queries
- Implement distributed cache for scalability

### Database Indexing
```sql
CREATE INDEX IX_Users_Username ON Users(Username);
CREATE INDEX IX_Users_Email ON Users(Email);
CREATE INDEX IX_RefreshTokens_Token ON RefreshTokens(Token);
CREATE INDEX IX_RefreshTokens_UserId ON RefreshTokens(UserId);
```

## Advanced Features (Future)

- [ ] Two-Factor Authentication (2FA)
- [ ] Email confirmation workflow
- [ ] Password reset via email
- [ ] Account lockout implementation
- [ ] OAuth2 external provider verification
- [ ] Token blacklisting
- [ ] Biometric authentication
- [ ] Single Sign-On (SSO)

## References

- [JWT.io](https://jwt.io/) - JWT specification
- [OAuth 2.0](https://oauth.net/2/) - OAuth2 specification
- [Microsoft Identity Platform](https://docs.microsoft.com/en-us/azure/active-directory/develop/)
- [OWASP Authentication Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html)

## Support

For issues or questions:
- GitHub Issues: [Create an issue](https://github.com/yourrepo/issues)
- Documentation: [Read the docs](https://github.com/yourrepo/docs)
