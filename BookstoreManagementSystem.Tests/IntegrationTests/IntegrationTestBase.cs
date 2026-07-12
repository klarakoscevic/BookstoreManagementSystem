using BookstoreManagementSystem.Data;
using BookstoreManagementSystem.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using BookstoreManagementSystem.DTOs.Auth;

namespace BookstoreManagementSystem.Tests.IntegrationTests;

// Custom DelegatingHandler to inject Authorization header into every request
public class AuthHeaderHandler : DelegatingHandler
{
    private readonly Func<string?> _getToken;

    public AuthHeaderHandler(Func<string?> getToken)
    {
        _getToken = getToken;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = _getToken();
        if (!string.IsNullOrEmpty(token) && request.Headers.Authorization == null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        return await base.SendAsync(request, cancellationToken);
    }
}

public class IntegrationTestBase : IAsyncLifetime
{
    protected readonly BookstoreWebApplicationFactory Factory;
    protected readonly HttpClient Client;
    protected string? AuthToken;
    private readonly AuthHeaderHandler _authHeaderHandler;

    public IntegrationTestBase()
    {
        Factory = new BookstoreWebApplicationFactory();
        _authHeaderHandler = new AuthHeaderHandler(() => AuthToken);
        // Create client with our custom auth handler
        Client = Factory.CreateDefaultClient(_authHeaderHandler);
    }

    public async Task InitializeAsync()
    {
        await SeedDatabase();
        await AuthenticateAsync();
    }

    public Task DisposeAsync()
    {
        Client.Dispose();
        Factory.Dispose();
        return Task.CompletedTask;
    }

    protected async Task SeedDatabase()
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BookstoreDbContext>();

        // Clear existing data
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        // Create roles
        var readRole = new Role { Id = 1, Name = "Read" };
        var readWriteRole = new Role { Id = 2, Name = "ReadWrite" };
        context.Roles.AddRange(readRole, readWriteRole);

        // Create test users
        var testUser = new User
        {
            Id = 1,
            Username = "testuser",
            PasswordHash = HashPassword("Test123!"),
            Email = "testuser@example.com",
            RoleId = readRole.Id,
            Role = readRole
        };

        var adminUser = new User
        {
            Id = 2,
            Username = "adminuser",
            PasswordHash = HashPassword("Admin123!"),
            Email = "admin@example.com",
            RoleId = readWriteRole.Id,
            Role = readWriteRole
        };

        context.Users.AddRange(testUser, adminUser);

        // Create test genres
        var fiction = new Genre { Id = 1, Name = "Fiction", IsActive = true };
        var sciFi = new Genre { Id = 2, Name = "Science Fiction", IsActive = true };
        var fantasy = new Genre { Id = 3, Name = "Fantasy", IsActive = true };
        context.Genres.AddRange(fiction, sciFi, fantasy);

        // Create test authors
        var author1 = new Author { Id = 1, Name = "J.K. Rowling", IsActive = true };
        var author2 = new Author { Id = 2, Name = "George Orwell", IsActive = true };
        var author3 = new Author { Id = 3, Name = "Isaac Asimov", IsActive = true };
        context.Authors.AddRange(author1, author2, author3);

        // Create test books
        var book1 = new Book
        {
            Id = 1,
            Title = "Harry Potter and the Philosopher's Stone",
            Price = 29.99m,
            IsActive = true,
            Authors = new List<Author> { author1 },
            Genres = new List<Genre> { fantasy, fiction }
        };

        var book2 = new Book
        {
            Id = 2,
            Title = "1984",
            Price = 19.99m,
            IsActive = true,
            Authors = new List<Author> { author2 },
            Genres = new List<Genre> { fiction }
        };

        var book3 = new Book
        {
            Id = 3,
            Title = "Foundation",
            Price = 24.99m,
            IsActive = true,
            Authors = new List<Author> { author3 },
            Genres = new List<Genre> { sciFi, fiction }
        };

        context.Books.AddRange(book1, book2, book3);

        // Create test reviews
        var review1 = new Review
        {
            Id = 1,
            BookId = 1,
            Rating = 5,
            Description = "Amazing book!",
            IsActive = true
        };

        var review2 = new Review
        {
            Id = 2,
            BookId = 2,
            Rating = 4,
            Description = "Thought-provoking",
            IsActive = true
        };

        context.Reviews.AddRange(review1, review2);

        await context.SaveChangesAsync();
    }

    protected async Task AuthenticateAsync(bool asAdmin = false)
    {
        var loginDto = new LoginDto
        {
            Username = asAdmin ? "adminuser" : "testuser",
            Password = asAdmin ? "Admin123!" : "Test123!"
        };

        var response = await Client.PostAsJsonAsync("/api/auth/login", loginDto);
        response.EnsureSuccessStatusCode();

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        AuthToken = authResponse?.Token;

        if (string.IsNullOrEmpty(AuthToken))
        {
            throw new InvalidOperationException("Failed to obtain authentication token");
        }
    }

    protected async Task<BookstoreDbContext> GetDbContextAsync()
    {
        var scope = Factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<BookstoreDbContext>();
    }

    protected void ClearAuthToken()
    {
        AuthToken = null;
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
}
