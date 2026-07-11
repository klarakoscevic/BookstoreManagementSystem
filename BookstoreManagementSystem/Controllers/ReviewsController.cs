using BookstoreManagementSystem.DTOs;
using BookstoreManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookstoreManagementSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;
    private readonly ILogger<ReviewsController> _logger;

    public ReviewsController(IReviewService reviewService, ILogger<ReviewsController> logger)
    {
        _reviewService = reviewService;
        _logger = logger;
    }

    /// <summary>
    /// Get all reviews
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<ReviewDto>>> GetAllReviews()
    {
        var reviews = await _reviewService.GetAllReviewsAsync();
        return Ok(reviews);
    }

    /// <summary>
    /// Get all reviews for a specific book
    /// </summary>
    [HttpGet("book/{bookId}")]
    public async Task<ActionResult<List<ReviewDto>>> GetReviewsByBookId(int bookId)
    {
        var reviews = await _reviewService.GetReviewsByBookIdAsync(bookId);
        return Ok(reviews);
    }

    /// <summary>
    /// Get a specific review by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ReviewDto>> GetReviewById(int id)
    {
        var review = await _reviewService.GetReviewByIdAsync(id);

        if (review == null)
        {
            var username = User.Identity?.Name ?? "Unknown";
            _logger.LogWarning("Review with Id: {ReviewId} not found for user {Username}", id, username);
            return NotFound();
        }

        return Ok(review);
    }

    /// <summary>
    /// Create a new review
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "ReadWrite")]
    public async Task<ActionResult<ReviewDto>> CreateReview([FromBody] CreateReviewDto createReviewDto)
    {
        var username = User.Identity?.Name ?? "Unknown";

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state for CreateReview by user {Username}", username);
            return BadRequest(ModelState);
        }

        var review = await _reviewService.CreateReviewAsync(createReviewDto);

        if (review == null)
        {
            _logger.LogWarning("Create review failed: Book with Id: {BookId} not found for user {Username}",
                createReviewDto.BookId, username);
            return BadRequest($"Book with ID {createReviewDto.BookId} does not exist.");
        }

        return CreatedAtAction(nameof(GetReviewById), new { id = review.Id }, review);
    }

    /// <summary>
    /// Update an existing review
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "ReadWrite")]
    public async Task<ActionResult<ReviewDto>> UpdateReview(int id, [FromBody] UpdateReviewDto updateReviewDto)
    {
        var username = User.Identity?.Name ?? "Unknown";

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state for UpdateReview by user {Username}, ReviewId: {ReviewId}",
                username, id);
            return BadRequest(ModelState);
        }

        var review = await _reviewService.UpdateReviewAsync(id, updateReviewDto);

        if (review == null)
        {
            _logger.LogWarning("Update failed: Review with Id: {ReviewId} not found for user {Username}",
                id, username);
            return NotFound();
        }

        return Ok(review);
    }

    /// <summary>
    /// Delete a review (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "ReadWrite")]
    public async Task<IActionResult> DeleteReview(int id)
    {
        var result = await _reviewService.DeleteReviewAsync(id);

        if (!result)
        {
            var username = User.Identity?.Name ?? "Unknown";
            _logger.LogWarning("Delete failed: Review with Id: {ReviewId} not found for user {Username}",
                id, username);
            return NotFound();
        }

        return NoContent();
    }
}
