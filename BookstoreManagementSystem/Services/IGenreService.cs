using BookstoreManagementSystem.DTOs;

namespace BookstoreManagementSystem.Services;

public interface IGenreService
{
    Task<List<GenreDto>> GetAllGenresAsync();
    Task<GenreDto?> GetGenreByIdAsync(int id);
    Task<GenreDto> CreateGenreAsync(CreateGenreDto createGenreDto);
    Task<bool> DeleteGenreAsync(int id);
}
