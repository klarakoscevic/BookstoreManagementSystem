using BookstoreManagementSystem.DTOs;
using BookstoreManagementSystem.Models;
using BookstoreManagementSystem.Repositories;

namespace BookstoreManagementSystem.Services;

public class AuthorService : IAuthorService
{
    private readonly IAuthorRepository _authorRepository;
    private readonly ILogger<AuthorService> _logger;

    public AuthorService(IAuthorRepository authorRepository, ILogger<AuthorService> logger)
    {
        _authorRepository = authorRepository;
        _logger = logger;
    }

    public async Task<List<AuthorDto>> GetAllAuthorsAsync()
    {
        var authors = await _authorRepository.GetAllAuthorsAsync();
        return authors.Select(MapToDto).ToList();
    }

    public async Task<AuthorDto?> GetAuthorByIdAsync(int id)
    {
        var author = await _authorRepository.GetAuthorByIdAsync(id);

        if (author == null)
        {
            _logger.LogWarning("Author with Id: {AuthorId} not found", id);
            return null;
        }

        return MapToDto(author);
    }

    public async Task<AuthorDto> CreateAuthorAsync(CreateAuthorDto createAuthorDto)
    {
        var author = new Author
        {
            Name = createAuthorDto.Name,
            YearOfBirth = createAuthorDto.YearOfBirth
        };

        var createdAuthor = await _authorRepository.CreateAuthorAsync(author);
        return MapToDto(createdAuthor);
    }

    public async Task<AuthorDto?> UpdateAuthorAsync(int id, UpdateAuthorDto updateAuthorDto)
    {
        var author = new Author
        {
            Id = id,
            Name = updateAuthorDto.Name,
            YearOfBirth = updateAuthorDto.YearOfBirth
        };

        var updatedAuthor = await _authorRepository.UpdateAuthorAsync(author);

        if (updatedAuthor == null)
        {
            _logger.LogWarning("Failed to update author: Author with Id: {AuthorId} not found", id);
            return null;
        }

        return MapToDto(updatedAuthor);
    }

    public async Task<bool> DeleteAuthorAsync(int id)
    {
        var result = await _authorRepository.DeleteAuthorAsync(id);

        if (!result)
        {
            _logger.LogWarning("Failed to delete author: Author with Id: {AuthorId} not found", id);
        }

        return result;
    }

    private static AuthorDto MapToDto(Author author)
    {
        return new AuthorDto
        {
            Id = author.Id,
            Name = author.Name,
            YearOfBirth = author.YearOfBirth
        };
    }
}
