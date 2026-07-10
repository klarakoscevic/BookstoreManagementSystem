namespace BookstoreManagementSystem.DTOs;

public class TopBookDto
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public decimal Price { get; set; }
    public double AverageReviewRating { get; set; }
    public string AuthorNames { get; set; } = string.Empty;
    public string GenreNames { get; set; } = string.Empty;
}
