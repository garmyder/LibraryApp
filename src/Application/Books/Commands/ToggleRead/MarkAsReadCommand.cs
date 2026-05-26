using MediatR;

namespace LibraryApp.Application.Books.Commands.ToggleRead;

/// <summary>Marks one or more books as read.</summary>
public sealed record MarkAsReadCommand(IReadOnlyList<long> BookIds) : IRequest<int>;