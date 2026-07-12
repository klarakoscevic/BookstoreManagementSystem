using BookstoreManagementSystem.DTOs;
using BookstoreManagementSystem.Models;
using BookstoreManagementSystem.Repositories;
using BookstoreManagementSystem.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace BookstoreManagementSystem.Tests.UnitTests;

public class AuthorServiceTests
{
    private readonly Mock<IAuthorRepository> _mockAuthorRepository;
    private readonly Mock<ILogger<AuthorService>> _mockLogger;
    private readonly AuthorService _authorService;

    public AuthorServiceTests()
    {
        _mockAuthorRepository = new Mock<IAuthorRepository>();
        _mockLogger = new Mock<ILogger<AuthorService>>();
        _authorService = new AuthorService(_mockAuthorRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAllAuthorsAsync_ShouldReturnAuthorDtos()
    {
        // Arrange
        var authors = new List<Author>
        {
            new() { Id = 1, Name = "J.K. Rowling", YearOfBirth = 1965 },
            new() { Id = 2, Name = "Stephen King", YearOfBirth = 1947 },
            new() { Id = 3, Name = "George R.R. Martin", YearOfBirth = 1948 }
        };

        _mockAuthorRepository.Setup(r => r.GetAllAuthorsAsync())
            .ReturnsAsync(authors);

        // Act
        var result = await _authorService.GetAllAuthorsAsync();

        // Assert
        result.Should().HaveCount(3);
        result[0].Id.Should().Be(1);
        result[0].Name.Should().Be("J.K. Rowling");
        result[0].YearOfBirth.Should().Be(1965);
        result[1].Name.Should().Be("Stephen King");
        result[2].Name.Should().Be("George R.R. Martin");
    }

    [Fact]
    public async Task GetAllAuthorsAsync_WhenNoAuthors_ShouldReturnEmptyList()
    {
        // Arrange
        _mockAuthorRepository.Setup(r => r.GetAllAuthorsAsync())
            .ReturnsAsync(new List<Author>());

        // Act
        var result = await _authorService.GetAllAuthorsAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAuthorByIdAsync_WhenAuthorExists_ShouldReturnAuthorDto()
    {
        // Arrange
        var author = new Author
        {
            Id = 1,
            Name = "J.R.R. Tolkien",
            YearOfBirth = 1892
        };

        _mockAuthorRepository.Setup(r => r.GetAuthorByIdAsync(1))
            .ReturnsAsync(author);

        // Act
        var result = await _authorService.GetAuthorByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("J.R.R. Tolkien");
        result.YearOfBirth.Should().Be(1892);
    }

    [Fact]
    public async Task GetAuthorByIdAsync_WhenAuthorDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        _mockAuthorRepository.Setup(r => r.GetAuthorByIdAsync(999))
            .ReturnsAsync((Author?)null);

        // Act
        var result = await _authorService.GetAuthorByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAuthorAsync_ShouldReturnCreatedAuthorDto()
    {
        // Arrange
        var createDto = new CreateAuthorDto
        {
            Name = "Brandon Sanderson",
            YearOfBirth = 1975
        };

        var createdAuthor = new Author
        {
            Id = 1,
            Name = "Brandon Sanderson",
            YearOfBirth = 1975
        };

        _mockAuthorRepository.Setup(r => r.CreateAuthorAsync(It.IsAny<Author>()))
            .ReturnsAsync(createdAuthor);

        // Act
        var result = await _authorService.CreateAuthorAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Name.Should().Be("Brandon Sanderson");
        result.YearOfBirth.Should().Be(1975);
        _mockAuthorRepository.Verify(r => r.CreateAuthorAsync(It.Is<Author>(a => 
            a.Name == "Brandon Sanderson" && a.YearOfBirth == 1975)), Times.Once);
    }

    [Fact]
    public async Task UpdateAuthorAsync_WhenAuthorExists_ShouldReturnUpdatedAuthorDto()
    {
        // Arrange
        var updateDto = new UpdateAuthorDto
        {
            Name = "Updated Author",
            YearOfBirth = 1980
        };

        var updatedAuthor = new Author
        {
            Id = 1,
            Name = "Updated Author",
            YearOfBirth = 1980
        };

        _mockAuthorRepository.Setup(r => r.UpdateAuthorAsync(It.IsAny<Author>()))
            .ReturnsAsync(updatedAuthor);

        // Act
        var result = await _authorService.UpdateAuthorAsync(1, updateDto);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Updated Author");
        result.YearOfBirth.Should().Be(1980);
        _mockAuthorRepository.Verify(r => r.UpdateAuthorAsync(It.Is<Author>(a => 
            a.Id == 1 && a.Name == "Updated Author" && a.YearOfBirth == 1980)), Times.Once);
    }

    [Fact]
    public async Task UpdateAuthorAsync_WhenAuthorDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var updateDto = new UpdateAuthorDto
        {
            Name = "Updated Author",
            YearOfBirth = 1980
        };

        _mockAuthorRepository.Setup(r => r.UpdateAuthorAsync(It.IsAny<Author>()))
            .ReturnsAsync((Author?)null);

        // Act
        var result = await _authorService.UpdateAuthorAsync(999, updateDto);

        // Assert
        result.Should().BeNull();
        _mockAuthorRepository.Verify(r => r.UpdateAuthorAsync(It.Is<Author>(a => a.Id == 999)), Times.Once);
    }

    [Fact]
    public async Task DeleteAuthorAsync_WhenAuthorExists_ShouldReturnTrue()
    {
        // Arrange
        _mockAuthorRepository.Setup(r => r.DeleteAuthorAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _authorService.DeleteAuthorAsync(1);

        // Assert
        result.Should().BeTrue();
        _mockAuthorRepository.Verify(r => r.DeleteAuthorAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeleteAuthorAsync_WhenAuthorDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        _mockAuthorRepository.Setup(r => r.DeleteAuthorAsync(999))
            .ReturnsAsync(false);

        // Act
        var result = await _authorService.DeleteAuthorAsync(999);

        // Assert
        result.Should().BeFalse();
        _mockAuthorRepository.Verify(r => r.DeleteAuthorAsync(999), Times.Once);
    }
}
