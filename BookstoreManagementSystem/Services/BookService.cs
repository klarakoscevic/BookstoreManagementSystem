using BookstoreManagementSystem.DTOs;
using BookstoreManagementSystem.Models;
using BookstoreManagementSystem.Repositories;

namespace BookstoreManagementSystem.Services;

public class BookService : IBookService
{
    private readonly IBookRepository _bookRepository;

    public BookService(IBookRepository bookRepository)
    {
        _bookRepository = bookRepository;
    }

    public async Task<List<BookDto>> GetAllBooksAsync()
    {
        var books = await _bookRepository.GetAllBooksAsync();
        return books.Select(MapToDto).ToList();
    }

    public async Task<BookDto?> GetBookByIdAsync(int id)
    {
        var book = await _bookRepository.GetBookByIdAsync(id);
        return book == null ? null : MapToDto(book);
    }

    public async Task<BookDto> CreateBookAsync(CreateBookDto createBookDto)
    {
        var book = new Book
        {
            Title = createBookDto.Title,
            Price = createBookDto.Price,
            Authors = createBookDto.AuthorIds.Select(id => new Author { Id = id, Name = string.Empty }).ToList(),
            Genres = createBookDto.GenreIds.Select(id => new Genre { Id = id, Name = string.Empty }).ToList()
        };

        var createdBook = await _bookRepository.CreateBookAsync(book);
        return MapToDto(createdBook);
    }

    public async Task<BookDto?> UpdateBookPriceAsync(int id, UpdateBookPriceDto updateBookPriceDto)
    {
        var book = await _bookRepository.UpdateBookPriceAsync(id, updateBookPriceDto.Price);
        return book == null ? null : MapToDto(book);
    }

    public async Task<bool> DeleteBookAsync(int id)
    {
        return await _bookRepository.DeleteBookAsync(id);
    }

    private static BookDto MapToDto(Book book)
    {
        return new BookDto
        {
            Id = book.Id,
            Title = book.Title,
            Price = book.Price,
            AuthorNames = book.Authors.Select(a => a.Name).ToList(),
            GenreNames = book.Genres.Select(g => g.Name).ToList(),
            AverageReviewRating = book.Reviews.Any() 
                ? book.Reviews.Average(r => r.Rating) 
                : 0
        };
    }
}
