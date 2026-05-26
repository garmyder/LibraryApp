// src/Application/Authors/Queries/GetAllAuthors/GetAllAuthorsQuery.cs
using LibraryApp.Application.Common.DTOs;
using MediatR;

namespace LibraryApp.Application.Authors.Queries.GetAllAuthors;

public sealed record GetAllAuthorsQuery : IRequest<IReadOnlyList<AuthorDto>>;