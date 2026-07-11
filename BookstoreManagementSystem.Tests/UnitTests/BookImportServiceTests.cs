using BookstoreManagementSystem.Data;
using BookstoreManagementSystem.DTOs;
using BookstoreManagementSystem.Models;
using BookstoreManagementSystem.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace BookstoreManagementSystem.Tests.UnitTests;

public class BookImportServiceTests : IDisposable
{
    private readonly BookstoreDbContext _context;
    private readonly Mock<ILogger<BookImportService>> _mockLogger;
    private readonly BookImportService _importService;

    public BookImportServiceTests()
    {
        // Use in-memory database for testing
        var options = new DbContextOptionsBuilder<BookstoreDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new BookstoreDbContext(options);
        _mockLogger = new Mock<ILogger<BookImportService>>();
        _importService = new BookImportService(_context, _mockLogger.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }  

    [Fact]
    public async Task FetchExternalBooksAsync_AllBooksShouldHaveRequiredProperties()
    {
        // Act
        var result = await _importService.FetchExternalBooksAsync();

        // Assert
        result.Should().AllSatisfy(book =>
        {
            book.Title.Should().NotBeNullOrWhiteSpace();
            book.Price.Should().BeGreaterThan(0);
            book.Authors.Should().NotBeEmpty();
            book.Genres.Should().NotBeEmpty();
        });
    }

    [Fact]
    public async Task ImportBooksAsync_WithEmptyDatabase_ShouldImportAllBooks()
    {
        // Arrange - Mock the fetch method to return test data
        var mockService = new Mock<BookImportService>(_context, _mockLogger.Object) { CallBase = true };
        mockService.Setup(s => s.FetchExternalBooksAsync())
            .ReturnsAsync(GetTestBooks());

        // Act
        await mockService.Object.ImportBooksAsync();

        // Assert
        var booksInDb = await _context.Books.CountAsync();
        booksInDb.Should().Be(4); // Should import all 4 test books
    }

    [Fact]
    public async Task ImportBooksAsync_WithExistingBooks_ShouldSkipDuplicates()
    {
        // Arrange - Add one book to database
        var existingBook = new Book
        {
            Title = "Crime and Punishment",
            Price = 19.99m,
            IsActive = true
        };
        _context.Books.Add(existingBook);
        await _context.SaveChangesAsync();

        var mockService = new Mock<BookImportService>(_context, _mockLogger.Object) { CallBase = true };
        mockService.Setup(s => s.FetchExternalBooksAsync())
            .ReturnsAsync(GetTestBooks());

        // Act
        await mockService.Object.ImportBooksAsync();

        // Assert
        var booksInDb = await _context.Books.ToListAsync();
        booksInDb.Should().HaveCount(3); // 1 existing + 2 new ("Crime and Punishment" and "Crime  and" both match, skip both)

        // Original book should not be modified
        var originalBook = booksInDb.First(b => b.Id == existingBook.Id);
        originalBook.Price.Should().Be(19.99m); // Price not updated
    }

    [Fact]
    public async Task ImportBooksAsync_ShouldMatchTitlesCaseInsensitively()
    {
        // Arrange - Add book with different casing
        var existingBook = new Book
        {
            Title = "CRIME AND PUNISHMENT",
            Price = 15.99m,
            IsActive = true
        };
        _context.Books.Add(existingBook);
        await _context.SaveChangesAsync();

        var mockService = new Mock<BookImportService>(_context, _mockLogger.Object) { CallBase = true };
        mockService.Setup(s => s.FetchExternalBooksAsync())
            .ReturnsAsync(GetTestBooks());

        // Act
        await mockService.Object.ImportBooksAsync();

        // Assert
        var booksInDb = await _context.Books.ToListAsync();
        booksInDb.Should().HaveCount(3); // Should skip "Crime and Punishment" and "Crime  and Punishment" (extra space) due to case-insensitive match
    }

    [Fact]
    public async Task ImportBooksAsync_ShouldMatchTitlesWithExtraSpaces()
    {
        // Arrange - Add book to match "Crime  and Punishment" (with extra space in import)
        var existingBook = new Book
        {
            Title = "Crime and Punishment",
            Price = 12.99m,
            IsActive = true
        };
        _context.Books.Add(existingBook);
        await _context.SaveChangesAsync();

        var mockService = new Mock<BookImportService>(_context, _mockLogger.Object) { CallBase = true };
        mockService.Setup(s => s.FetchExternalBooksAsync())
            .ReturnsAsync(GetTestBooks());

        // Act
        await mockService.Object.ImportBooksAsync();

        // Assert
        var booksInDb = await _context.Books.ToListAsync();
        // "Crime  and Punishment" should match existing "Crime and Punishment"
        booksInDb.Should().HaveCount(3); // 1 existing + 2 new (1 matched and skipped, "Criem" is separate)
    }

    [Fact]
    public async Task ImportBooksAsync_ShouldNotMatchCharacterTypos()
    {
        // Arrange - Add correct spelling
        var existingBook = new Book
        {
            Title = "Crime and Punishment",
            Price = 14.99m,
            IsActive = true
        };
        _context.Books.Add(existingBook);
        await _context.SaveChangesAsync();

        var mockService = new Mock<BookImportService>(_context, _mockLogger.Object) { CallBase = true };
        mockService.Setup(s => s.FetchExternalBooksAsync())
            .ReturnsAsync(GetTestBooks());

        // Act
        await mockService.Object.ImportBooksAsync();

        // Assert
        var booksInDb = await _context.Books.ToListAsync();

        // "Criem and Punishment" should NOT match and be imported as new book
        booksInDb.Should().HaveCount(3); // 1 existing + 2 new ("Criem" and "War and Peace", "Crime  and" skipped)

        booksInDb.Should().ContainSingle(b => b.Title == "Crime and Punishment");
        booksInDb.Should().ContainSingle(b => b.Title == "Criem and Punishment");
    }

    [Fact]
    public async Task ImportBooksAsync_ShouldCreateNewAuthors()
    {
        // Arrange
        var mockService = new Mock<BookImportService>(_context, _mockLogger.Object) { CallBase = true };
        mockService.Setup(s => s.FetchExternalBooksAsync())
            .ReturnsAsync(GetTestBooks());

        // Act
        await mockService.Object.ImportBooksAsync();

        // Assert
        var authorsInDb = await _context.Authors.ToListAsync();
        authorsInDb.Should().NotBeEmpty();
        authorsInDb.Should().Contain(a => a.Name == "Fyodor Dostoevsky");
    }

    [Fact]
    public async Task ImportBooksAsync_ShouldReuseExistingAuthors()
    {
        // Arrange - Add existing author
        var existingAuthor = new Author
        {
            Name = "Fyodor Dostoevsky",
            YearOfBirth = 1821,
            IsActive = true
        };
        _context.Authors.Add(existingAuthor);
        await _context.SaveChangesAsync();

        var mockService = new Mock<BookImportService>(_context, _mockLogger.Object) { CallBase = true };
        mockService.Setup(s => s.FetchExternalBooksAsync())
            .ReturnsAsync(GetTestBooks());

        // Act
        await mockService.Object.ImportBooksAsync();

        // Assert
        var authorsInDb = await _context.Authors.Where(a => a.Name == "Fyodor Dostoevsky").ToListAsync();
        authorsInDb.Should().HaveCount(1); // Should not create duplicate
    }

    [Fact]
    public async Task ImportBooksAsync_ShouldMatchAuthorsCaseInsensitively()
    {
        // Arrange - Add author with different casing
        var existingAuthor = new Author
        {
            Name = "FYODOR DOSTOEVSKY", // Uppercase
            YearOfBirth = 1821,
            IsActive = true
        };
        _context.Authors.Add(existingAuthor);
        await _context.SaveChangesAsync();

        var mockService = new Mock<BookImportService>(_context, _mockLogger.Object) { CallBase = true };
        mockService.Setup(s => s.FetchExternalBooksAsync())
            .ReturnsAsync(GetTestBooks());

        // Act
        await mockService.Object.ImportBooksAsync();

        // Assert
        var dostoevskys = await _context.Authors
            .Where(a => a.Name.ToLower() == "fyodor dostoevsky")
            .ToListAsync();
        dostoevskys.Should().HaveCount(1); // Should reuse existing author
    }

    [Fact]
    public async Task ImportBooksAsync_ShouldCreateNewGenres()
    {
        // Arrange
        var mockService = new Mock<BookImportService>(_context, _mockLogger.Object) { CallBase = true };
        mockService.Setup(s => s.FetchExternalBooksAsync())
            .ReturnsAsync(GetTestBooks());

        // Act
        await mockService.Object.ImportBooksAsync();

        // Assert
        var genresInDb = await _context.Genres.ToListAsync();
        genresInDb.Should().NotBeEmpty();
        genresInDb.Should().Contain(g => g.Name == "Classic");
    }

    [Fact]
    public async Task ImportBooksAsync_ShouldReuseExistingGenres()
    {
        // Arrange - Add existing genre
        var existingGenre = new Genre
        {
            Name = "Classic",
            IsActive = true
        };
        _context.Genres.Add(existingGenre);
        await _context.SaveChangesAsync();

        var mockService = new Mock<BookImportService>(_context, _mockLogger.Object) { CallBase = true };
        mockService.Setup(s => s.FetchExternalBooksAsync())
            .ReturnsAsync(GetTestBooks());

        // Act
        await mockService.Object.ImportBooksAsync();

        // Assert
        var classicGenres = await _context.Genres.Where(g => g.Name == "Classic").ToListAsync();
        classicGenres.Should().HaveCount(1); // Should not create duplicate
    }

    [Fact]
    public async Task ImportBooksAsync_ShouldAssociateAuthorsWithBooks()
    {
        // Arrange
        var mockService = new Mock<BookImportService>(_context, _mockLogger.Object) { CallBase = true };
        mockService.Setup(s => s.FetchExternalBooksAsync())
            .ReturnsAsync(GetTestBooks());

        // Act
        await mockService.Object.ImportBooksAsync();

        // Assert
        var crimeAndPunishment = await _context.Books
            .Include(b => b.Authors)
            .FirstOrDefaultAsync(b => b.Title == "Crime and Punishment");

        crimeAndPunishment.Should().NotBeNull();
        crimeAndPunishment!.Authors.Should().HaveCount(1);
        crimeAndPunishment.Authors.First().Name.Should().Be("Fyodor Dostoevsky");
    }

    [Fact]
    public async Task ImportBooksAsync_ShouldAssociateGenresWithBooks()
    {
        // Arrange
        var mockService = new Mock<BookImportService>(_context, _mockLogger.Object) { CallBase = true };
        mockService.Setup(s => s.FetchExternalBooksAsync())
            .ReturnsAsync(GetTestBooks());

        // Act
        await mockService.Object.ImportBooksAsync();

        // Assert
        var crimeAndPunishment = await _context.Books
            .Include(b => b.Genres)
            .FirstOrDefaultAsync(b => b.Title == "Crime and Punishment");

        crimeAndPunishment.Should().NotBeNull();
        crimeAndPunishment!.Genres.Should().NotBeEmpty();
        crimeAndPunishment.Genres.Should().Contain(g => g.Name == "Classic");
    }

    [Fact]
    public async Task ImportBooksAsync_ShouldHandleWhitespaceInTitles()
    {
        // Arrange - Add book with extra spaces in database
        var existingBook = new Book
        {
            Title = "  War   and   Peace  ", // Multiple spaces
            Price = 20.99m,
            IsActive = true
        };
        _context.Books.Add(existingBook);
        await _context.SaveChangesAsync();

        var mockService = new Mock<BookImportService>(_context, _mockLogger.Object) { CallBase = true };
        mockService.Setup(s => s.FetchExternalBooksAsync())
            .ReturnsAsync(GetTestBooks());

        // Act
        await mockService.Object.ImportBooksAsync();

        // Assert
        var booksInDb = await _context.Books.ToListAsync();
        // Both "War and Peace" and "War  and  Peace" should match existing book
        var warAndPeaceBooks = booksInDb.Where(b => 
            b.Title.Replace(" ", "").ToLower() == "warandpeace").ToList();

        // Should only have 1 book (the original) since both imports match and are skipped
        warAndPeaceBooks.Should().HaveCount(1);
    }

    [Fact]
    public async Task ImportBooksAsync_ShouldSetBooksAsActive()
    {
        // Arrange
        var mockService = new Mock<BookImportService>(_context, _mockLogger.Object) { CallBase = true };
        mockService.Setup(s => s.FetchExternalBooksAsync())
            .ReturnsAsync(GetTestBooks());

        // Act
        await mockService.Object.ImportBooksAsync();

        // Assert
        var books = await _context.Books.ToListAsync();
        books.Should().AllSatisfy(book => book.IsActive.Should().BeTrue());
    }

    private static List<ImportBookDto> GetTestBooks()
    {
        return new List<ImportBookDto>
        {
            new ImportBookDto
            {
                Title = "Crime and Punishment",
                Price = 14.99m,
                Authors = new List<string> { "Fyodor Dostoevsky" },
                Genres = new List<string> { "Classic", "Fiction" }
            },
            new ImportBookDto
            {
                Title = "Criem and Punishment", // Character typo
                Price = 14.99m,
                Authors = new List<string> { "Fyodor Dostoevsky" },
                Genres = new List<string> { "Classic" }
            },
            new ImportBookDto
            {
                Title = "Crime  and Punishment", // Extra space
                Price = 15.99m,
                Authors = new List<string> { "Fyodor Dostoevsky" },
                Genres = new List<string> { "Classic" }
            },
            new ImportBookDto
            {
                Title = "War and Peace",
                Price = 18.99m,
                Authors = new List<string> { "Leo Tolstoy" },
                Genres = new List<string> { "Classic", "Historical Fiction" }
            }
        };
    }
}
