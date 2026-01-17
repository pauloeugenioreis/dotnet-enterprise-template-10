# Authentication Unit Tests - TODO

## Status

Os testes unitários para autenticação precisam ser implementados.

## Testes Necessários

### 1. JwtTokenServiceTests
- [ ] GenerateAccessToken_WithValidUser_ShouldReturnValidToken
- [ ] GenerateRefreshToken_WithIpAddress_ShouldReturnToken
- [ ] ValidateToken_WithValidToken_ShouldReturnPrincipal
- [ ] ValidateToken_WithInvalidToken_ShouldReturnNull
- [ ] ValidateToken_WithExpiredToken_ShouldReturnNull

### 2. AuthServiceTests
- [ ] RegisterAsync_WithValidData_ShouldCreateUser
- [ ] RegisterAsync_WithExistingUsername_ShouldThrowException
- [ ] RegisterAsync_WithWeakPassword_ShouldThrowException
- [ ] LoginAsync_WithValidCredentials_ShouldReturnToken
- [ ] LoginAsync_WithInvalidCredentials_ShouldThrowException
- [ ] ChangePasswordAsync_WithValidData_ShouldUpdatePassword
- [ ] RefreshTokenAsync_WithValidToken_ShouldReturnNewToken
- [ ] RevokeTokenAsync_ShouldRevokeToken

### 3. UserRepositoryTests (Integration Tests)
- [ ] CreateAsync_ShouldAddUserToDatabase
- [ ] GetByIdAsync_WithExistingId_ShouldReturnUser
- [ ] GetByUsernameAsync_ShouldReturnUser
- [ ] GetByEmailAsync_ShouldReturnUser
- [ ] AddRoleToUserAsync_ShouldAssignRole
- [ ] GetUserRolesAsync_ShouldReturnRoles
- [ ] CreateRefreshTokenAsync_ShouldAddToken
- [ ] RevokeRefreshTokenAsync_ShouldMarkAsRevoked

## Como Implementar

Os testes devem usar:
- **xUnit** como framework de testes
- **FluentAssertions** para assertions
- **Moq** para mocking de dependências
- **InMemory Database** para testes de repositório

## Exemplo de Estrutura

public class JwtTokenServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly JwtTokenService _tokenService;

    public JwtTokenServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        var authSettings = new AuthenticationSettings { /* config */ };
        _tokenService = new JwtTokenService(authSettings, _userRepositoryMock.Object);
    }

    [Fact]
    public void GenerateAccessToken_WithValidUser_ShouldReturnToken()
    {
        // Arrange
        var user = new User { /* user data */ };

        // Act
        var token = _tokenService.GenerateAccessToken(user);

        // Assert
        token.Should().NotBeNullOrEmpty();
    }
}
```

## Referências

- [xUnit Documentation](https://xunit.net/)
- [FluentAssertions Documentation](https://fluentassertions.com/)
- [Moq Documentation](https://github.com/moq/moq4)
- [EF Core In-Memory Database](https://docs.microsoft.com/en-us/ef/core/providers/in-memory/)
