using BookstoreManagementSystem.DTOs;
using BookstoreManagementSystem.Models;
using BookstoreManagementSystem.Repositories;

namespace BookstoreManagementSystem.Services;

public class GenreService : IGenreService
{
    private readonly IGenreRepository _genreRepository;

    public GenreService(IGenreRepository genreRepository)
    {
        _genreRepository = genreRepository;
    }

    public async Task<List<GenreDto>> GetAllGenresAsync()
    {
        var genres = await _genreRepository.GetAllGenresAsync();
        return genres.Select(MapToDto).ToList();
    }

    public async Task<GenreDto?> GetGenreByIdAsync(int id)
    {
        var genre = await _genreRepository.GetGenreByIdAsync(id);
        return genre == null ? null : MapToDto(genre);
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
        return await _genreRepository.DeleteGenreAsync(id);
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
