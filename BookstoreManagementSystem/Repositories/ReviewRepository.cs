using BookstoreManagementSystem.Data;
using BookstoreManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace BookstoreManagementSystem.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly BookstoreDbContext _context;

    public ReviewRepository(BookstoreDbContext context)
    {
        _context = context;
    }

    public async Task<List<Review>> GetAllReviewsAsync()
    {
        return await _context.Reviews
            .Include(r => r.Book)
            .ToListAsync();
    }

    public async Task<List<Review>> GetReviewsByBookIdAsync(int bookId)
    {
        return await _context.Reviews
            .Include(r => r.Book)
            .Where(r => r.BookId == bookId)
            .ToListAsync();
    }

    public async Task<Review?> GetReviewByIdAsync(int id)
    {
        return await _context.Reviews
            .Include(r => r.Book)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Review?> CreateReviewAsync(Review review)
    {
        // Validate that the book exists
        var bookExists = await _context.Books.AnyAsync(b => b.Id == review.BookId);
        if (!bookExists)
        {
            return null;
        }

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        return await _context.Reviews
            .Include(r => r.Book)
            .FirstAsync(r => r.Id == review.Id);
    }

    public async Task<Review?> UpdateReviewAsync(int id, int rating, string description)
    {
        var review = await _context.Reviews
            .Include(r => r.Book)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (review == null)
        {
            return null;
        }

        review.Rating = rating;
        review.Description = description;

        await _context.SaveChangesAsync();
        return review;
    }

    public async Task<bool> DeleteReviewAsync(int id)
    {
        var review = await _context.Reviews.FindAsync(id);

        if (review == null)
        {
            return false;
        }

        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ReviewExistsAsync(int id)
    {
        return await _context.Reviews.AnyAsync(r => r.Id == id);
    }
}
