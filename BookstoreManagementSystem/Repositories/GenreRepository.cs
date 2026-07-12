using BookstoreManagementSystem.Data;
using BookstoreManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace BookstoreManagementSystem.Repositories;

public class GenreRepository : IGenreRepository
{
    private readonly BookstoreDbContext _context;

    public GenreRepository(BookstoreDbContext context)
    {
        _context = context;
    }

    public async Task<List<Genre>> GetAllGenresAsync()
    {
        return await _context.Genres.ToListAsync();
    }

    public async Task<Genre?> GetGenreByIdAsync(int id)
    {
        return await _context.Genres.FindAsync(id);
    }

    public async Task<Genre> CreateGenreAsync(Genre genre)
    {
        _context.Genres.Add(genre);
        await _context.SaveChangesAsync();
        return genre;
    }

    public async Task<Genre?> UpdateGenreAsync(Genre genre)
    {
        var existingGenre = await _context.Genres.FindAsync(genre.Id);
        if (existingGenre == null)
            return null;

        existingGenre.Name = genre.Name;
        await _context.SaveChangesAsync();
        return existingGenre;
    }

    public async Task<bool> DeleteGenreAsync(int id)
    {
        var genre = await _context.Genres.FindAsync(id);
        if (genre == null)
            return false;

        _context.Genres.Remove(genre);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> GenreExistsAsync(int id)
    {
        return await _context.Genres.AnyAsync(g => g.Id == id);
    }
}
