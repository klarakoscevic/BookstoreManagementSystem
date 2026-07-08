using System.ComponentModel.DataAnnotations;

namespace BookstoreManagementSystem.DTOs;

public class CreateGenreDto
{
    [Required]
    [MaxLength(100)]
    public required string Name { get; set; }
}
