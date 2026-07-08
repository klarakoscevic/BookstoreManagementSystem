using BookstoreManagementSystem.Data;
using BookstoreManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace BookstoreManagementSystem.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var context = new BookstoreDbContext(
            serviceProvider.GetRequiredService<DbContextOptions<BookstoreDbContext>>());

        // Check if data already exists
        if (await context.Books.AnyAsync() || await context.Authors.AnyAsync())
        {
            return; // Database has been seeded
        }

        // Seed Genres first
        var genres = new Genre[]
        {
            new() { Name = "Fantasy" },
            new() { Name = "Science Fiction" },
            new() { Name = "Mystery" },
            new() { Name = "Horror" },
            new() { Name = "Adventure" }
        };
        context.Genres.AddRange(genres);
        await context.SaveChangesAsync();

        // Seed Authors second
        var authors = new Author[]
        {
            new() { Name = "J.K. Rowling", YearOfBirth = 1965 },
            new() { Name = "George R.R. Martin", YearOfBirth = 1948 },
            new() { Name = "J.R.R. Tolkien", YearOfBirth = 1892 },
            new() { Name = "Stephen King", YearOfBirth = 1947 },
            new() { Name = "Agatha Christie", YearOfBirth = 1890 }
        };
        context.Authors.AddRange(authors);
        await context.SaveChangesAsync();

        // Seed Books
        var books = new Book[]
        {
            new()
            {
                Title = "Harry Potter and the Philosopher's Stone",
                Price = 19.99m,
                Authors = new List<Author> { authors[0] },
                Genres = new List<Genre> { genres[0], genres[4] }
            },
            new()
            {
                Title = "A Game of Thrones",
                Price = 24.99m,
                Authors = new List<Author> { authors[1] },
                Genres = new List<Genre> { genres[0] }
            },
            new()
            {
                Title = "The Hobbit",
                Price = 14.99m,
                Authors = new List<Author> { authors[2] },
                Genres = new List<Genre> { genres[0], genres[4] }
            },
            new()
            {
                Title = "The Shining",
                Price = 16.99m,
                Authors = new List<Author> { authors[3] },
                Genres = new List<Genre> { genres[3] }
            },
            new()
            {
                Title = "Murder on the Orient Express",
                Price = 12.99m,
                Authors = new List<Author> { authors[4] },
                Genres = new List<Genre> { genres[2] }
            }
        };
        context.Books.AddRange(books);
        await context.SaveChangesAsync();

        // Seed Reviews
        var reviews = new Review[]
        {
            new() { BookId = books[0].Id, Rating = 5, Description = "Absolutely magical!" },
            new() { BookId = books[0].Id, Rating = 5, Description = "A masterpiece of fantasy literature" },
            new() { BookId = books[0].Id, Rating = 4, Description = "Great start to an amazing series" },

            new() { BookId = books[1].Id, Rating = 5, Description = "Epic and immersive" },
            new() { BookId = books[1].Id, Rating = 4, Description = "Complex characters and plot" },

            new() { BookId = books[2].Id, Rating = 5, Description = "A timeless classic" },
            new() { BookId = books[2].Id, Rating = 5, Description = "Perfect adventure story" },

            new() { BookId = books[3].Id, Rating = 5, Description = "Terrifying and brilliant" },
            new() { BookId = books[3].Id, Rating = 4, Description = "King at his best" },

            new() { BookId = books[4].Id, Rating = 5, Description = "Classic mystery perfection" }
        };
        context.Reviews.AddRange(reviews);
        await context.SaveChangesAsync();
    }
}
