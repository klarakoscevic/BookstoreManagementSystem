using BookstoreManagementSystem.DTOs;

namespace BookstoreManagementSystem.Services;

public interface IAuthorService
{
    Task<List<AuthorDto>> GetAllAuthorsAsync();
    Task<AuthorDto?> GetAuthorByIdAsync(int id);
    Task<AuthorDto> CreateAuthorAsync(CreateAuthorDto createAuthorDto);
    Task<AuthorDto?> UpdateAuthorAsync(int id, UpdateAuthorDto updateAuthorDto);
    Task<bool> DeleteAuthorAsync(int id);
}
