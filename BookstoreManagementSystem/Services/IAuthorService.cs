using BookstoreManagementSystem.DTOs;

namespace BookstoreManagementSystem.Services;

public interface IAuthorService
{
    Task<List<AuthorDto>> GetAllAuthorsAsync();
    Task<AuthorDto?> GetAuthorByIdAsync(int id);
    Task<AuthorDto> CreateAuthorAsync(CreateAuthorDto createAuthorDto);
    Task<bool> DeleteAuthorAsync(int id);
}
