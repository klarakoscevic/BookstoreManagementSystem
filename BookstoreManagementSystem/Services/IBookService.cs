using BookstoreManagementSystem.DTOs;

namespace BookstoreManagementSystem.Services;

public interface IBookService
{
    Task<List<BookDto>> GetAllBooksAsync();
    Task<BookDto?> GetBookByIdAsync(int id);
    Task<BookDto> CreateBookAsync(CreateBookDto createBookDto);
    Task<BookDto?> UpdateBookPriceAsync(int id, UpdateBookPriceDto updateBookPriceDto);
    Task<bool> DeleteBookAsync(int id);
    Task<List<TopBookDto>> GetTop10BooksByRatingAsync();
}
