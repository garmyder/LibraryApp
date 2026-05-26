// src/Infrastructure/DependencyInjection.cs

using LibraryApp.Core.Interfaces;
using LibraryApp.Infrastructure.Data;
using LibraryApp.Infrastructure.Scanning;
using LibraryApp.Infrastructure.Scanning.Parsers;
using LibraryApp.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryApp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<LibraryDbContext>(opt => opt.UseSqlite(connectionString));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Scanning
        services.AddSingleton<IFileHasher, FileHasher>();
        services.AddTransient<IMetadataParser, Fb2MetadataParser>();
        services.AddTransient<IMetadataParser, EpubMetadataParser>();
        services.AddTransient<IMetadataParser, MobiMetadataParser>();
        services.AddTransient<IMetadataParser, PdfMetadataParser>();
        services.AddTransient<IBookScanner, BookScanner>();
        services.AddScoped<IImportService, ImportService>();
        services.AddSingleton<IEncodingDetector, EncodingDetector>();
        services.AddTransient<IArchiveScanner,  ZipArchiveScanner>();

        return services;
    }
}