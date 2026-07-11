using BookstoreManagementSystem.DTOs;

namespace BookstoreManagementSystem.Services;

public interface IBookImportService
{
    Task ImportBooksAsync();
    Task<List<ImportBookDto>> FetchExternalBooksAsync();
    Task<int> GetBookCountAsync();
    Task<int> GetAuthorCountAsync();
    Task<int> GetGenreCountAsync();
}
