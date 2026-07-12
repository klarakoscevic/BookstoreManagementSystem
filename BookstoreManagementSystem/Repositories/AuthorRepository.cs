using BookstoreManagementSystem.Data;
using BookstoreManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace BookstoreManagementSystem.Repositories;

public class AuthorRepository : IAuthorRepository
{
    private readonly BookstoreDbContext _context;

    public AuthorRepository(BookstoreDbContext context)
    {
        _context = context;
    }

    public async Task<List<Author>> GetAllAuthorsAsync()
    {
        return await _context.Authors.ToListAsync();
    }

    public async Task<Author?> GetAuthorByIdAsync(int id)
    {
        return await _context.Authors.FindAsync(id);
    }

    public async Task<Author> CreateAuthorAsync(Author author)
    {
        _context.Authors.Add(author);
        await _context.SaveChangesAsync();
        return author;
    }

    public async Task<Author?> UpdateAuthorAsync(Author author)
    {
        var existingAuthor = await _context.Authors.FindAsync(author.Id);
        if (existingAuthor == null)
            return null;

        existingAuthor.Name = author.Name;
        existingAuthor.YearOfBirth = author.YearOfBirth;
        await _context.SaveChangesAsync();
        return existingAuthor;
    }

    public async Task<bool> DeleteAuthorAsync(int id)
    {
        var author = await _context.Authors.FindAsync(id);
        if (author == null)
            return false;

        _context.Authors.Remove(author);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AuthorExistsAsync(int id)
    {
        return await _context.Authors.AnyAsync(a => a.Id == id);
    }
}
