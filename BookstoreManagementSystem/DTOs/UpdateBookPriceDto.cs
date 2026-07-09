using System.ComponentModel.DataAnnotations;

namespace BookstoreManagementSystem.DTOs;

public class UpdateBookPriceDto
{
    [Required]
    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }
}
