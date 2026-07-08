namespace BookstoreManagementSystem.Models;

public class Genre
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Book> Books { get; set; } = new List<Book>();
}
