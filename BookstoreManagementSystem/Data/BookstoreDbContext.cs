using BookstoreManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace BookstoreManagementSystem.Data;

public class BookstoreDbContext : DbContext
{
    public BookstoreDbContext(DbContextOptions<BookstoreDbContext> options)
        : base(options)
    {
    }

    public DbSet<Book> Books { get; set; }
    public DbSet<Author> Authors { get; set; }
    public DbSet<Genre> Genres { get; set; }
    public DbSet<Review> Reviews { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Book entity configuration
        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.Property(b => b.Title).IsRequired().HasMaxLength(500);
            entity.Property(b => b.Price).HasColumnType("decimal(18,2)");
            entity.HasQueryFilter(b => b.IsActive);

            // Many-to-many: Books <-> Authors
            entity.HasMany(b => b.Authors)
                  .WithMany(a => a.Books)
                  .UsingEntity(j => j.ToTable("BookAuthors"));

            // Many-to-many: Books <-> Genres
            entity.HasMany(b => b.Genres)
                  .WithMany(g => g.Books)
                  .UsingEntity(j => j.ToTable("BookGenres"));
        });

        // Author entity configuration
        modelBuilder.Entity<Author>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Name).IsRequired().HasMaxLength(200);
            entity.Property(a => a.YearOfBirth).IsRequired();
            entity.HasQueryFilter(a => a.IsActive);
        });

        // Genre entity configuration
        modelBuilder.Entity<Genre>(entity =>
        {
            entity.HasKey(g => g.Id);
            entity.Property(g => g.Name).IsRequired().HasMaxLength(100);
            entity.HasQueryFilter(g => g.IsActive);
        });

        // Review entity configuration
        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Rating).IsRequired();
            entity.Property(r => r.Description).IsRequired().HasMaxLength(2000);
            entity.HasQueryFilter(r => r.IsActive);

            // One-to-many: Book -> Reviews
            entity.HasOne(r => r.Book)
                  .WithMany(b => b.Reviews)
                  .HasForeignKey(r => r.BookId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }

    public override int SaveChanges()
    {
        UpdateSoftDeleteStatuses();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateSoftDeleteStatuses();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateSoftDeleteStatuses()
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.CurrentValues["IsActive"] = false;
            }
        }
    }
}
