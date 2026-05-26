using LibraryApp.Core.Domain;
using LibraryApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LibraryApp.Infrastructure.Services;

/// <summary>Session-scoped cache that deduplicates authors during a single import run.</summary>
internal sealed class AuthorCache(LibraryDbContext db)
{
    private readonly Dictionary<string, Author> _cache = [];

    public async Task<Author> GetOrCreateAsync(AuthorMetadata meta, CancellationToken ct)
    {
        var key = BuildKey(meta);
        if (_cache.TryGetValue(key, out var cached)) return cached;

        var existing = await db.Authors
            .FirstOrDefaultAsync(a =>
                a.LastName  == (meta.LastName  ?? "") &&
                a.FirstName == (meta.FirstName ?? ""), ct);

        var author = existing ?? new Author(
            meta.FirstName ?? "Unknown",
            meta.LastName  ?? "Unknown",
            meta.MiddleName);

        if (existing is null) await db.Authors.AddAsync(author, ct);
        _cache[key] = author;
        return author;
    }

    private static string BuildKey(AuthorMetadata m)
        => $"{m.LastName?.ToLowerInvariant()}|{m.FirstName?.ToLowerInvariant()}";
}