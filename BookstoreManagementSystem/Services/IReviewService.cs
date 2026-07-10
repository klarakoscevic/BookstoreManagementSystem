using BookstoreManagementSystem.DTOs;

namespace BookstoreManagementSystem.Services;

public interface IReviewService
{
    Task<List<ReviewDto>> GetAllReviewsAsync();
    Task<List<ReviewDto>> GetReviewsByBookIdAsync(int bookId);
    Task<ReviewDto?> GetReviewByIdAsync(int id);
    Task<ReviewDto?> CreateReviewAsync(CreateReviewDto createReviewDto);
    Task<ReviewDto?> UpdateReviewAsync(int id, UpdateReviewDto updateReviewDto);
    Task<bool> DeleteReviewAsync(int id);
}
