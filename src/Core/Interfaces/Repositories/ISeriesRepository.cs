// src/Core/Interfaces/Repositories/ISeriesRepository.cs
using LibraryApp.Core.Domain;

namespace LibraryApp.Core.Interfaces.Repositories;

public interface ISeriesRepository : IRepository<Series>
{
    Task<IReadOnlyList<Series>> GetAllWithBooksAsync(CancellationToken ct = default);
}