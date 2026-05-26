using LibraryApp.Core.Domain;

namespace LibraryApp.Core.Interfaces.Repositories;

public interface IBookRepository : IRepository<Book>
{
    Task<IReadOnlyList<BookWithRelations>> GetByAuthorAsync(long authorId, CancellationToken ct = default);
    Task<IReadOnlyList<BookWithRelations>> SearchAsync(string query, CancellationToken ct = default);
    // Task<IReadOnlyList<BookWithRelations>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default);
    Task<IReadOnlyList<Book>> GetByIdsAsync(IReadOnlyList<long> ids, CancellationToken ct = default);
    Task DeleteRangeAsync(IReadOnlyList<long> ids, CancellationToken ct = default);
}