using LibraryApp.Core.Interfaces;
using MediatR;

namespace LibraryApp.Application.Books.Commands.ToggleRead;

internal sealed class MarkAsReadHandler(IUnitOfWork uow)
    : IRequestHandler<MarkAsReadCommand, int>
{
    public async Task<int> Handle(MarkAsReadCommand request, CancellationToken ct)
    {
        var books = await uow.Books.GetByIdsAsync(request.BookIds, ct);

        foreach (var book in books)
            book.MarkAsRead();

        return await uow.SaveChangesAsync(ct);
    }
}