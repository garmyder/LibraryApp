// LibraryApp.Core/Domain/Genre.cs
namespace LibraryApp.Core.Domain;

public sealed class Genre
{
    public long GenreId { get; private set; }
    public string Name { get; private set; } = null!;

    // Navigation
    public ICollection<Book> Books { get; private set; } = [];

    private Genre() { }

    public Genre(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
    }
}