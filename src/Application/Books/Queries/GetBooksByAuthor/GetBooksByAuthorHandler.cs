// src/Application/Books/Queries/GetBooksByAuthor/GetBooksByAuthorHandler.cs
using LibraryApp.Application.Common.DTOs;
using LibraryApp.Core.Domain;
using LibraryApp.Core.Interfaces;
using MediatR;

namespace LibraryApp.Application.Books.Queries.GetBooksByAuthor;

internal sealed class GetBooksByAuthorHandler(IUnitOfWork uow)
    : IRequestHandler<GetBooksByAuthorQuery, IReadOnlyList<BookDto>>
{
    public async Task<IReadOnlyList<BookDto>> Handle(
        GetBooksByAuthorQuery request, CancellationToken ct)
    {
        var books = await uow.Books.GetByAuthorAsync(request.AuthorId, ct);
        return books.Select(ToDto).ToList();
    }

    private static BookDto ToDto(BookWithRelations bwr) => new(
        bwr.Book.BookId,
        bwr.Book.Title,
        bwr.Book.Format,
        bwr.Book.Genre?.Name,
        bwr.Book.Language?.Name,
        bwr.Series?.SeriesName,
        bwr.Book.SeriesNumber,
        bwr.Book.Read,
        bwr.Book.Rate,
        bwr.Book.Annotation);
}