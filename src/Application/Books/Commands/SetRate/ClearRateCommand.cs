using MediatR;

namespace LibraryApp.Application.Books.Commands.SetRate;

public sealed record ClearRateCommand(long BookId) : IRequest;