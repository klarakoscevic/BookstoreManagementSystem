using BookstoreManagementSystem.DTOs;
using BookstoreManagementSystem.Models;
using BookstoreManagementSystem.Repositories;

namespace BookstoreManagementSystem.Services;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository;

    public ReviewService(IReviewRepository reviewRepository)
    {
        _reviewRepository = reviewRepository;
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
        return review == null ? null : MapToDto(review);
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
        return createdReview == null ? null : MapToDto(createdReview);
    }

    public async Task<ReviewDto?> UpdateReviewAsync(int id, UpdateReviewDto updateReviewDto)
    {
        var review = await _reviewRepository.UpdateReviewAsync(
            id,
            updateReviewDto.Rating,
            updateReviewDto.Description);

        return review == null ? null : MapToDto(review);
    }

    public async Task<bool> DeleteReviewAsync(int id)
    {
        return await _reviewRepository.DeleteReviewAsync(id);
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
