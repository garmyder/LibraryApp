// src/Application/Common/DTOs/BookDto.cs — виправлений
using LibraryApp.Core.Domain;

namespace LibraryApp.Application.Common.DTOs;

public sealed record BookDto(
    long BookId,
    string Title,
    BookFormat Format,
    string? Genre,
    string? Language,
    string? SeriesName,
    string? SeriesNumber,
    bool Read,
    int? Rate,
    string? Annotation);