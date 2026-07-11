using BookstoreManagementSystem.DTOs;
using BookstoreManagementSystem.Models;
using BookstoreManagementSystem.Repositories;

namespace BookstoreManagementSystem.Services;

public class GenreService : IGenreService
{
    private readonly IGenreRepository _genreRepository;
    private readonly ILogger<GenreService> _logger;

    public GenreService(IGenreRepository genreRepository, ILogger<GenreService> logger)
    {
        _genreRepository = genreRepository;
        _logger = logger;
    }

    public async Task<List<GenreDto>> GetAllGenresAsync()
    {
        var genres = await _genreRepository.GetAllGenresAsync();
        return genres.Select(MapToDto).ToList();
    }

    public async Task<GenreDto?> GetGenreByIdAsync(int id)
    {
        _logger.LogInformation("Retrieving genre with Id: {GenreId}", id);
        var genre = await _genreRepository.GetGenreByIdAsync(id);

        if (genre == null)
        {
            _logger.LogWarning("Genre with Id: {GenreId} not found", id);
            return null;
        }

        return MapToDto(genre);
    }

    public async Task<GenreDto> CreateGenreAsync(CreateGenreDto createGenreDto)
    {
        var genre = new Genre
        {
            Name = createGenreDto.Name
        };

        var createdGenre = await _genreRepository.CreateGenreAsync(genre);
        return MapToDto(createdGenre);
    }

    public async Task<bool> DeleteGenreAsync(int id)
    {
        var result = await _genreRepository.DeleteGenreAsync(id);

        if (!result)
        {
            _logger.LogWarning("Failed to delete genre: Genre with Id: {GenreId} not found", id);
        }

        return result;
    }

    private static GenreDto MapToDto(Genre genre)
    {
        return new GenreDto
        {
            Id = genre.Id,
            Name = genre.Name
        };
    }
}
