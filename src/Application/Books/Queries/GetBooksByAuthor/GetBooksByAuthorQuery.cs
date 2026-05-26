// src/Application/Books/Queries/GetBooksByAuthor/GetBooksByAuthorQuery.cs
using LibraryApp.Application.Common.DTOs;
using MediatR;

namespace LibraryApp.Application.Books.Queries.GetBooksByAuthor;

public sealed record GetBooksByAuthorQuery(long AuthorId)
    : IRequest<IReadOnlyList<BookDto>>;