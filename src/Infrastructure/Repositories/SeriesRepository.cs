// src/Infrastructure/Repositories/SeriesRepository.cs
using LibraryApp.Core.Domain;
using LibraryApp.Core.Interfaces.Repositories;
using LibraryApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LibraryApp.Infrastructure.Repositories;

internal sealed class SeriesRepository(LibraryDbContext db)
    : EfRepository<Series>(db), ISeriesRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<Series>> GetAllWithBooksAsync(CancellationToken ct = default)
        => await Db.Series
            .Include(s => s.Books)
            .OrderBy(s => s.SeriesName)
            .ToListAsync(ct);
}