using LibraryApp.Core.Domain;
using LibraryApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LibraryApp.Infrastructure.Services;

internal sealed class LookupCache(LibraryDbContext db)
{
    private readonly Dictionary<string, long> _genres = [];
    private readonly Dictionary<string, long> _langs  = [];
    private readonly Dictionary<string, long> _series = [];

    public async Task<long?> GetOrCreateGenreIdAsync(string? name, CancellationToken ct)
    {
        if (name is null) return null;
        if (_genres.TryGetValue(name, out var id)) return id;
        var entity = await db.Genres.FirstOrDefaultAsync(g => g.Name == name, ct)
                     ?? (await db.Genres.AddAsync(new Genre(name), ct)).Entity;
        await db.SaveChangesAsync(ct);
        return _genres[name] = entity.GenreId;
    }

    public async Task<long?> GetOrCreateLanguageIdAsync(string? code, CancellationToken ct)
    {
        if (code is null) return null;
        if (_langs.TryGetValue(code, out var id)) return id;
        var entity = await db.Languages.FirstOrDefaultAsync(l => l.Code == code, ct)
                     ?? (await db.Languages.AddAsync(new Language(code, code), ct)).Entity;
        await db.SaveChangesAsync(ct);
        return _langs[code] = entity.LanguageId;
    }

    public async Task<long?> GetOrCreateSeriesIdAsync(string? name, CancellationToken ct)
    {
        if (name is null) return null;
        if (_series.TryGetValue(name, out var id)) return id;
        var entity = await db.Series.FirstOrDefaultAsync(s => s.SeriesName == name, ct)
                     ?? (await db.Series.AddAsync(new Series(name), ct)).Entity;
        await db.SaveChangesAsync(ct);
        return _series[name] = entity.SeriesId;
    }
}