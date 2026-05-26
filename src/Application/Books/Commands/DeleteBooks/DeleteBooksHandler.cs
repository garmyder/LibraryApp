using LibraryApp.Core.Interfaces;
using MediatR;

namespace LibraryApp.Application.Books.Commands.DeleteBooks;

internal sealed class DeleteBooksHandler(IUnitOfWork uow)
    : IRequestHandler<DeleteBooksCommand, int>
{
    public async Task<int> Handle(DeleteBooksCommand request, CancellationToken ct)
    {
        await uow.Books.DeleteRangeAsync(request.BookIds, ct);
        return request.BookIds.Count;   // ExecuteDeleteAsync already committed
    }
}