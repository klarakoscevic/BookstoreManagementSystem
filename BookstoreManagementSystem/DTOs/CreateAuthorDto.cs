using System.ComponentModel.DataAnnotations;

namespace BookstoreManagementSystem.DTOs;

public class CreateAuthorDto
{
    [Required]
    [MaxLength(200)]
    public required string Name { get; set; }

    [Required]
    [Range(1000, 2100)]
    public int YearOfBirth { get; set; }
}
