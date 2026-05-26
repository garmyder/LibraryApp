// src/Core/Interfaces/IUnitOfWork.cs
using LibraryApp.Core.Interfaces.Repositories;

namespace LibraryApp.Core.Interfaces;

public interface IUnitOfWork : IAsyncDisposable
{
    IAuthorRepository Authors { get; }
    IBookRepository Books { get; }
    ISeriesRepository Series { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}