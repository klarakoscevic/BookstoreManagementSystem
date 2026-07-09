using System.ComponentModel.DataAnnotations;

namespace BookstoreManagementSystem.DTOs;

public class CreateBookDto
{
    [Required]
    [MaxLength(500)]
    public required string Title { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    public List<int> AuthorIds { get; set; } = new();
    public List<int> GenreIds { get; set; } = new();
}
