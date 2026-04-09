# Autenticação - JWT & OAuth2

## 📋 Índice

- [Visão Geral](#overview)
- [Credenciais Padrão do Admin](#default-admin-credentials)
- [Funcionalidades](#features)
- [Arquitetura](#architecture)
- [Configuração](#configuration)
- [Início Rápido](#quick-start)
- [Endpoints da API](#api-endpoints)
- [Exemplos de Uso](#usage-examples)
- [Boas Práticas de Segurança](#security-best-practices)
- [Schema do Banco de Dados](#database-schema)
- [Configuração do OAuth2](#oauth2-setup)
- [Testando com Swagger](#testing-with-swagger)
- [Solução de Problemas](#troubleshooting)
- [Migração do Basic Auth](#migration-from-basic-auth)
- [Considerações de Performance](#performance-considerations)
- [Funcionalidades Avançadas (Futuro)](#advanced-features-future)
- [Referências](#references)
- [Suporte](#support)

---

## Visão Geral

Este template oferece um sistema de autenticação completo com suporte a JWT (JSON Web Tokens) e OAuth2. A implementação segue as melhores práticas de segurança e é totalmente configurável via `appsettings.json`.

## Credenciais Padrão do Admin

Para desenvolvimento e testes, um usuário admin padrão é criado automaticamente ao popular o banco de dados:

```text
Username: admin
Password: Admin@2026!Secure
Email:    admin@projecttemplate.com
Role:     Admin
```

> ⚠️ **IMPORTANTE**: Altere esta senha imediatamente em ambientes de produção!

Para popular o banco com o usuário admin padrão, execute:

```bash
dotnet run --project src/Api
```

O seeder criará automaticamente:

- **Roles**: Admin, User, Manager
- **Usuário Admin Padrão** com as credenciais acima

<a id="features"></a>

## Funcionalidades

- ✅ **Autenticação JWT** - Autenticação segura baseada em tokens
- ✅ **Refresh Tokens** - Tokens de longa duração para renovação transparente
- ✅ **Provedores OAuth2** - Integração com Google, Microsoft e GitHub
- ✅ **Política de Senhas** - Requisitos de senha configuráveis
- ✅ **Autorização por Roles** - Atribuir roles aos usuários
- ✅ **Revogação de Token** - Logout e invalidação de refresh tokens
- ✅ **Rastreio de IP** - Registrar IPs de login para auditoria de segurança
- ✅ **Configurável** - Habilitar/desabilitar via configurações

<a id="architecture"></a>

## Arquitetura

```text
┌──────────────────────┐
│  AuthController      │
└──────────────────────┘
           │
           │  Endpoints REST da API
           ▼
┌──────────────────────┐
│  AuthService         │
└──────────────────────┘
           │
           │  Lógica de negócio
           ▼
┌──────────────────────┐
│  JwtTokenService     │
└──────────────────────┘
           │
           │  Geração/validação de tokens
           ▼
┌──────────────────────┐
│  UserRepository      │
└──────────────────────┘
           │
           │  Acesso a dados
           ▼
┌──────────────────────┐
│  ApplicationDbContext│
└──────────────────────┘
```

<a id="configuration"></a>

## Configuração

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
```

### Opções de Configuração

#### JwtSettings

- **Secret**: Chave secreta para assinar os JWTs (mínimo 32 caracteres para HS256)
- **Issuer**: Emissor do token (quem criou o token)
- **Audience**: Audiência do token (quem pode usar o token)
- **ExpirationMinutes**: Tempo de vida do access token (padrão: 60 minutos)
- **RefreshTokenExpirationDays**: Tempo de vida do refresh token (padrão: 7 dias)
- Flags **Validate\***: Habilitar/desabilitar validações do JWT

#### PasswordPolicySettings

- **MinimumLength**: Comprimento mínimo da senha (padrão: 8)
- **RequireDigit**: Exigir pelo menos um número
- **RequireLowercase**: Exigir pelo menos uma letra minúscula
- **RequireUppercase**: Exigir pelo menos uma letra maiúscula
- **RequireNonAlphanumeric**: Exigir pelo menos um caractere especial
- **MaxFailedAccessAttempts**: Máximo de tentativas de login antes do bloqueio
- **LockoutMinutes**: Duração do bloqueio da conta

#### RefreshTokenSettings

- **ExpirationDays**: Validade dos refresh tokens
- **ReuseTokens**: Permitir reutilização do mesmo refresh token (não recomendado)

<a id="quick-start"></a>

## Início Rápido

### 1. Executar a Aplicação

```bash
# Iniciar a API
dotnet run --project src/Api

# API disponível em http://localhost:5000
# Swagger UI em http://localhost:5000/swagger
```

### 2. Login com as Credenciais do Admin

Use as credenciais padrão do admin para obter um token JWT:

```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "usernameOrEmail": "admin",
    "password": "Admin@2026!Secure"
  }'
```

**Resposta:**

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
```

### 3. Utilizar o Access Token

Adicione o token ao cabeçalho `Authorization`:

```bash
curl -X GET http://localhost:5000/api/auth/me \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

### 4. Testar no Swagger

1. Abrir http://localhost:5000/swagger
2. Click "Authorize" button (🔒)
3. Enter: `Bearer <your-access-token>`
4. Click "Authorize" and "Close"
5. Todos os endpoints passarão a usar sua autenticação

<a id="api-endpoints"></a>

## Endpoints da API

### 1. Registro

Cria uma nova conta de usuário.

**Endpoint:** `POST /api/auth/register`

**Requisição:**

```json
{
  "username": "john.doe",
  "email": "john.doe@example.com",
  "password": "P@ssw0rd123",
  "firstName": "John",
  "lastName": "Doe"
}
```

**Resposta:**

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
```

### 2. Login

Autentica com usuário/e-mail e senha.

**Endpoint:** `POST /api/auth/login`

**Requisição:**

```json
{
  "usernameOrEmail": "john.doe",
  "password": "P@ssw0rd123"
}
```

**Resposta:**

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
```

### 3. Refresh Token

Obtém um novo access token usando um refresh token.

**Endpoint:** `POST /api/auth/refresh-token`

**Requisição:**

```json
{
  "refreshToken": "a1b2c3d4e5f6..."
}
```

**Resposta:**

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
```

### 4. Revogar Token (Logout)

Invalida um refresh token.

**Endpoint:** `POST /api/auth/revoke-token`

**Requisição:**

```json
{
  "refreshToken": "a1b2c3d4e5f6..."
}
```

**Resposta:** `204 No Content`

### 5. Obter Usuário Atual

Obtém as informações do usuário autenticado.

**Endpoint:** `GET /api/auth/me`

**Cabeçalhos:**

```text
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Resposta:**

```json
{
  "id": 1,
  "username": "john.doe",
  "email": "john.doe@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "roles": ["User"]
}
```

### 6. Alterar Senha

Altera a senha do usuário atual.

**Endpoint:** `POST /api/auth/change-password`

**Cabeçalhos:**

```bash
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Requisição:**

```json
{
  "currentPassword": "P@ssw0rd123",
  "newPassword": "NewP@ssw0rd456"
}
```

**Resposta:** `204 No Content`

### 7. Atualizar Perfil

Atualiza as informações do perfil do usuário.

**Endpoint:** `PUT /api/auth/profile`

**Cabeçalhos:**

```bash
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Requisição:**

```json
{
  "firstName": "John",
  "lastName": "Smith",
  "phoneNumber": "+1234567890",
  "profileImageUrl": "https://example.com/avatar.jpg"
}
```

**Resposta:**

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
```

### 8. Login OAuth2 (Em Breve)

Login com provedores OAuth2 externos.

**Endpoint:** `POST /api/auth/oauth2/login`

**Requisição:**

```json
{
  "provider": "Google",
  "accessToken": "ya29.a0AfH6SMBx..."
}
```

**Resposta:**

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
```

<a id="usage-examples"></a>

## Exemplos de Uso

### C# Client

```csharp
using System.Net.Http.Json;

var client = new HttpClient { BaseAddress = new Uri("http://localhost:5000") };

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

var loginDto = new
{
  UsernameOrEmail = "john.doe",
  Password = "P@ssw0rd123"
};

var loginResponse = await client.PostAsJsonAsync("/api/auth/login", loginDto);
authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();

client.DefaultRequestHeaders.Authorization =
  new AuthenticationHeaderValue("Bearer", authResponse.AccessToken);

var meResponse = await client.GetAsync("/api/auth/me");
var user = await meResponse.Content.ReadFromJsonAsync<UserDto>();

var refreshDto = new { RefreshToken = authResponse.RefreshToken };
var refreshResponse = await client.PostAsJsonAsync("/api/auth/refresh-token", refreshDto);
authResponse = await refreshResponse.Content.ReadFromJsonAsync<AuthResponse>();
```

### JavaScript/TypeScript

```typescript
const API_URL = 'http://localhost:5000/api/auth';

// Registro
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

// Chamada à API com autenticação
async function getCurrentUser() {
  const token = localStorage.getItem('accessToken');

  const response = await fetch(`${API_URL}/me`, {
    headers: { 'Authorization': `Bearer ${token}` }
  });

  if (response.status === 401) {
    // Token expirado, renovar
    await refreshToken();
    return getCurrentUser(); // Tentar novamente
  }

  return await response.json();
}

// Renovar token
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
```

### cURL

```bash
# Registro
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

# Obter usuário atual
curl -X GET http://localhost:5000/api/auth/me \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"

# Renovar token
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
```

<a id="security-best-practices"></a>

## Boas Práticas de Segurança

### 1. Chave Secreta JWT

- Use uma chave secreta forte e aleatória (mínimo 32 caracteres para HS256)
- Armazene em variáveis de ambiente ou Azure Key Vault, **nunca faça commit no controle de versão**
- Rotacione as chaves periodicamente

**Gerar uma chave aleatória segura:**

```bash
openssl rand -base64 32
```

### 2. Somente HTTPS

- Sempre use HTTPS em produção
- Defina `RequireHttpsMetadata = true` em produção

### 3. Armazenamento de Tokens

- **No cliente:**
  - Armazene access tokens em memória (variáveis)
  - Armazene refresh tokens em cookies HttpOnly (preferível) ou armazenamento seguro
  - **Nunca armazene tokens no localStorage** (vulnerável a XSS)

### 4. Expiração de Tokens

- Mantenha os access tokens com vida curta (15-60 minutos)
- Use refresh tokens com vida mais longa (7-30 dias)
- Implemente rotação de tokens (gere um novo refresh token a cada renovação)

### 5. Hash de Senhas

- A implementação atual usa SHA256 (simplificado)
- **Recomendação para produção:** Use BCrypt, Argon2 ou PBKDF2

```csharp
// Exemplo com BCrypt.Net
using BCrypt.Net;

// Gerar hash da senha
var hashedPassword = BCrypt.HashPassword(password, workFactor: 12);

// Verificar senha
bool isValid = BCrypt.Verify(password, hashedPassword);
```

### 6. Rate Limiting

- Habilite rate limiting para prevenir ataques de força bruta
- Limite tentativas de login por IP

### 7. Bloqueio de Conta

- Implemente bloqueio de conta após tentativas de login com falha
- Configure `MaxFailedAccessAttempts` e `LockoutMinutes`

### 8. Log de Auditoria

- Registre eventos de autenticação (login, logout, tentativas com falha)
- Rastreie endereços IP e user agents
- Monitore atividades suspeitas

<a id="database-schema"></a>

## Schema do Banco de Dados

### Tabela Users

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
```

### Tabela Roles

```sql
CREATE TABLE Roles (
  Id BIGINT IDENTITY(1,1) PRIMARY KEY,
  Name NVARCHAR(50) UNIQUE NOT NULL,
  Description NVARCHAR(200),
  IsSystemRole BIT NOT NULL DEFAULT 0,
  CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);
```

### Tabela UserRoles (Many-to-Many)

```sql
CREATE TABLE UserRoles (
  UserId BIGINT NOT NULL,
  RoleId BIGINT NOT NULL,
  AssignedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
  PRIMARY KEY (UserId, RoleId),
  FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
  FOREIGN KEY (RoleId) REFERENCES Roles(Id) ON DELETE CASCADE
);
```

### Tabela RefreshTokens

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
```

<a id="oauth2-setup"></a>

## Configuração do OAuth2

### Google OAuth2

1. **Criar credenciais OAuth2:**
   - Acesse o [Google Cloud Console](https://console.cloud.google.com/)
   - Crie um novo projeto ou selecione um existente
   - Enable Google+ API
   - Crie as credenciais OAuth2 (Web application)
   - Adicione as URIs de redirecionamento autorizadas: `https://yourdomain.com/signin-google`

2. **Configurar appsettings.json:**

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
   ```

### Microsoft OAuth2

1. **Registrar a aplicação:**
   - Acesse o [Azure Portal](https://portal.azure.com/)
   - Navegue até Azure Active Directory > Registros de aplicativo
   - Nova inscrição
   - Adicione a URI de redirecionamento: `https://yourdomain.com/signin-microsoft`

2. **Configurar appsettings.json:**

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
   ```

### GitHub OAuth2

1. **Criar OAuth App:**
   - Acesse GitHub Settings > Developer settings > OAuth Apps
   - New OAuth App
   - URL de callback de autorização: `https://yourdomain.com/signin-github`

2. **Configurar appsettings.json:**

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
   ```

<a id="testing-with-swagger"></a>

## Testando com Swagger

### Passo 1: Iniciar a aplicação

```bash
dotnet run --project src/Api/Api.csproj
```

### Passo 2: Abrir o Swagger UI

```text
http://localhost:5000
```

### Passo 3: Registrar um novo usuário

- POST `/api/auth/register`
- Copiar o `accessToken` da resposta

### Passo 4: Autorizar no Swagger

- Clicar no botão "Authorize" (ícone de cadeado)
- Enter: `Bearer YOUR_ACCESS_TOKEN`
- Clicar em "Authorize"

### Passo 5: Testar endpoints autenticados

- GET `/api/auth/me`
- POST `/api/auth/change-password`
- PUT `/api/auth/profile`

<a id="troubleshooting"></a>

## Solução de Problemas

### Erros "Unauthorized"

- Verifique se o token expirou
- Verifique o formato do token: `Bearer <token>`
- Verifique se o secret JWT corresponde à configuração

### Erros "Invalid token"

- Verifique se issuer/audience correspondem à configuração
- Verifique o tempo de expiração do token
- Certifique-se de que a chave secreta está correta

### Erros de política de senha

- Verifique se a senha atende a todos os requisitos
- Verifique as configurações de política de senha

### Erros de banco de dados

- Execute as migrations: `dotnet ef migrations add AddAuthentication`
- Atualize o banco de dados: `dotnet ef database update`

<a id="migration-from-basic-auth"></a>

## Migração do Basic Auth

Se você está migrando da autenticação básica:

1. **Criar a migration:**

    ```bash
    dotnet ef migrations add AddAuthentication --project src/Data --startup-project src/Api
    ```

2. **Atualizar o banco de dados:**

    ```bash
    dotnet ef database update --project src/Data --startup-project src/Api
    ```

3. **Popular as roles padrão:**

    ```csharp
    // Em DbSeeder.cs
    if (!context.Roles.Any())
    {
      context.Roles.AddRange(
        new Role { Name = "Admin", Description = "Papel de administrador", IsSystemRole = true },
        new Role { Name = "User", Description = "Papel padrão de usuário", IsSystemRole = true }
      );
      await context.SaveChangesAsync();
    }
    ```

<a id="performance-considerations"></a>

## Considerações de Performance

### Validação de Tokens

- A validação JWT é stateless e rápida
- Não é necessário consultar o banco de dados para access tokens
- Refresh tokens requerem consulta ao banco de dados

### Cache

- Faça cache das roles do usuário para reduzir consultas ao banco
- Implemente cache distribuído para escalabilidade

### Indexação do Banco de Dados

```sql
CREATE INDEX IX_Users_Username ON Users(Username);
CREATE INDEX IX_Users_Email ON Users(Email);
CREATE INDEX IX_RefreshTokens_Token ON RefreshTokens(Token);
CREATE INDEX IX_RefreshTokens_UserId ON RefreshTokens(UserId);
```

<a id="advanced-features-future"></a>

## Funcionalidades Avançadas (Futuro)

- [ ] Autenticação de Dois Fatores (2FA)
- [ ] Fluxo de confirmação de e-mail
- [ ] Redefinição de senha por e-mail
- [ ] Implementação de bloqueio de conta
- [ ] Verificação de provedor externo OAuth2
- [ ] Lista negra de tokens
- [ ] Autenticação biométrica
- [ ] Single Sign-On (SSO)

<a id="references"></a>

## Referências

- [JWT.io](https://jwt.io/) - Especificação JWT
- [OAuth 2.0](https://oauth.net/2/) - Especificação OAuth2
- [Microsoft Identity Platform](https://docs.microsoft.com/en-us/azure/active-directory/develop/)
- [OWASP Authentication Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html)

<a id="support"></a>

## Suporte

Para dúvidas ou problemas:

- GitHub Issues: [Abrir uma issue](https://github.com/yourrepo/issues)
- Documentação: [Ler a documentação](https://github.com/yourrepo/docs)
