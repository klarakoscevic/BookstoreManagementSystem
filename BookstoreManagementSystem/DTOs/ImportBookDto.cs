namespace BookstoreManagementSystem.DTOs;

public class ImportBookDto
{
    public required string Title { get; set; }
    public decimal Price { get; set; }
    public List<string> Authors { get; set; } = new();
    public List<string> Genres { get; set; } = new();
}
