using BookstoreManagementSystem.DTOs;
using BookstoreManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookstoreManagementSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GenresController : ControllerBase
{
    private readonly IGenreService _genreService;
    private readonly ILogger<GenresController> _logger;

    public GenresController(IGenreService genreService, ILogger<GenresController> logger)
    {
        _genreService = genreService;
        _logger = logger;
    }

    /// <summary>
    /// Get all genres
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<GenreDto>>> GetAllGenres()
    {
        var genres = await _genreService.GetAllGenresAsync();
        return Ok(genres);
    }

    /// <summary>
    /// Get a specific genre by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<GenreDto>> GetGenre(int id)
    {
        var genre = await _genreService.GetGenreByIdAsync(id);

        if (genre == null)
        {
            var username = User.Identity?.Name ?? "Unknown";
            _logger.LogWarning("Genre with Id: {GenreId} not found for user {Username}", id, username);
            return NotFound(new { message = $"Genre with ID {id} not found" });
        }

        return Ok(genre);
    }

    /// <summary>
    /// Create a new genre
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "ReadWrite")]
    public async Task<ActionResult<GenreDto>> CreateGenre([FromBody] CreateGenreDto createGenreDto)
    {
        var username = User.Identity?.Name ?? "Unknown";

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state for CreateGenre by user {Username}", username);
            return BadRequest(ModelState);
        }

        _logger.LogInformation("User {Username} creating new genre: {GenreName}", username, createGenreDto.Name);
        var genre = await _genreService.CreateGenreAsync(createGenreDto);
        return CreatedAtAction(nameof(GetGenre), new { id = genre.Id }, genre);
    }

    /// <summary>
    /// Delete a genre (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "ReadWrite")]
    public async Task<IActionResult> DeleteGenre(int id)
    {
        var username = User.Identity?.Name ?? "Unknown";

        var result = await _genreService.DeleteGenreAsync(id);

        if (!result)
        {
            _logger.LogWarning("Delete failed: Genre with Id: {GenreId} not found for user {Username}", id, username);
            return NotFound(new { message = $"Genre with ID {id} not found" });
        }

        _logger.LogInformation("User {Username} successfully deleted genre Id: {GenreId}", username, id);
        return NoContent();
    }
}
