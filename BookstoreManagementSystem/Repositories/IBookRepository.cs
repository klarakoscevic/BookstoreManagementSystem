using BookstoreManagementSystem.DTOs;
using BookstoreManagementSystem.Models;

namespace BookstoreManagementSystem.Repositories;

public interface IBookRepository
{
    Task<List<Book>> GetAllBooksAsync();
    Task<Book?> GetBookByIdAsync(int id);
    Task<Book> CreateBookAsync(Book book);
    Task<Book?> UpdateBookPriceAsync(int id, decimal price);
    Task<bool> DeleteBookAsync(int id);
    Task<bool> BookExistsAsync(int id);
    Task<List<TopBookDto>> GetTop10BooksByRatingAsync();
}
