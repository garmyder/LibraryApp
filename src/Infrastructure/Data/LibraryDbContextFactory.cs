// LibraryApp.Infrastructure/Data/LibraryDbContextFactory.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LibraryApp.Infrastructure.Data;

internal sealed class LibraryDbContextFactory : IDesignTimeDbContextFactory<LibraryDbContext>
{
    public LibraryDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseSqlite("Data Source=library.db")
            .Options;

        return new LibraryDbContext(options);
    }
}