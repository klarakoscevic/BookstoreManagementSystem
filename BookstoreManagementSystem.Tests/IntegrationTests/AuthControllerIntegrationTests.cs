using BookstoreManagementSystem.DTOs.Auth;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace BookstoreManagementSystem.Tests.IntegrationTests;

public class AuthControllerIntegrationTests : IntegrationTestBase
{
    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnToken()
    {
        // Arrange
        ClearAuthToken(); // Start without authentication

        var loginDto = new LoginDto
        {
            Username = "testuser",
            Password = "Test123!"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login", loginDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        authResponse.Should().NotBeNull();
        authResponse!.Token.Should().NotBeNullOrEmpty();
        authResponse.Username.Should().Be("testuser");
        authResponse.Role.Should().Be("Read");
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ShouldReturnUnauthorized()
    {
        // Arrange
        ClearAuthToken();

        var loginDto = new LoginDto
        {
            Username = "testuser",
            Password = "WrongPassword123!"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login", loginDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithNonExistentUser_ShouldReturnUnauthorized()
    {
        // Arrange
        ClearAuthToken();

        var loginDto = new LoginDto
        {
            Username = "nonexistentuser",
            Password = "Password123!"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login", loginDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Register_WithValidData_ShouldCreateNewUser()
    {
        // Arrange
        ClearAuthToken();

        var registerDto = new RegisterDto
        {
            Username = "newuser",
            Password = "NewUser123!",
            Email = "newuser@example.com"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/register", registerDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        authResponse.Should().NotBeNull();
        authResponse!.Token.Should().NotBeNullOrEmpty();
        authResponse.Username.Should().Be("newuser");
        authResponse.Role.Should().Be("Read"); // Default role
    }

    [Fact]
    public async Task Register_WithExistingUsername_ShouldReturnBadRequest()
    {
        // Arrange
        ClearAuthToken();

        var registerDto = new RegisterDto
        {
            Username = "testuser", // Already exists
            Password = "NewUser123!",
            Email = "another@example.com"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/register", registerDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithExistingEmail_ShouldReturnBadRequest()
    {
        // Arrange
        ClearAuthToken();

        var registerDto = new RegisterDto
        {
            Username = "anotheruser",
            Password = "NewUser123!",
            Email = "testuser@example.com" // Already exists
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/register", registerDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AuthenticatedRequest_WithValidToken_ShouldSucceed()
    {
        // Arrange - Already authenticated in base class initialization

        // Act
        var response = await Client.GetAsync("/api/books");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AuthenticatedRequest_WithoutToken_ShouldReturnUnauthorized()
    {
        // Arrange
        ClearAuthToken();

        // Act
        var response = await Client.GetAsync("/api/books");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
