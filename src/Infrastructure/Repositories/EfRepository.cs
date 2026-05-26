// src/Infrastructure/Repositories/EfRepository.cs
using LibraryApp.Core.Interfaces.Repositories;
using LibraryApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LibraryApp.Infrastructure.Repositories;

internal abstract class EfRepository<T>(LibraryDbContext db) : IRepository<T>
    where T : class
{
    protected readonly LibraryDbContext Db = db;

    public async Task<T?> GetByIdAsync(long id, CancellationToken ct = default)
        => await Db.Set<T>().FindAsync([id], ct);

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default)
        => await Db.Set<T>().ToListAsync(ct);

    public async Task AddAsync(T entity, CancellationToken ct = default)
        => await Db.Set<T>().AddAsync(entity, ct);

    public void Update(T entity) => Db.Set<T>().Update(entity);
    public void Delete(T entity) => Db.Set<T>().Remove(entity);
}