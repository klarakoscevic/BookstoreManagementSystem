using BookstoreManagementSystem.Models;

namespace BookstoreManagementSystem.Repositories;

public interface IReviewRepository
{
    Task<List<Review>> GetAllReviewsAsync();
    Task<List<Review>> GetReviewsByBookIdAsync(int bookId);
    Task<Review?> GetReviewByIdAsync(int id);
    Task<Review?> CreateReviewAsync(Review review);
    Task<Review?> UpdateReviewAsync(int id, int rating, string description);
    Task<bool> DeleteReviewAsync(int id);
    Task<bool> ReviewExistsAsync(int id);
}
