using System.ComponentModel.DataAnnotations;

namespace BookstoreManagementSystem.DTOs;

public class CreateReviewDto
{
    [Required]
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
    public int Rating { get; set; }

    [Required]
    [StringLength(1000, MinimumLength = 1)]
    public required string Description { get; set; }

    [Required]
    public int BookId { get; set; }
}
