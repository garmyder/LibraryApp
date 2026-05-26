// src/Application/Common/DTOs/AuthorDto.cs
namespace LibraryApp.Application.Common.DTOs;

public sealed record AuthorDto(
    long AuthorId,
    string FullName,
    int BookCount);