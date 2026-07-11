using BookstoreManagementSystem.DTOs;
using BookstoreManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookstoreManagementSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AuthorsController : ControllerBase
{
    private readonly IAuthorService _authorService;
    private readonly ILogger<AuthorsController> _logger;

    public AuthorsController(IAuthorService authorService, ILogger<AuthorsController> logger)
    {
        _authorService = authorService;
        _logger = logger;
    }

    /// <summary>
    /// Get all authors
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<AuthorDto>>> GetAllAuthors()
    {
        var authors = await _authorService.GetAllAuthorsAsync();
        return Ok(authors);
    }

    /// <summary>
    /// Get a specific author by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<AuthorDto>> GetAuthor(int id)
    {
        var author = await _authorService.GetAuthorByIdAsync(id);

        if (author == null)
        {
            var username = User.Identity?.Name ?? "Unknown";
            _logger.LogWarning("Author with Id: {AuthorId} not found for user {Username}", id, username);
            return NotFound(new { message = $"Author with ID {id} not found" });
        }

        return Ok(author);
    }

    /// <summary>
    /// Create a new author
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "ReadWrite")]
    public async Task<ActionResult<AuthorDto>> CreateAuthor([FromBody] CreateAuthorDto createAuthorDto)
    {

        if (!ModelState.IsValid)
        {
            var username = User.Identity?.Name ?? "Unknown";
            _logger.LogWarning("Invalid model state for CreateAuthor by user {Username}", username);
            return BadRequest(ModelState);
        }

        var author = await _authorService.CreateAuthorAsync(createAuthorDto);
        return CreatedAtAction(nameof(GetAuthor), new { id = author.Id }, author);
    }

    /// <summary>
    /// Delete an author (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "ReadWrite")]
    public async Task<IActionResult> DeleteAuthor(int id)
    {
        var result = await _authorService.DeleteAuthorAsync(id);

        if (!result)
        {
            var username = User.Identity?.Name ?? "Unknown";
            _logger.LogWarning("Delete failed: Author with Id: {AuthorId} not found for user {Username}", id, username);
            return NotFound(new { message = $"Author with ID {id} not found" });
        }

        return NoContent();
    }
}
