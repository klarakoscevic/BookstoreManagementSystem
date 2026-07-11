using BookstoreManagementSystem.DTOs;
using BookstoreManagementSystem.Models;
using BookstoreManagementSystem.Repositories;
using BookstoreManagementSystem.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace BookstoreManagementSystem.Tests.UnitTests;

public class BookServiceTests
{
    private readonly Mock<IBookRepository> _mockBookRepository;
    private readonly Mock<ILogger<BookService>> _mockLogger;
    private readonly BookService _bookService;

    public BookServiceTests()
    {
        _mockBookRepository = new Mock<IBookRepository>();
        _mockLogger = new Mock<ILogger<BookService>>();
        _bookService = new BookService(_mockBookRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAllBooksAsync_ShouldReturnBookDtos()
    {
        // Arrange
        var books = new List<Book>
        {
            new()
            {
                Id = 1,
                Title = "Test Book",
                Price = 19.99m,
                Authors = new List<Author> { new() { Id = 1, Name = "Test Author" } },
                Genres = new List<Genre> { new() { Id = 1, Name = "Fiction" } },
                Reviews = new List<Review> { new() { Id = 1, Rating = 5, Description = "Great!" } }
            }
        };

        _mockBookRepository.Setup(r => r.GetAllBooksAsync())
            .ReturnsAsync(books);

        // Act
        var result = await _bookService.GetAllBooksAsync();

        // Assert
        result.Should().HaveCount(1);
        result[0].Title.Should().Be("Test Book");
        result[0].Price.Should().Be(19.99m);
        result[0].AuthorNames.Should().ContainSingle().Which.Should().Be("Test Author");
        result[0].GenreNames.Should().ContainSingle().Which.Should().Be("Fiction");
        result[0].AverageReviewRating.Should().Be(5);
    }

    [Fact]
    public async Task GetBookByIdAsync_WhenBookExists_ShouldReturnBookDto()
    {
        // Arrange
        var book = new Book
        {
            Id = 1,
            Title = "Test Book",
            Price = 19.99m,
            Authors = new List<Author>(),
            Genres = new List<Genre>(),
            Reviews = new List<Review>()
        };

        _mockBookRepository.Setup(r => r.GetBookByIdAsync(1))
            .ReturnsAsync(book);

        // Act
        var result = await _bookService.GetBookByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Title.Should().Be("Test Book");
    }

    [Fact]
    public async Task GetBookByIdAsync_WhenBookDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        _mockBookRepository.Setup(r => r.GetBookByIdAsync(999))
            .ReturnsAsync((Book?)null);

        // Act
        var result = await _bookService.GetBookByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateBookAsync_ShouldReturnCreatedBookDto()
    {
        // Arrange
        var createDto = new CreateBookDto
        {
            Title = "New Book",
            Price = 29.99m,
            AuthorIds = new List<int> { 1 },
            GenreIds = new List<int> { 1 }
        };

        var createdBook = new Book
        {
            Id = 1,
            Title = "New Book",
            Price = 29.99m,
            Authors = new List<Author> { new() { Id = 1, Name = "Author 1" } },
            Genres = new List<Genre> { new() { Id = 1, Name = "Genre 1" } },
            Reviews = new List<Review>()
        };

        _mockBookRepository.Setup(r => r.CreateBookAsync(It.IsAny<Book>()))
            .ReturnsAsync(createdBook);

        // Act
        var result = await _bookService.CreateBookAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Title.Should().Be("New Book");
        result.Price.Should().Be(29.99m);
    }

    [Fact]
    public async Task UpdateBookPriceAsync_WhenBookExists_ShouldReturnUpdatedBookDto()
    {
        // Arrange
        var updateDto = new UpdateBookPriceDto { Price = 15.99m };
        var updatedBook = new Book
        {
            Id = 1,
            Title = "Test Book",
            Price = 15.99m,
            Authors = new List<Author>(),
            Genres = new List<Genre>(),
            Reviews = new List<Review>()
        };

        _mockBookRepository.Setup(r => r.UpdateBookPriceAsync(1, 15.99m))
            .ReturnsAsync(updatedBook);

        // Act
        var result = await _bookService.UpdateBookPriceAsync(1, updateDto);

        // Assert
        result.Should().NotBeNull();
        result!.Price.Should().Be(15.99m);
    }

    [Fact]
    public async Task UpdateBookPriceAsync_WhenBookDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var updateDto = new UpdateBookPriceDto { Price = 15.99m };
        _mockBookRepository.Setup(r => r.UpdateBookPriceAsync(999, 15.99m))
            .ReturnsAsync((Book?)null);

        // Act
        var result = await _bookService.UpdateBookPriceAsync(999, updateDto);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteBookAsync_WhenBookExists_ShouldReturnTrue()
    {
        // Arrange
        _mockBookRepository.Setup(r => r.DeleteBookAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _bookService.DeleteBookAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteBookAsync_WhenBookDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        _mockBookRepository.Setup(r => r.DeleteBookAsync(999))
            .ReturnsAsync(false);

        // Act
        var result = await _bookService.DeleteBookAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetTop10BooksByRatingAsync_ShouldReturnTopBooks()
    {
        // Arrange
        var topBooks = new List<TopBookDto>
        {
            new() { Id = 1, Title = "Book 1", Price = 19.99m, AverageReviewRating = 4.5, AuthorNames = "Author 1", GenreNames = "Fiction" },
            new() { Id = 2, Title = "Book 2", Price = 24.99m, AverageReviewRating = 4.2, AuthorNames = "Author 2", GenreNames = "Mystery" }
        };

        _mockBookRepository.Setup(r => r.GetTop10BooksByRatingAsync())
            .ReturnsAsync(topBooks);

        // Act
        var result = await _bookService.GetTop10BooksByRatingAsync();

        // Assert
        result.Should().HaveCount(2);
        result[0].AverageReviewRating.Should().Be(4.5);
        result[1].AverageReviewRating.Should().Be(4.2);
    }
}
