using BookstoreManagementSystem.DTOs;
using BookstoreManagementSystem.Models;
using BookstoreManagementSystem.Repositories;

namespace BookstoreManagementSystem.Services;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository;
    private readonly ILogger<ReviewService> _logger;

    public ReviewService(IReviewRepository reviewRepository, ILogger<ReviewService> logger)
    {
        _reviewRepository = reviewRepository;
        _logger = logger;
    }

    public async Task<List<ReviewDto>> GetAllReviewsAsync()
    {
        var reviews = await _reviewRepository.GetAllReviewsAsync();
        return reviews.Select(MapToDto).ToList();
    }

    public async Task<List<ReviewDto>> GetReviewsByBookIdAsync(int bookId)
    {
        var reviews = await _reviewRepository.GetReviewsByBookIdAsync(bookId);
        return reviews.Select(MapToDto).ToList();
    }

    public async Task<ReviewDto?> GetReviewByIdAsync(int id)
    {
        var review = await _reviewRepository.GetReviewByIdAsync(id);

        if (review == null)
        {
            _logger.LogWarning("Review with Id: {ReviewId} not found", id);
            return null;
        }

        return MapToDto(review);
    }

    public async Task<ReviewDto?> CreateReviewAsync(CreateReviewDto createReviewDto)
    {
        var review = new Review
        {
            Rating = createReviewDto.Rating,
            Description = createReviewDto.Description,
            BookId = createReviewDto.BookId
        };

        var createdReview = await _reviewRepository.CreateReviewAsync(review);

        if (createdReview == null)
        {
            _logger.LogWarning("Failed to create review: Book with Id: {BookId} not found", createReviewDto.BookId);
            return null;
        }

        return MapToDto(createdReview);
    }

    public async Task<ReviewDto?> UpdateReviewAsync(int id, UpdateReviewDto updateReviewDto)
    {
        var review = await _reviewRepository.UpdateReviewAsync(
              id,
              updateReviewDto.Rating,
              updateReviewDto.Description);

        if (review == null)
        {
            _logger.LogWarning("Failed to update review: Review with Id: {ReviewId} not found", id);
            return null;
        }

        return MapToDto(review);
    }

    public async Task<bool> DeleteReviewAsync(int id)
    {
        var result = await _reviewRepository.DeleteReviewAsync(id);

        if (!result)
        {
            _logger.LogWarning("Failed to delete review: Review with Id: {ReviewId} not found", id);
        }

        return result;
    }

    private static ReviewDto MapToDto(Review review)
    {
        return new ReviewDto
        {
            Id = review.Id,
            Rating = review.Rating,
            Description = review.Description,
            BookId = review.BookId,
            BookTitle = review.Book?.Title ?? string.Empty
        };
    }
}
