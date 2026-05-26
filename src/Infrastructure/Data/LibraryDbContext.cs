// LibraryApp.Infrastructure/Data/LibraryDbContext.cs

using LibraryApp.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace LibraryApp.Infrastructure.Data;

public sealed class LibraryDbContext(DbContextOptions<LibraryDbContext> options)
    : DbContext(options)
{
    public DbSet<Book> Books => Set<Book>();
    public DbSet<Author> Authors => Set<Author>();
    public DbSet<Series> Series => Set<Series>();
    public DbSet<Genre> Genres => Set<Genre>();
    public DbSet<Language> Languages => Set<Language>();
    public DbSet<Tag> Tags => Set<Tag>();

    protected override void OnModelCreating(ModelBuilder mb)
        => mb.ApplyConfigurationsFromAssembly(typeof(LibraryDbContext).Assembly);
}