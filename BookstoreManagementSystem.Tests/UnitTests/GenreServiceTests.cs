using BookstoreManagementSystem.DTOs;
using BookstoreManagementSystem.Models;
using BookstoreManagementSystem.Repositories;
using BookstoreManagementSystem.Services;
using FluentAssertions;
using Moq;

namespace BookstoreManagementSystem.Tests.UnitTests;

public class GenreServiceTests
{
    private readonly Mock<IGenreRepository> _mockGenreRepository;
    private readonly GenreService _genreService;

    public GenreServiceTests()
    {
        _mockGenreRepository = new Mock<IGenreRepository>();
        _genreService = new GenreService(_mockGenreRepository.Object);
    }

    [Fact]
    public async Task GetAllGenresAsync_ShouldReturnGenreDtos()
    {
        // Arrange
        var genres = new List<Genre>
        {
            new() { Id = 1, Name = "Fiction" },
            new() { Id = 2, Name = "Mystery" },
            new() { Id = 3, Name = "Science Fiction" }
        };

        _mockGenreRepository.Setup(r => r.GetAllGenresAsync())
            .ReturnsAsync(genres);

        // Act
        var result = await _genreService.GetAllGenresAsync();

        // Assert
        result.Should().HaveCount(3);
        result[0].Id.Should().Be(1);
        result[0].Name.Should().Be("Fiction");
        result[1].Name.Should().Be("Mystery");
        result[2].Name.Should().Be("Science Fiction");
    }

    [Fact]
    public async Task GetAllGenresAsync_WhenNoGenres_ShouldReturnEmptyList()
    {
        // Arrange
        _mockGenreRepository.Setup(r => r.GetAllGenresAsync())
            .ReturnsAsync(new List<Genre>());

        // Act
        var result = await _genreService.GetAllGenresAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetGenreByIdAsync_WhenGenreExists_ShouldReturnGenreDto()
    {
        // Arrange
        var genre = new Genre
        {
            Id = 1,
            Name = "Fantasy"
        };

        _mockGenreRepository.Setup(r => r.GetGenreByIdAsync(1))
            .ReturnsAsync(genre);

        // Act
        var result = await _genreService.GetGenreByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Fantasy");
    }

    [Fact]
    public async Task GetGenreByIdAsync_WhenGenreDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        _mockGenreRepository.Setup(r => r.GetGenreByIdAsync(999))
            .ReturnsAsync((Genre?)null);

        // Act
        var result = await _genreService.GetGenreByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateGenreAsync_ShouldReturnCreatedGenreDto()
    {
        // Arrange
        var createDto = new CreateGenreDto
        {
            Name = "Horror"
        };

        var createdGenre = new Genre
        {
            Id = 1,
            Name = "Horror"
        };

        _mockGenreRepository.Setup(r => r.CreateGenreAsync(It.IsAny<Genre>()))
            .ReturnsAsync(createdGenre);

        // Act
        var result = await _genreService.CreateGenreAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Name.Should().Be("Horror");
        _mockGenreRepository.Verify(r => r.CreateGenreAsync(It.Is<Genre>(g => g.Name == "Horror")), Times.Once);
    }

    [Fact]
    public async Task DeleteGenreAsync_WhenGenreExists_ShouldReturnTrue()
    {
        // Arrange
        _mockGenreRepository.Setup(r => r.DeleteGenreAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _genreService.DeleteGenreAsync(1);

        // Assert
        result.Should().BeTrue();
        _mockGenreRepository.Verify(r => r.DeleteGenreAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeleteGenreAsync_WhenGenreDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        _mockGenreRepository.Setup(r => r.DeleteGenreAsync(999))
            .ReturnsAsync(false);

        // Act
        var result = await _genreService.DeleteGenreAsync(999);

        // Assert
        result.Should().BeFalse();
        _mockGenreRepository.Verify(r => r.DeleteGenreAsync(999), Times.Once);
    }
}
