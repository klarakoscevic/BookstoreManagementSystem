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

    public GenresController(IGenreService genreService)
    {
        _genreService = genreService;
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
            return NotFound(new { message = $"Genre with ID {id} not found" });

        return Ok(genre);
    }

    /// <summary>
    /// Create a new genre
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "ReadWrite")]
    public async Task<ActionResult<GenreDto>> CreateGenre([FromBody] CreateGenreDto createGenreDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

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
        var result = await _genreService.DeleteGenreAsync(id);

        if (!result)
            return NotFound(new { message = $"Genre with ID {id} not found" });

        return NoContent();
    }
}
