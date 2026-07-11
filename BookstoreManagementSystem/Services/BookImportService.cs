using BookstoreManagementSystem.Data;
using BookstoreManagementSystem.DTOs;
using BookstoreManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace BookstoreManagementSystem.Services;

public class BookImportService : IBookImportService
{
    private readonly BookstoreDbContext _context;
    private readonly ILogger<BookImportService> _logger;

    public BookImportService(BookstoreDbContext context, ILogger<BookImportService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task ImportBooksAsync()
    {
        _logger.LogInformation("Starting book import process");

        try
        {
            var externalBooks = await FetchExternalBooksAsync();
            _logger.LogInformation("Fetched {Count} books from external source", externalBooks.Count);

            var importedCount = 0;
            var skippedCount = 0;

            // Load existing book titles, authors, and genres into memory for comparison
            // This is necessary because NormalizeTitle() can't be translated to SQL
            var existingBooksDict = await _context.Books
                .Select(b => b.Title)
                .ToListAsync();

            var normalizedExistingTitles = existingBooksDict
                .Select(t => NormalizeTitle(t))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Load existing authors and genres into memory
            var existingAuthors = await _context.Authors.ToListAsync();
            var existingGenres = await _context.Genres.ToListAsync();

            foreach (var importBook in externalBooks)
            {
                // Match by title: case-insensitive, trimmed
                // Check if book already exists in database
                var normalizedTitle = NormalizeTitle(importBook.Title);

                if (normalizedExistingTitles.Contains(normalizedTitle))
                {
                    _logger.LogDebug("Skipping existing book: {Title}", importBook.Title);
                    skippedCount++;
                    continue;
                }

                _logger.LogDebug("Creating new book: {Title}", importBook.Title);

                var newBook = new Book
                {
                    Title = importBook.Title,
                    Price = importBook.Price,
                    IsActive = true
                };

                // Match or create authors
                foreach (var authorName in importBook.Authors)
                {
                    var normalizedAuthorName = NormalizeTitle(authorName);
                    var author = existingAuthors
                        .FirstOrDefault(a => NormalizeTitle(a.Name) == normalizedAuthorName);

                    if (author == null)
                    {
                        author = new Author { Name = authorName, IsActive = true };
                        _context.Authors.Add(author);
                        existingAuthors.Add(author);
                        _logger.LogDebug("Created new author: {AuthorName}", authorName);
                    }

                    newBook.Authors.Add(author);
                }

                // Match or create genres
                foreach (var genreName in importBook.Genres)
                {
                    var normalizedGenreName = NormalizeTitle(genreName);
                    var genre = existingGenres
                        .FirstOrDefault(g => NormalizeTitle(g.Name) == normalizedGenreName);

                    if (genre == null)
                    {
                        genre = new Genre { Name = genreName, IsActive = true };
                        _context.Genres.Add(genre);
                        existingGenres.Add(genre);
                        _logger.LogDebug("Created new genre: {GenreName}", genreName);
                    }

                    newBook.Genres.Add(genre);
                }

                _context.Books.Add(newBook);
                importedCount++;
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Import completed successfully. Imported: {ImportedCount}, Skipped: {SkippedCount}",
                importedCount, skippedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during book import");
            throw;
        }
    }

    public virtual async Task<List<ImportBookDto>> FetchExternalBooksAsync()
    {
        // Simulated external API that returns 100,000+ books in one batch
        // In production, this would be a real API call
        await Task.Delay(100); // Simulate network delay

        _logger.LogInformation("Generating 100,000 mock books from simulated API");

        var books = new List<ImportBookDto>();
        var random = new Random(); 
        //var random = new Random(); // Fixed seed for reproducibility
        

        // Sample data for generation
        var titlePrefixes = new[] { "The", "A", "An" };
        var titleWords = new[]
        {
            "Great", "Lost", "Hidden", "Secret", "Ancient", "Modern", "Dark", "Bright",
            "Mystery", "Adventure", "Story", "Tale", "Legend", "Chronicles", "Journey",
            "Quest", "War", "Peace", "Love", "Death", "Life", "Time", "Space", "World"
        };
        var titleSuffixes = new[]
        {
            "Nation", "Kingdom", "Empire", "City", "Village", "Mountain", "Ocean", "Sky",
            "Forest", "Desert", "Island", "River", "Valley", "Castle", "Temple", "Tower"
        };

        var authorFirstNames = new[]
        {
            "John", "Jane", "Michael", "Sarah", "David", "Emma", "Robert", "Lisa",
            "William", "Mary", "James", "Patricia", "Richard", "Jennifer", "Thomas", "Linda"
        };
        var authorLastNames = new[]
        {
            "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis",
            "Martinez", "Hernandez", "Lopez", "Wilson", "Anderson", "Thomas", "Taylor", "Moore"
        };

        var genres = new[]
        {
            "Fiction", "Non-Fiction", "Science Fiction", "Fantasy", "Mystery", "Thriller",
            "Romance", "Horror", "Biography", "History", "Self-Help", "Business",
            "Philosophy", "Poetry", "Drama", "Adventure", "Classic", "Contemporary",
            "Romantasy"
        };

        //var randomBooks = new Random();
        //int booksImport = randomBooks.Next(100000, 200001);
        int booksImport = random.Next(100000, 200001);

        // Generate books
        for (int i = 0; i < booksImport; i++)
        {
            // Generate random title
            var prefix = random.Next(3) == 0 ? titlePrefixes[random.Next(titlePrefixes.Length)] + " " : "";
            var word1 = titleWords[random.Next(titleWords.Length)];
            var word2 = titleWords[random.Next(titleWords.Length)];
            var suffix = titleSuffixes[random.Next(titleSuffixes.Length)];
            var title = $"{prefix}{word1} {word2} {suffix}";

            // Generate random authors (1-3 per book)
            var authorCount = random.Next(1, 4);
            var authors = new List<string>();
            for (int j = 0; j < authorCount; j++)
            {
                var firstName = authorFirstNames[random.Next(authorFirstNames.Length)];
                var lastName = authorLastNames[random.Next(authorLastNames.Length)];
                authors.Add($"{firstName} {lastName}");
            }

            // Generate random genres (1-3 per book)
            var genreCount = random.Next(1, 4);
            var bookGenres = new List<string>();
            for (int j = 0; j < genreCount; j++)
            {
                var genre = genres[random.Next(genres.Length)];
                if (!bookGenres.Contains(genre))
                {
                    bookGenres.Add(genre);
                }
            }

            // Random price between $9.99 and $49.99
            var price = Math.Round((decimal)(random.NextDouble() * 40 + 9.99), 2);

            books.Add(new ImportBookDto
            {
                Title = title,
                Price = price,
                Authors = authors,
                Genres = bookGenres
            });
        }

        _logger.LogInformation("Generated {Count} mock books", books.Count);
        return books;
    }

    /// <summary>
    /// Normalizes title for matching - handles case insensitivity and whitespace
    /// This implements exact matching (case-insensitive, trimmed) as per requirements
    /// 
    /// TYPO HANDLING APPROACH:
    /// 
    /// Current Implementation (Simple):
    /// - Case-insensitive matching
    /// - Trimming whitespace
    /// - Collapsing multiple spaces into single spaces
    /// - This handles common formatting differences like:
    ///   * "Crime and Punishment" matches "crime and punishment"
    ///   * "Crime  and  Punishment" matches "Crime and Punishment"
    ///   * "  Crime and Punishment  " matches "Crime and Punishment"
    /// 
    /// LIMITATION: Character-level typos NOT handled by current implementation
    /// - Example: "Criem and Punishment" will NOT match "Crime and Punishment"
    /// - This is by design for the exact-match requirement
    /// 
    /// Advanced Typo Handling (Future Enhancement):
    /// For handling actual character typos like "Criem" vs "Crime", consider:
    /// 
    /// 1. LEVENSHTEIN DISTANCE:
    ///    - Measures number of single-character edits needed
    ///    - Example: "Criem" -> "Crime" = 2 edits
    ///    - Set threshold (e.g., distance <= 2 for titles under 20 chars)
    ///    - Library: FuzzySharp NuGet package
    ///    - Code example:
    ///      var distance = Levenshtein.Distance(title1, title2);
    ///      if (distance <= 2) { /* potential match */ }
    /// 
    /// 2. JARO-WINKLER DISTANCE:
    ///    - Better for shorter strings and transpositions
    ///    - Returns similarity score (0-1)
    ///    - Set threshold (e.g., >= 0.9 for high confidence)
    ///    - Library: FuzzySharp or SimMetrics
    /// 
    /// 3. PHONETIC MATCHING:
    ///    - Soundex, Metaphone, or Double Metaphone algorithms
    ///    - Matches words that sound similar
    ///    - Good for names: "Stephen" vs "Steven"
    ///    - Less useful for book titles
    /// 
    /// 4. N-GRAM SIMILARITY:
    ///    - Break titles into character sequences (bigrams, trigrams)
    ///    - Calculate Jaccard or Cosine similarity
    ///    - Good for longer titles with multiple word variations
    /// 
    /// 5. WEIGHTED COMBINATION:
    ///    - Combine multiple algorithms with weights
    ///    - Example: 60% Levenshtein + 40% token sorting
    ///    - Reduces false positives
    /// 
    /// 6. PERFORMANCE OPTIMIZATION FOR 100K+ BOOKS:
    ///    - Pre-compute normalized titles and store in database with index
    ///    - Use SQL Server full-text search or Elasticsearch
    ///    - Implement caching with MemoryCache for frequent lookups
    ///    - Batch processing with parallel queries
    ///    - Use BK-Tree data structure for fast fuzzy search
    ///    - Consider bloom filters to quickly eliminate non-matches
    /// 
    /// Implementation Strategy for Production:
    /// - Phase 1: Use database index on normalized title for fast exact match
    /// - Phase 2: For non-matches, query "similar" titles using SQL LIKE or full-text
    /// - Phase 3: Apply fuzzy matching in-memory on candidate set (< 100 books)
    /// - Phase 4: Manual review queue for similarity score 0.85-0.95
    /// - Phase 5: Auto-accept matches with score >= 0.95
    ///     
    /// </summary>
   
    private string NormalizeTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return string.Empty;

        // Convert to lowercase, trim, and collapse multiple spaces
        // This handles exact matching with case-insensitivity and whitespace normalization
        return string.Join(" ", title.ToLowerInvariant()
            .Trim()
            .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
    }

    public async Task<int> GetBookCountAsync()
    {
        return await _context.Books.CountAsync();
    }

    public async Task<int> GetAuthorCountAsync()
    {
        return await _context.Authors.CountAsync();
    }

    public async Task<int> GetGenreCountAsync()
    {
        return await _context.Genres.CountAsync();
    }
}
