using System.ComponentModel.DataAnnotations;

namespace BookstoreManagementSystem.DTOs;

public class UpdateGenreDto
{
    [Required]
    [MaxLength(100)]
    public required string Name { get; set; }
}
