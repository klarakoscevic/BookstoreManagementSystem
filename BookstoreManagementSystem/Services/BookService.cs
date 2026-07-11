using BookstoreManagementSystem.DTOs;
using BookstoreManagementSystem.Models;
using BookstoreManagementSystem.Repositories;

namespace BookstoreManagementSystem.Services;

public class BookService : IBookService
{
    private readonly IBookRepository _bookRepository;
    private readonly ILogger<BookService> _logger;

    public BookService(IBookRepository bookRepository, ILogger<BookService> logger)
    {
        _bookRepository = bookRepository;
        _logger = logger;
    }

    public async Task<List<BookDto>> GetAllBooksAsync()
    {
        var books = await _bookRepository.GetAllBooksAsync();
        return books.Select(MapToDto).ToList();
    }

    public async Task<BookDto?> GetBookByIdAsync(int id)
    {
        var book = await _bookRepository.GetBookByIdAsync(id);

        if (book == null)
        {
            _logger.LogWarning("Book with Id: {BookId} not found", id);
            return null;
        }

        return MapToDto(book);
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

        if (book == null)
        {
            _logger.LogWarning("Failed to update book price: Book with Id: {BookId} not found", id);
            return null;
        }

        return MapToDto(book);
    }

    public async Task<bool> DeleteBookAsync(int id)
    {
        var result = await _bookRepository.DeleteBookAsync(id);

        if (!result)
        {
            _logger.LogWarning("Failed to delete book: Book with Id: {BookId} not found", id);
        }

        return result;
    }

    public async Task<List<TopBookDto>> GetTop10BooksByRatingAsync()
    {
        return await _bookRepository.GetTop10BooksByRatingAsync();
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
