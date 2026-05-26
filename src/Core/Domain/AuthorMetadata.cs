namespace LibraryApp.Core.Domain;

public sealed record AuthorMetadata(
    string? FirstName,
    string? LastName,
    string? MiddleName);