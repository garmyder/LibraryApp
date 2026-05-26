namespace LibraryApp.UI.ViewModels;

/// <summary>Lightweight VM for the authors list.</summary>
public sealed record AuthorListItemVm(long AuthorId, string Name, int BookCount);