// src/Application/Books/Queries/SearchBooks/SearchBooksQuery.cs
using LibraryApp.Application.Common.DTOs;
using MediatR;

namespace LibraryApp.Application.Books.Queries.SearchBooks;

public sealed record SearchBooksQuery(string SearchTerm)
    : IRequest<IReadOnlyList<BookDto>>;