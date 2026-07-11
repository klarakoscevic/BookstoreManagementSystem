using BookstoreManagementSystem.DTOs;
using BookstoreManagementSystem.Models;
using BookstoreManagementSystem.Repositories;
using BookstoreManagementSystem.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace BookstoreManagementSystem.Tests.UnitTests;

public class ReviewServiceTests
{
    private readonly Mock<IReviewRepository> _mockReviewRepository;
    private readonly Mock<ILogger<ReviewService>> _mockLogger;
    private readonly ReviewService _reviewService;

    public ReviewServiceTests()
    {
        _mockReviewRepository = new Mock<IReviewRepository>();
        _mockLogger = new Mock<ILogger<ReviewService>>();
        _reviewService = new ReviewService(_mockReviewRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAllReviewsAsync_ShouldReturnReviewDtos()
    {
        // Arrange
        var reviews = new List<Review>
        {
            new()
            {
                Id = 1,
                Rating = 5,
                Description = "Excellent!",
                BookId = 1,
                Book = new Book { Id = 1, Title = "Test Book", Price = 19.99m }
            },
            new()
            {
                Id = 2,
                Rating = 4,
                Description = "Good read",
                BookId = 1,
                Book = new Book { Id = 1, Title = "Test Book", Price = 19.99m }
            }
        };

        _mockReviewRepository.Setup(r => r.GetAllReviewsAsync())
            .ReturnsAsync(reviews);

        // Act
        var result = await _reviewService.GetAllReviewsAsync();

        // Assert
        result.Should().HaveCount(2);
        result[0].Rating.Should().Be(5);
        result[0].Description.Should().Be("Excellent!");
        result[0].BookTitle.Should().Be("Test Book");
        result[1].Rating.Should().Be(4);
    }

    [Fact]
    public async Task GetAllReviewsAsync_WhenNoReviews_ShouldReturnEmptyList()
    {
        // Arrange
        _mockReviewRepository.Setup(r => r.GetAllReviewsAsync())
            .ReturnsAsync(new List<Review>());

        // Act
        var result = await _reviewService.GetAllReviewsAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetReviewsByBookIdAsync_ShouldReturnReviewsForSpecificBook()
    {
        // Arrange
        var reviews = new List<Review>
        {
            new()
            {
                Id = 1,
                Rating = 5,
                Description = "Great!",
                BookId = 1,
                Book = new Book { Id = 1, Title = "Test Book", Price = 19.99m }
            }
        };

        _mockReviewRepository.Setup(r => r.GetReviewsByBookIdAsync(1))
            .ReturnsAsync(reviews);

        // Act
        var result = await _reviewService.GetReviewsByBookIdAsync(1);

        // Assert
        result.Should().HaveCount(1);
        result[0].BookId.Should().Be(1);
        result[0].Rating.Should().Be(5);
    }

    [Fact]
    public async Task GetReviewByIdAsync_WhenReviewExists_ShouldReturnReviewDto()
    {
        // Arrange
        var review = new Review
        {
            Id = 1,
            Rating = 5,
            Description = "Excellent book!",
            BookId = 1,
            Book = new Book { Id = 1, Title = "Test Book", Price = 19.99m }
        };

        _mockReviewRepository.Setup(r => r.GetReviewByIdAsync(1))
            .ReturnsAsync(review);

        // Act
        var result = await _reviewService.GetReviewByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Rating.Should().Be(5);
        result.Description.Should().Be("Excellent book!");
        result.BookTitle.Should().Be("Test Book");
    }

    [Fact]
    public async Task GetReviewByIdAsync_WhenReviewDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        _mockReviewRepository.Setup(r => r.GetReviewByIdAsync(999))
            .ReturnsAsync((Review?)null);

        // Act
        var result = await _reviewService.GetReviewByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateReviewAsync_WhenBookExists_ShouldReturnCreatedReviewDto()
    {
        // Arrange
        var createDto = new CreateReviewDto
        {
            Rating = 4,
            Description = "Good book!",
            BookId = 1
        };

        var createdReview = new Review
        {
            Id = 1,
            Rating = 4,
            Description = "Good book!",
            BookId = 1,
            Book = new Book { Id = 1, Title = "Test Book", Price = 19.99m }
        };

        _mockReviewRepository.Setup(r => r.CreateReviewAsync(It.IsAny<Review>()))
            .ReturnsAsync(createdReview);

        // Act
        var result = await _reviewService.CreateReviewAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Rating.Should().Be(4);
        result.Description.Should().Be("Good book!");
        result.BookId.Should().Be(1);
        _mockReviewRepository.Verify(r => r.CreateReviewAsync(It.Is<Review>(
            rv => rv.Rating == 4 && rv.Description == "Good book!" && rv.BookId == 1)), Times.Once);
    }

    [Fact]
    public async Task CreateReviewAsync_WhenBookDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var createDto = new CreateReviewDto
        {
            Rating = 4,
            Description = "Good book!",
            BookId = 999
        };

        _mockReviewRepository.Setup(r => r.CreateReviewAsync(It.IsAny<Review>()))
            .ReturnsAsync((Review?)null);

        // Act
        var result = await _reviewService.CreateReviewAsync(createDto);

        // Assert
        result.Should().BeNull();
        _mockReviewRepository.Verify(r => r.CreateReviewAsync(It.Is<Review>(
            rv => rv.Rating == 4 && rv.Description == "Good book!" && rv.BookId == 999)), Times.Once);
    }

    [Fact]
    public async Task UpdateReviewAsync_WhenReviewExists_ShouldReturnUpdatedReviewDto()
    {
        // Arrange
        var updateDto = new UpdateReviewDto
        {
            Rating = 3,
            Description = "Updated description"
        };

        var updatedReview = new Review
        {
            Id = 1,
            Rating = 3,
            Description = "Updated description",
            BookId = 1,
            Book = new Book { Id = 1, Title = "Test Book", Price = 19.99m }
        };

        _mockReviewRepository.Setup(r => r.UpdateReviewAsync(1, 3, "Updated description"))
            .ReturnsAsync(updatedReview);

        // Act
        var result = await _reviewService.UpdateReviewAsync(1, updateDto);

        // Assert
        result.Should().NotBeNull();
        result!.Rating.Should().Be(3);
        result.Description.Should().Be("Updated description");
    }

    [Fact]
    public async Task UpdateReviewAsync_WhenReviewDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var updateDto = new UpdateReviewDto
        {
            Rating = 3,
            Description = "Updated description"
        };

        _mockReviewRepository.Setup(r => r.UpdateReviewAsync(999, 3, "Updated description"))
            .ReturnsAsync((Review?)null);

        // Act
        var result = await _reviewService.UpdateReviewAsync(999, updateDto);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteReviewAsync_WhenReviewExists_ShouldReturnTrue()
    {
        // Arrange
        _mockReviewRepository.Setup(r => r.DeleteReviewAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _reviewService.DeleteReviewAsync(1);

        // Assert
        result.Should().BeTrue();
        _mockReviewRepository.Verify(r => r.DeleteReviewAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeleteReviewAsync_WhenReviewDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        _mockReviewRepository.Setup(r => r.DeleteReviewAsync(999))
            .ReturnsAsync(false);

        // Act
        var result = await _reviewService.DeleteReviewAsync(999);

        // Assert
        result.Should().BeFalse();
        _mockReviewRepository.Verify(r => r.DeleteReviewAsync(999), Times.Once);
    }
}
