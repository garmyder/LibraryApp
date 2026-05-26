using MediatR;

namespace LibraryApp.Application.Books.Commands.ToggleRead;

/// <summary>Marks one or more books as unread.</summary>
public sealed record MarkAsUnreadCommand(IReadOnlyList<long> BookIds) : IRequest<int>;