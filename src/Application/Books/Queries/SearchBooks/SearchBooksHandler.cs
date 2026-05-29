// src/Application/Books/Queries/SearchBooks/SearchBooksHandler.cs

using LibraryApp.Application.Common.DTOs;
using LibraryApp.Core.Domain;
using LibraryApp.Core.Interfaces;
using MediatR;

namespace LibraryApp.Application.Books.Queries.SearchBooks;

internal sealed class SearchBooksHandler(IUnitOfWork uow)
    : IRequestHandler<SearchBooksQuery, IReadOnlyList<BookDto>>
{
    public async Task<IReadOnlyList<BookDto>> Handle(
        SearchBooksQuery request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.SearchTerm))
            return [];

        var books = await uow.Books.SearchAsync(request.SearchTerm, ct);
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
