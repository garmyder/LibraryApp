// src/Infrastructure/Repositories/BookRepository.cs
using LibraryApp.Core.Domain;
using LibraryApp.Core.Interfaces.Repositories;
using LibraryApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LibraryApp.Infrastructure.Repositories;

internal sealed class BookRepository(LibraryDbContext db)
    : EfRepository<Book>(db), IBookRepository
{
    /// <summary>Base query with all navigation properties loaded.</summary>
    private IQueryable<Book> WithRelations => Db.Books
        .Include(b => b.Authors)
        .Include(b => b.Series)
        .Include(b => b.Genre)
        .Include(b => b.Language)
        .Include(b => b.Tags);

    /// <inheritdoc/>
    public async Task<IReadOnlyList<BookWithRelations>> GetByAuthorAsync(
        long authorId, CancellationToken ct = default)
    {
        var books = await WithRelations
            .Where(b => b.Authors.Any(a => a.AuthorId == authorId))
            .OrderBy(b => b.SeriesId)
            .ThenBy(b => b.SeriesNumber)
            .ThenBy(b => b.Title)
            .ToListAsync(ct);

        return books.ConvertAll(ToRelations);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<BookWithRelations>> SearchAsync(
        string query, CancellationToken ct = default)
    {
        var q = query.Trim();

        var books = await WithRelations
            .Where(b => EF.Functions.Like(b.Title, $"%{q}%") ||
                        b.Authors.Any(a =>
                            EF.Functions.Like(a.LastName, $"%{q}%") ||
                            EF.Functions.Like(a.FirstName, $"%{q}%")))
            .OrderBy(b => b.Title)
            .ToListAsync(ct);

        return books.ConvertAll(ToRelations);
    }
    
    // /// <inheritdoc/>
    // public async Task<IReadOnlyList<BookWithRelations>> GetPagedAsync(
    //     int page, int pageSize, CancellationToken ct = default)
    // {
    //     ArgumentOutOfRangeException.ThrowIfLessThan(page, 1);
    //     ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);
    //
    //     var books = await WithRelations
    //         .OrderBy(b => b.Title)
    //         .Skip((page - 1) * pageSize)
    //         .Take(pageSize)
    //         .ToListAsync(ct);
    //
    //     return books.ConvertAll(ToRelations);
    // }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Book>> GetByIdsAsync(
        IReadOnlyList<long> ids, CancellationToken ct = default) =>
        await Db.Books
            .Where(b => ids.Contains(b.BookId))
            .ToListAsync(ct);

    /// <inheritdoc/>
    public async Task DeleteRangeAsync(
        IReadOnlyList<long> ids, CancellationToken ct = default)
    {
        await Db.Books
            .Where(b => ids.Contains(b.BookId))
            .ExecuteDeleteAsync(ct);
    }
    
    private static BookWithRelations ToRelations(Book b)
        => new(b, b.Authors.ToList(), b.Series);
}