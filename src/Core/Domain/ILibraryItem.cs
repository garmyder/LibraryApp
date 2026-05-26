namespace LibraryApp.Core.Domain;

public interface ILibraryItem
{
    string Name { get; }
    LibraryItemType Type { get; }
}