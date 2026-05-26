using LibraryApp.Core.Interfaces;
using MediatR;

namespace LibraryApp.Application.Books.Commands.SetRate;

internal sealed class ClearRateHandler(IUnitOfWork uow) : IRequestHandler<ClearRateCommand>
{
    public async Task Handle(ClearRateCommand request, CancellationToken ct)
    {
        var book = await uow.Books.GetByIdAsync(request.BookId, ct)
                   ?? throw new KeyNotFoundException($"Book {request.BookId} not found.");

        book.ClearRate();
        await uow.SaveChangesAsync(ct);
    }
}