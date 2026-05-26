// src/Infrastructure/Data/UnitOfWork.cs
using LibraryApp.Core.Interfaces;
using LibraryApp.Core.Interfaces.Repositories;
using LibraryApp.Infrastructure.Repositories;

namespace LibraryApp.Infrastructure.Data;

internal sealed class UnitOfWork : IUnitOfWork
{
    private readonly LibraryDbContext _db;

    public IAuthorRepository Authors { get; }
    public IBookRepository Books { get; }
    public ISeriesRepository Series { get; }

    public UnitOfWork(LibraryDbContext db)
    {
        _db = db;
        Authors = new AuthorRepository(db);
        Books   = new BookRepository(db);
        Series  = new SeriesRepository(db);
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);

    public ValueTask DisposeAsync()
        => _db.DisposeAsync();
}