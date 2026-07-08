namespace BookstoreManagementSystem.Models;

public class Book
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Author> Authors { get; set; } = new List<Author>();
    public ICollection<Genre> Genres { get; set; } = new List<Genre>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
