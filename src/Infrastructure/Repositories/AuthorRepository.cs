// src/Infrastructure/Repositories/AuthorRepository.cs
using LibraryApp.Core.Domain;
using LibraryApp.Core.Interfaces.Repositories;
using LibraryApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LibraryApp.Infrastructure.Repositories;

internal sealed class AuthorRepository(LibraryDbContext db)
    : EfRepository<Author>(db), IAuthorRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<Author>> GetAllWithBookCountAsync(CancellationToken ct = default)
        => await Db.Authors
            .Include(a => a.Books)
            .OrderBy(a => a.LastName)
            .ThenBy(a => a.FirstName)
            .ToListAsync(ct);

    /// <inheritdoc/>
    public async Task<Author?> GetWithBooksAsync(long authorId, CancellationToken ct = default)
        => await Db.Authors
            .Include(a => a.Books).ThenInclude(b => b.Series)
            .Include(a => a.Books).ThenInclude(b => b.Genre)
            .Include(a => a.Books).ThenInclude(b => b.Language)
            .Include(a => a.Books).ThenInclude(b => b.Tags)
            .FirstOrDefaultAsync(a => a.AuthorId == authorId, ct);
}