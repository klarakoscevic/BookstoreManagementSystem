namespace BookstoreManagementSystem.DTOs;

public class AuthorDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int YearOfBirth { get; set; }
}
