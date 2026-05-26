using MediatR;

namespace LibraryApp.Application.Books.Commands.DeleteBooks;

/// <summary>Deletes one or more books by their IDs.</summary>
public sealed record DeleteBooksCommand(IReadOnlyList<long> BookIds) : IRequest<int>;