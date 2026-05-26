using LibraryApp.Core.Domain;

namespace LibraryApp.Core.Interfaces.Repositories;

public interface IAuthorRepository : IRepository<Author>
{
    Task<IReadOnlyList<Author>> GetAllWithBookCountAsync(CancellationToken ct = default);
    Task<Author?> GetWithBooksAsync(long authorId, CancellationToken ct = default);
}
