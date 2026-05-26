using LibraryApp.Core.Interfaces;
using MediatR;

namespace LibraryApp.Application.Books.Commands.ToggleRead;

internal sealed class MarkAsUnreadHandler(IUnitOfWork uow)
    : IRequestHandler<MarkAsUnreadCommand, int>
{
    public async Task<int> Handle(MarkAsUnreadCommand request, CancellationToken ct)
    {
        var books = await uow.Books.GetByIdsAsync(request.BookIds, ct);

        foreach (var book in books)
            book.MarkAsUnread();

        return await uow.SaveChangesAsync(ct);
    }
}