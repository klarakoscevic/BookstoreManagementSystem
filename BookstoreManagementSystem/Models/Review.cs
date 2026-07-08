namespace BookstoreManagementSystem.Models;

public class Review
{
    public int Id { get; set; }
    public int Rating { get; set; } // 1-5
    public required string Description { get; set; }
    public bool IsActive { get; set; } = true;

    public int BookId { get; set; }
    public Book Book { get; set; } = null!;
}
