using BookstoreManagementSystem.DTOs;
using BookstoreManagementSystem.Models;
using BookstoreManagementSystem.Repositories;

namespace BookstoreManagementSystem.Services;

public class AuthorService : IAuthorService
{
    private readonly IAuthorRepository _authorRepository;

    public AuthorService(IAuthorRepository authorRepository)
    {
        _authorRepository = authorRepository;
    }

    public async Task<List<AuthorDto>> GetAllAuthorsAsync()
    {
        var authors = await _authorRepository.GetAllAuthorsAsync();
        return authors.Select(MapToDto).ToList();
    }

    public async Task<AuthorDto?> GetAuthorByIdAsync(int id)
    {
        var author = await _authorRepository.GetAuthorByIdAsync(id);
        return author == null ? null : MapToDto(author);
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

    public async Task<bool> DeleteAuthorAsync(int id)
    {
        return await _authorRepository.DeleteAuthorAsync(id);
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
