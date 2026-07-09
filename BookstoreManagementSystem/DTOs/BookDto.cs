namespace BookstoreManagementSystem.DTOs;

public class BookDto
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public decimal Price { get; set; }
    public List<string> AuthorNames { get; set; } = new();
    public List<string> GenreNames { get; set; } = new();
    public double AverageReviewRating { get; set; }
}
