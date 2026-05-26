using LibraryApp.Core.Interfaces;
using MediatR;

namespace LibraryApp.Application.Books.Commands.SetRate;

internal sealed class SetRateHandler(IUnitOfWork uow)
    : IRequestHandler<SetRateCommand>
{
    public async Task Handle(SetRateCommand request, CancellationToken ct)
    {
        var book = await uow.Books.GetByIdAsync(request.BookId, ct)
                   ?? throw new KeyNotFoundException($"Book {request.BookId} not found.");

        book.SetRate(request.Rate);
        await uow.SaveChangesAsync(ct);
    }
}