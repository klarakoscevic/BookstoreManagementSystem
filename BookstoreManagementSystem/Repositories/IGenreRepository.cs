using BookstoreManagementSystem.Models;

namespace BookstoreManagementSystem.Repositories;

public interface IGenreRepository
{
    Task<List<Genre>> GetAllGenresAsync();
    Task<Genre?> GetGenreByIdAsync(int id);
    Task<Genre> CreateGenreAsync(Genre genre);
    Task<bool> DeleteGenreAsync(int id);
    Task<bool> GenreExistsAsync(int id);
}
