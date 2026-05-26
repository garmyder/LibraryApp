namespace LibraryApp.Core.Domain;

public sealed record BookWithRelations(
    Book Book,
    IReadOnlyList<Author> Authors,
    Series? Series) : ILibraryItem
{
    public string Name => Book.Name;
    public LibraryItemType Type => LibraryItemType.Book;
}