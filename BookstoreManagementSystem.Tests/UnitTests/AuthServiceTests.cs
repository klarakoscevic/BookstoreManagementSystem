using BookstoreManagementSystem.Data;
using BookstoreManagementSystem.DTOs.Auth;
using BookstoreManagementSystem.Models;
using BookstoreManagementSystem.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace BookstoreManagementSystem.Tests.UnitTests;

public class AuthServiceTests
{
    private readonly DbContextOptions<BookstoreDbContext> _dbContextOptions;
    private readonly IConfiguration _configuration;

    public AuthServiceTests()
    {
        _dbContextOptions = new DbContextOptionsBuilder<BookstoreDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        // Setup configuration for JWT settings
        var inMemorySettings = new Dictionary<string, string>
        {
            {"JwtSettings:SecretKey", "TestSecretKeyForUnitTestingPurposesOnly123456789"},
            {"JwtSettings:Issuer", "TestIssuer"},
            {"JwtSettings:Audience", "TestAudience"},
            {"JwtSettings:ExpirationHours", "1"}
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();
    }

    private BookstoreDbContext CreateContext()
    {
        var context = new BookstoreDbContext(_dbContextOptions);

        // Seed roles if not already present
        if (!context.Roles.Any())
        {
            context.Roles.AddRange(
                new Role { Id = 1, Name = "Read", Description = "Can access GET endpoints only", IsActive = true },
                new Role { Id = 2, Name = "ReadWrite", Description = "Can access all endpoints", IsActive = true }
            );
            context.SaveChanges();
        }

        return context;
    }

    [Fact]
    public async Task RegisterAsync_WithValidData_ShouldCreateNewUser()
    {
        // Arrange
        using var context = CreateContext();
        var authService = new AuthService(context, _configuration);
        var registerDto = new RegisterDto
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Password123"
        };

        // Act
        var result = await authService.RegisterAsync(registerDto);

        // Assert
        result.Should().NotBeNull();
        result!.Username.Should().Be("testuser");
        result.Email.Should().Be("test@example.com");
        result.Role.Should().Be("Read"); // Default role
        result.Token.Should().NotBeNullOrEmpty();

        var userInDb = await context.Users.FirstOrDefaultAsync(u => u.Username == "testuser");
        userInDb.Should().NotBeNull();
        userInDb!.Email.Should().Be("test@example.com");
        userInDb.RoleId.Should().Be(1); // Read role ID
    }

    [Fact]
    public async Task RegisterAsync_WithDuplicateUsername_ShouldReturnNull()
    {
        // Arrange
        using var context = CreateContext();
        var readRole = await context.Roles.FirstAsync(r => r.Name == "Read");

        context.Users.Add(new User
        {
            Username = "existinguser",
            Email = "existing@example.com",
            PasswordHash = "hash",
            RoleId = readRole.Id,
            IsActive = true
        });
        await context.SaveChangesAsync();

        var authService = new AuthService(context, _configuration);
        var registerDto = new RegisterDto
        {
            Username = "existinguser",
            Email = "newemail@example.com",
            Password = "Password123"
        };

        // Act
        var result = await authService.RegisterAsync(registerDto);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task RegisterAsync_WithDuplicateEmail_ShouldReturnNull()
    {
        // Arrange
        using var context = CreateContext();
        var readRole = await context.Roles.FirstAsync(r => r.Name == "Read");

        context.Users.Add(new User
        {
            Username = "existinguser",
            Email = "existing@example.com",
            PasswordHash = "hash",
            RoleId = readRole.Id,
            IsActive = true
        });
        await context.SaveChangesAsync();

        var authService = new AuthService(context, _configuration);
        var registerDto = new RegisterDto
        {
            Username = "newuser",
            Email = "existing@example.com",
            Password = "Password123"
        };

        // Act
        var result = await authService.RegisterAsync(registerDto);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnAuthResponse()
    {
        // Arrange
        using var context = CreateContext();
        var authService = new AuthService(context, _configuration);

        // First register a user
        var registerDto = new RegisterDto
        {
            Username = "loginuser",
            Email = "login@example.com",
            Password = "Password123"
        };
        await authService.RegisterAsync(registerDto);

        var loginDto = new LoginDto
        {
            Username = "loginuser",
            Password = "Password123"
        };

        // Act
        var result = await authService.LoginAsync(loginDto);

        // Assert
        result.Should().NotBeNull();
        result!.Username.Should().Be("loginuser");
        result.Email.Should().Be("login@example.com");
        result.Role.Should().Be("Read");
        result.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task LoginAsync_WithInvalidUsername_ShouldReturnNull()
    {
        // Arrange
        using var context = CreateContext();
        var authService = new AuthService(context, _configuration);
        var loginDto = new LoginDto
        {
            Username = "nonexistentuser",
            Password = "Password123"
        };

        // Act
        var result = await authService.LoginAsync(loginDto);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ShouldReturnNull()
    {
        // Arrange
        using var context = CreateContext();
        var authService = new AuthService(context, _configuration);

        // First register a user
        var registerDto = new RegisterDto
        {
            Username = "loginuser",
            Email = "login@example.com",
            Password = "Password123"
        };
        await authService.RegisterAsync(registerDto);

        var loginDto = new LoginDto
        {
            Username = "loginuser",
            Password = "WrongPassword"
        };

        // Act
        var result = await authService.LoginAsync(loginDto);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_WithInactiveUser_ShouldReturnNull()
    {
        // Arrange
        using var context = CreateContext();
        var authService = new AuthService(context, _configuration);

        // First register a user, then deactivate them
        var registerDto = new RegisterDto
        {
            Username = "inactiveuser",
            Email = "inactive@example.com",
            Password = "Password123"
        };
        await authService.RegisterAsync(registerDto);

        // Deactivate the user
        var user = await context.Users.FirstAsync(u => u.Username == "inactiveuser");
        user.IsActive = false;
        await context.SaveChangesAsync();

        var loginDto = new LoginDto
        {
            Username = "inactiveuser",
            Password = "Password123"
        };

        // Act
        var result = await authService.LoginAsync(loginDto);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task RegisterAsync_ShouldAssignReadRoleByDefault()
    {
        // Arrange
        using var context = CreateContext();
        var authService = new AuthService(context, _configuration);
        var registerDto = new RegisterDto
        {
            Username = "roleuser",
            Email = "role@example.com",
            Password = "Password123"
        };

        // Act
        var result = await authService.RegisterAsync(registerDto);

        // Assert
        result.Should().NotBeNull();
        result!.Role.Should().Be("Read");

        var userInDb = await context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Username == "roleuser");
        userInDb.Should().NotBeNull();
        userInDb!.Role.Name.Should().Be("Read");
    }

    [Fact]
    public async Task RegisterAsync_ShouldHashPassword()
    {
        // Arrange
        using var context = CreateContext();
        var authService = new AuthService(context, _configuration);
        var registerDto = new RegisterDto
        {
            Username = "hashuser",
            Email = "hash@example.com",
            Password = "Password123"
        };

        // Act
        var result = await authService.RegisterAsync(registerDto);

        // Assert
        result.Should().NotBeNull();

        var userInDb = await context.Users.FirstOrDefaultAsync(u => u.Username == "hashuser");
        userInDb.Should().NotBeNull();
        userInDb!.PasswordHash.Should().NotBe("Password123"); // Password should be hashed
        userInDb.PasswordHash.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task PasswordHashing_ShouldBeConsistent()
    {
        // Arrange
        using var context = CreateContext();
        var authService = new AuthService(context, _configuration);
        var password = "TestPassword123";

        var registerDto1 = new RegisterDto
        {
            Username = "user1",
            Email = "user1@example.com",
            Password = password
        };

        var registerDto2 = new RegisterDto
        {
            Username = "user2",
            Email = "user2@example.com",
            Password = password
        };

        // Act
        await authService.RegisterAsync(registerDto1);
        await authService.RegisterAsync(registerDto2);

        // Assert - Same password should produce same hash
        var user1 = await context.Users.FirstOrDefaultAsync(u => u.Username == "user1");
        var user2 = await context.Users.FirstOrDefaultAsync(u => u.Username == "user2");

        user1.Should().NotBeNull();
        user2.Should().NotBeNull();
        user1!.PasswordHash.Should().Be(user2!.PasswordHash);
    }

    [Fact]
    public async Task PasswordVerification_ShouldWorkCorrectly()
    {
        // Arrange
        using var context = CreateContext();
        var authService = new AuthService(context, _configuration);

        var registerDto = new RegisterDto
        {
            Username = "verifyuser",
            Email = "verify@example.com",
            Password = "CorrectPassword123"
        };
        await authService.RegisterAsync(registerDto);

        var correctLoginDto = new LoginDto
        {
            Username = "verifyuser",
            Password = "CorrectPassword123"
        };

        var incorrectLoginDto = new LoginDto
        {
            Username = "verifyuser",
            Password = "WrongPassword123"
        };

        // Act
        var correctResult = await authService.LoginAsync(correctLoginDto);
        var incorrectResult = await authService.LoginAsync(incorrectLoginDto);

        // Assert
        correctResult.Should().NotBeNull(); // Correct password should succeed
        incorrectResult.Should().BeNull(); // Incorrect password should fail
    }
}
