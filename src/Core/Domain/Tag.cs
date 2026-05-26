// LibraryApp.Core/Domain/Tag.cs
namespace LibraryApp.Core.Domain;

public sealed class Tag
{
    public long TagId { get; private set; }
    public string Name { get; private set; } = null!;

    // Navigation
    public ICollection<Book> Books { get; private set; } = [];

    private Tag() { }

    public Tag(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
    }
}