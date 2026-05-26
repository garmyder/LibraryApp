// LibraryApp.Core/Domain/Author.cs
namespace LibraryApp.Core.Domain;

public sealed class Author : ILibraryItem
{
    public long AuthorId { get; private set; }
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public string? MiddleName { get; private set; }

    // Navigation
    public ICollection<Book> Books { get; private set; } = [];

    public string Name => string.IsNullOrWhiteSpace(MiddleName)
        ? $"{LastName} {FirstName}"
        : $"{LastName} {FirstName} {MiddleName}";

    public LibraryItemType Type => LibraryItemType.Book;

    /// <summary>Required by EF Core.</summary>
    private Author() { }

    public Author(string firstName, string lastName, string? middleName = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName);
        FirstName = firstName;
        LastName = lastName;
        MiddleName = middleName;
    }
}