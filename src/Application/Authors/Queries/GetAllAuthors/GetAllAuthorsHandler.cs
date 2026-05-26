// src/Application/Authors/Queries/GetAllAuthors/GetAllAuthorsHandler.cs
using LibraryApp.Application.Common.DTOs;
using LibraryApp.Core.Interfaces;
using MediatR;

namespace LibraryApp.Application.Authors.Queries.GetAllAuthors;

internal sealed class GetAllAuthorsHandler(IUnitOfWork uow)
    : IRequestHandler<GetAllAuthorsQuery, IReadOnlyList<AuthorDto>>
{
    public async Task<IReadOnlyList<AuthorDto>> Handle(
        GetAllAuthorsQuery request, CancellationToken ct)
    {
        var authors = await uow.Authors.GetAllWithBookCountAsync(ct);

        return authors
            .Select(a => new AuthorDto(a.AuthorId, a.Name, a.Books.Count))
            .ToList();
    }
}