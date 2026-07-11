using BookstoreManagementSystem.DTOs;
using BookstoreManagementSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookstoreManagementSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImportController : ControllerBase
{
    private readonly IBookImportService _importService;
    private readonly ILogger<ImportController> _logger;

    public ImportController(IBookImportService importService, ILogger<ImportController> logger)
    {
        _importService = importService;
        _logger = logger;
    }

    /// <summary>
    /// Manually trigger the book import process
    /// This will import 100,000 books from the simulated external API
    /// </summary>
    /// <remarks>
    /// WARNING: This operation may take several minutes to complete.
    /// It will skip books that already exist (case-insensitive title match).
    /// </remarks>
    /// <returns>Import summary with counts</returns>
    /// <response code="200">Import completed successfully</response>
    /// <response code="500">An error occurred during import</response>
    [HttpPost("trigger")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ImportResultDto>> TriggerImport()
    {
        _logger.LogInformation("Manual import triggered via API");

        try
        {
            var startTime = DateTime.UtcNow;

            await _importService.ImportBooksAsync();

            var duration = DateTime.UtcNow - startTime;

            _logger.LogInformation("Manual import completed in {Duration}ms", duration.TotalMilliseconds);

            return Ok(new ImportResultDto
            {
                Success = true,
                Message = "Import completed successfully",
                DurationSeconds = (int)duration.TotalSeconds,
                StartedAt = startTime,
                CompletedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Manual import failed");
            return StatusCode(500, new ImportResultDto
            {
                Success = false,
                Message = $"Import failed: {ex.Message}",
                StartedAt = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Get the count of books currently in the database
    /// </summary>
    /// <remarks>
    /// Use this endpoint to check how many books exist before/after import.
    /// </remarks>
    /// <returns>Database statistics</returns>
    [HttpGet("stats")]
    public async Task<ActionResult<DbStatsDto>> GetStats()
    {
        var bookCount = await _importService.GetBookCountAsync();
        var authorCount = await _importService.GetAuthorCountAsync();
        var genreCount = await _importService.GetGenreCountAsync();

        return Ok(new DbStatsDto
        {
            TotalBooks = bookCount,
            TotalAuthors = authorCount,
            TotalGenres = genreCount
        });
    }    
}
