using BookstoreManagementSystem.DTOs;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace BookstoreManagementSystem.Tests.IntegrationTests;

public class ReviewsControllerIntegrationTests : IntegrationTestBase
{
    [Fact]
    public async Task GetAllReviews_ShouldReturnAllReviews()
    {
        // Act
        var response = await Client.GetAsync("/api/reviews");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var reviews = await response.Content.ReadFromJsonAsync<List<ReviewDto>>();
        reviews.Should().NotBeNull();
        reviews.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetReview_WithValidId_ShouldReturnReview()
    {
        // Act
        var response = await Client.GetAsync("/api/reviews/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var review = await response.Content.ReadFromJsonAsync<ReviewDto>();
        review.Should().NotBeNull();
        review!.Id.Should().Be(1);
        review.Rating.Should().Be(5);
        review.Description.Should().Be("Amazing book!");
    }

    [Fact]
    public async Task GetReview_WithInvalidId_ShouldReturnNotFound()
    {
        // Act
        var response = await Client.GetAsync("/api/reviews/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateReview_WithValidData_AsAdmin_ShouldCreateReview()
    {
        // Arrange
        await AuthenticateAsync(asAdmin: true);

        var createReviewDto = new CreateReviewDto
        {
            BookId = 3,
            Rating = 4,
            Description = "Great science fiction!"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/reviews", createReviewDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdReview = await response.Content.ReadFromJsonAsync<ReviewDto>();
        createdReview.Should().NotBeNull();
        createdReview!.Rating.Should().Be(4);
        createdReview.Description.Should().Be("Great science fiction!");
        createdReview.BookId.Should().Be(3);

        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateReview_WithInvalidRating_ShouldReturnBadRequest()
    {
        // Arrange
        await AuthenticateAsync(asAdmin: true);

        var createReviewDto = new CreateReviewDto
        {
            BookId = 1,
            Rating = 6, // Invalid rating (should be 1-5)
            Description = "Test review"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/reviews", createReviewDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateReview_ForNonExistentBook_ShouldReturnBadRequest()
    {
        // Arrange
        await AuthenticateAsync(asAdmin: true);

        var createReviewDto = new CreateReviewDto
        {
            BookId = 999,
            Rating = 5,
            Description = "Test review"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/reviews", createReviewDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateReview_AsReadUser_ShouldReturnForbidden()
    {
        // Arrange
        var createReviewDto = new CreateReviewDto
        {
            BookId = 1,
            Rating = 5,
            Description = "Test review"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/reviews", createReviewDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateReview_WithValidData_AsAdmin_ShouldUpdateReview()
    {
        // Arrange
        await AuthenticateAsync(asAdmin: true);

        var updateReviewDto = new UpdateReviewDto
        {
            Rating = 3,
            Description = "Updated review description"
        };

        // Act
        var response = await Client.PutAsJsonAsync("/api/reviews/1", updateReviewDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedReview = await response.Content.ReadFromJsonAsync<ReviewDto>();
        updatedReview.Should().NotBeNull();
        updatedReview!.Rating.Should().Be(3);
        updatedReview.Description.Should().Be("Updated review description");
    }

    [Fact]
    public async Task DeleteReview_WithValidId_AsAdmin_ShouldSoftDeleteReview()
    {
        // Arrange
        await AuthenticateAsync(asAdmin: true);

        // Act
        var response = await Client.DeleteAsync("/api/reviews/2");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify review is soft deleted
        var getResponse = await Client.GetAsync("/api/reviews");
        var reviews = await getResponse.Content.ReadFromJsonAsync<List<ReviewDto>>();
        reviews.Should().NotContain(r => r.Id == 2);
    }

    [Fact]
    public async Task GetBookReviews_WithValidBookId_ShouldReturnReviews()
    {
        // Act
        var response = await Client.GetAsync("/api/reviews/book/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var reviews = await response.Content.ReadFromJsonAsync<List<ReviewDto>>();
        reviews.Should().NotBeNull();
        reviews.Should().NotBeEmpty();
        reviews!.All(r => r.BookId == 1).Should().BeTrue();
    }

    [Fact]
    public async Task GetBookReviews_WithInvalidBookId_ShouldReturnBadRequest()
    {
        // Act
        var response = await Client.GetAsync("/api/reviews/book/50000000000");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
