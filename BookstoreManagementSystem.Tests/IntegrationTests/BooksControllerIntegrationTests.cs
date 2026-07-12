using BookstoreManagementSystem.DTOs;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace BookstoreManagementSystem.Tests.IntegrationTests;

public class BooksControllerIntegrationTests : IntegrationTestBase
{
    [Fact]
    public async Task GetAllBooks_ShouldReturnAllBooks()
    {
        // Act
        var response = await Client.GetAsync("/api/books");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var books = await response.Content.ReadFromJsonAsync<List<BookDto>>();
        books.Should().NotBeNull();
        books.Should().HaveCount(3);
        books![0].Title.Should().Be("Harry Potter and the Philosopher's Stone");
    }

    [Fact]
    public async Task GetBook_WithValidId_ShouldReturnBook()
    {
        // Act
        var response = await Client.GetAsync("/api/books/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var book = await response.Content.ReadFromJsonAsync<BookDto>();
        book.Should().NotBeNull();
        book!.Id.Should().Be(1);
        book.Title.Should().Be("Harry Potter and the Philosopher's Stone");
        book.Price.Should().Be(29.99m);
    }

    [Fact]
    public async Task GetBook_WithInvalidId_ShouldReturnNotFound()
    {
        // Act
        var response = await Client.GetAsync("/api/books/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateBook_WithValidData_AsAdmin_ShouldCreateBook()
    {
        // Arrange
        await AuthenticateAsync(asAdmin: true);

        var createBookDto = new CreateBookDto
        {
            Title = "New Test Book",
            Price = 15.99m,
            AuthorIds = new List<int> { 1 },
            GenreIds = new List<int> { 1 }
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/books", createBookDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdBook = await response.Content.ReadFromJsonAsync<BookDto>();
        createdBook.Should().NotBeNull();
        createdBook!.Title.Should().Be("New Test Book");
        createdBook.Price.Should().Be(15.99m);

        // Verify location header
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateBook_WithValidData_AsReadUser_ShouldReturnForbidden()
    {
        // Arrange - authenticated as read-only user by default
        var createBookDto = new CreateBookDto
        {
            Title = "New Test Book",
            Price = 15.99m,
            AuthorIds = new List<int> { 1 },
            GenreIds = new List<int> { 1 }
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/books", createBookDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateBook_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        ClearAuthToken();

        var createBookDto = new CreateBookDto
        {
            Title = "New Test Book",
            Price = 15.99m,
            AuthorIds = new List<int> { 1 },
            GenreIds = new List<int> { 1 }
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/books", createBookDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateBookPrice_WithValidData_AsAdmin_ShouldUpdatePrice()
    {
        // Arrange
        await AuthenticateAsync(asAdmin: true);

        var updatePriceDto = new UpdateBookPriceDto
        {
            Price = 39.99m
        };

        // Act
        var response = await Client.PutAsJsonAsync("/api/books/1/price", updatePriceDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedBook = await response.Content.ReadFromJsonAsync<BookDto>();
        updatedBook.Should().NotBeNull();
        updatedBook!.Price.Should().Be(39.99m);
    }

    [Fact]
    public async Task DeleteBook_WithValidId_AsAdmin_ShouldSoftDeleteBook()
    {
        // Arrange
        await AuthenticateAsync(asAdmin: true);

        // Act
        var response = await Client.DeleteAsync("/api/books/3");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify book is soft deleted
        var getResponse = await Client.GetAsync("/api/books");
        var books = await getResponse.Content.ReadFromJsonAsync<List<BookDto>>();
        books.Should().NotContain(b => b.Id == 3);
    }

    [Fact]
    public async Task DeleteBook_AsReadUser_ShouldReturnForbidden()
    {
        // Act
        var response = await Client.DeleteAsync("/api/books/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
