using MediatR;

namespace LibraryApp.Application.Books.Commands.SetRate;

/// <summary>Sets a rating (1–5) for a single book.</summary>
public sealed record SetRateCommand(long BookId, int Rate) : IRequest;