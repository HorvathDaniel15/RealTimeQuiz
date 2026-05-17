using System.Net;
using System.Net.Http.Json;
using RealTimeQuiz.Logic.Contracts.Auth.Requests;
using RealTimeQuiz.Logic.Contracts.Auth.Responses;

namespace RealTimeQuiz.Tests.Integration;

public class AuthIntegrationTests : BaseIntegrationTest
{
    public AuthIntegrationTests(RealTimeQuizWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Register_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "testuser_register@local.test",
            Password = "TestPassword123!",
            UserName = "testuser_reg"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<RegisterResult>();
        
        Assert.NotNull(result);
        Assert.Equal(request.Email, result.Email);
        Assert.NotEmpty(result.UserId);
    }

    [Fact]
    public async Task Login_WithValidData_ShouldReturnJwtToken()
    {
        // Arrange
        var registerRequest = new RegisterRequest
        {
            Email = "testuser_login@local.test",
            Password = "TestPassword123!",
            UserName = "testuser_log"
        };
        await Client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new LoginRequest
        {
            Email = "testuser_login@local.test",
            Password = "TestPassword123!"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<LoginResult>();

        Assert.NotNull(result);
        Assert.NotEmpty(result.AccessToken);
        Assert.NotEmpty(result.RefreshToken);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ShouldReturnBadRequest()
    {
        // Arrange
        var registerRequest = new RegisterRequest
        {
            Email = "testuser_fail@local.test",
            Password = "TestPassword123!",
            UserName = "testuser_fail"
        };
        await Client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new LoginRequest
        {
            Email = "testuser_fail@local.test",
            Password = "WrongPassword!00"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithExistingEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "duplicate@local.test",
            Password = "TestPassword123!",
            UserName = "duplicate_user"
        };

        // Act - First registration should succeed
        var firstResponse = await Client.PostAsJsonAsync("/api/auth/register", request);
        firstResponse.EnsureSuccessStatusCode();

        // Act - Second registration with the same email
        var secondResponse = await Client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, secondResponse.StatusCode);
    }

    [Fact]
    public async Task Register_WithWeakPassword_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "weakpass@local.test",
            Password = "123", // Weak password
            UserName = "weakpass_user"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithNonExistingUser_ShouldReturnForbidden()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "doesnotexist@local.test",
            Password = "TestPassword123!"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
