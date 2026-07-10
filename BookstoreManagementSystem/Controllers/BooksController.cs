using BookstoreManagementSystem.DTOs;
using BookstoreManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookstoreManagementSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // All endpoints require authentication
public class BooksController : ControllerBase
{
    private readonly IBookService _bookService;

    public BooksController(IBookService bookService)
    {
        _bookService = bookService;
    }

    /// <summary>
    /// Get all books with their details
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<BookDto>>> GetAllBooks()
    {
        var books = await _bookService.GetAllBooksAsync();
        return Ok(books);
    }

    /// <summary>
    /// Get top 10 books by average rating (using raw SQL)
    /// </summary>
    [HttpGet("top10")]
    public async Task<ActionResult<List<TopBookDto>>> GetTop10Books()
    {
        var books = await _bookService.GetTop10BooksByRatingAsync();
        return Ok(books);
    }

    /// <summary>
    /// Get a specific book by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<BookDto>> GetBook(int id)
    {
        var book = await _bookService.GetBookByIdAsync(id);

        if (book == null)
            return NotFound(new { message = $"Book with ID {id} not found" });

        return Ok(book);
    }

    /// <summary>
    /// Create a new book
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "ReadWrite")]
    public async Task<ActionResult<BookDto>> CreateBook([FromBody] CreateBookDto createBookDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var book = await _bookService.CreateBookAsync(createBookDto);
        return CreatedAtAction(nameof(GetBook), new { id = book.Id }, book);
    }

    /// <summary>
    /// Update book price only
    /// </summary>
    [HttpPut("{id}/price")]
    [Authorize(Roles = "ReadWrite")]
    public async Task<ActionResult<BookDto>> UpdateBookPrice(int id, [FromBody] UpdateBookPriceDto updateBookPriceDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var book = await _bookService.UpdateBookPriceAsync(id, updateBookPriceDto);

        if (book == null)
            return NotFound(new { message = $"Book with ID {id} not found" });

        return Ok(book);
    }

    /// <summary>
    /// Delete a book (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "ReadWrite")]
    public async Task<IActionResult> DeleteBook(int id)
    {
        var result = await _bookService.DeleteBookAsync(id);

        if (!result)
            return NotFound(new { message = $"Book with ID {id} not found" });

        return NoContent();
    }
}
