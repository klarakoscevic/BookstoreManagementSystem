using BookstoreManagementSystem.DTOs;

namespace BookstoreManagementSystem.Services;

public interface IGenreService
{
    Task<List<GenreDto>> GetAllGenresAsync();
    Task<GenreDto?> GetGenreByIdAsync(int id);
    Task<GenreDto> CreateGenreAsync(CreateGenreDto createGenreDto);
    Task<GenreDto?> UpdateGenreAsync(int id, UpdateGenreDto updateGenreDto);
    Task<bool> DeleteGenreAsync(int id);
}
