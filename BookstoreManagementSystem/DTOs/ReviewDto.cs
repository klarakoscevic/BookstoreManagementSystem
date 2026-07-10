namespace BookstoreManagementSystem.DTOs;

public class ReviewDto
{
    public int Id { get; set; }
    public int Rating { get; set; }
    public string Description { get; set; } = string.Empty;
    public int BookId { get; set; }
    public string BookTitle { get; set; } = string.Empty;
}
