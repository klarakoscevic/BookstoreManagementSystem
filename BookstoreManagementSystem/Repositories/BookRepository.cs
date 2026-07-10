using BookstoreManagementSystem.Data;
using BookstoreManagementSystem.DTOs;
using BookstoreManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace BookstoreManagementSystem.Repositories;

public class BookRepository : IBookRepository
{
    private readonly BookstoreDbContext _context;

    public BookRepository(BookstoreDbContext context)
    {
        _context = context;
    }

    public async Task<List<Book>> GetAllBooksAsync()
    {
        return await _context.Books
            .Include(b => b.Authors)
            .Include(b => b.Genres)
            .Include(b => b.Reviews)
            .ToListAsync();
    }

    public async Task<Book?> GetBookByIdAsync(int id)
    {
        return await _context.Books
            .Include(b => b.Authors)
            .Include(b => b.Genres)
            .Include(b => b.Reviews)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<Book> CreateBookAsync(Book book)
    {
        if (book.Authors.Any())
        {
            var authorIds = book.Authors.Select(a => a.Id).ToList();
            var authors = await _context.Authors
                .Where(a => authorIds.Contains(a.Id))
                .ToListAsync();
            book.Authors = authors;
        }

        if (book.Genres.Any())
        {
            var genreIds = book.Genres.Select(g => g.Id).ToList();
            var genres = await _context.Genres
                .Where(g => genreIds.Contains(g.Id))
                .ToListAsync();
            book.Genres = genres;
        }

        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        // Reload with all navigation properties in one query
        return await _context.Books
            .Include(b => b.Authors)
            .Include(b => b.Genres)
            .Include(b => b.Reviews)
            .FirstAsync(b => b.Id == book.Id);
    }

    public async Task<Book?> UpdateBookPriceAsync(int id, decimal price)
    {
        var book = await _context.Books
            .Include(b => b.Authors)
            .Include(b => b.Genres)
            .Include(b => b.Reviews)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (book == null)
            return null;

        book.Price = price;
        await _context.SaveChangesAsync();

        return book;
    }

    public async Task<bool> DeleteBookAsync(int id)
    {
        var book = await _context.Books.FindAsync(id);
        if (book == null)
            return false;

        _context.Books.Remove(book);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> BookExistsAsync(int id)
    {
        return await _context.Books.AnyAsync(b => b.Id == id);
    }

    public async Task<List<TopBookDto>> GetTop10BooksByRatingAsync()
    {
        var sql = @"
         SELECT TOP 10 
             b.Id,
             b.Title,
             b.Price,
             ISNULL(AVG(CAST(r.Rating AS FLOAT)), 0) AS AverageReviewRating,
             ISNULL((
                 SELECT STRING_AGG(a.Name, ', ')
                 FROM (
                     SELECT DISTINCT a.Name
                     FROM BookAuthors ba
                     INNER JOIN Authors a ON ba.AuthorsId = a.Id
                     WHERE ba.BooksId = b.Id AND a.IsActive = 1
                 ) a
             ), '') AS AuthorNames,
             ISNULL((
                 SELECT STRING_AGG(g.Name, ', ')
                 FROM (
                     SELECT DISTINCT g.Name
                     FROM BookGenres bg
                     INNER JOIN Genres g ON bg.GenresId = g.Id
                     WHERE bg.BooksId = b.Id AND g.IsActive = 1
                 ) g
             ), '') AS GenreNames
         FROM Books b
         LEFT JOIN Reviews r ON b.Id = r.BookId AND r.IsActive = 1
         WHERE b.IsActive = 1
         GROUP BY b.Id, b.Title, b.Price
         ORDER BY AverageReviewRating DESC, b.Title";

        var result = await _context.Database
            .SqlQueryRaw<TopBookDto>(sql)
            .ToListAsync();

        return result;
    }
}
