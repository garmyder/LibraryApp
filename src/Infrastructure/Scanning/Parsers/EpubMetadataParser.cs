using System.IO;
using System.IO.Compression;
using System.Xml.Linq;
using LibraryApp.Core.Domain;
using LibraryApp.Core.Interfaces;

namespace LibraryApp.Infrastructure.Scanning.Parsers;

internal sealed class EpubMetadataParser : IMetadataParser
{
    private static readonly XNamespace Opf  = "http://www.idpf.org/2007/opf";
    private static readonly XNamespace Dc   = "http://purl.org/dc/elements/1.1/";
    private static readonly XNamespace Cont = "urn:oasis:names:tc:opendocument:xmlns:container";

    public bool CanParse(string extension)
        => extension.Equals(".epub", StringComparison.OrdinalIgnoreCase);

    public async Task<BookMetadata> ParseAsync(string filePath, CancellationToken ct = default)
    {
        using var zip = ZipFile.OpenRead(filePath);

        var opfPath = await FindOpfPathAsync(zip, ct);
        var opfEntry = zip.GetEntry(opfPath)
            ?? throw new InvalidDataException($"OPF entry '{opfPath}' not found in {filePath}.");

        await using var opfStream = opfEntry.Open();
        var opf = await XDocument.LoadAsync(opfStream, LoadOptions.None, ct);

        var meta = opf.Descendants(Opf + "metadata").FirstOrDefault()
                ?? opf.Descendants("metadata").FirstOrDefault();

        var title   = meta?.Element(Dc + "title")?.Value.Trim()
                   ?? Path.GetFileNameWithoutExtension(filePath);
        var lang    = meta?.Element(Dc + "language")?.Value.Trim();
        var isbn    = meta?.Elements(Dc + "identifier")
                          .FirstOrDefault(e => e.Attribute(Opf + "scheme")?.Value == "ISBN")
                          ?.Value.Trim();
        var published = meta?.Element(Dc + "date")?.Value.Trim();
        var description = meta?.Element(Dc + "description")?.Value.Trim();

        var authors = meta?.Elements(Dc + "creator")
            .Select(ParseEpubAuthor)
            .ToList() ?? [];

        var (seriesName, seriesNumber) = ParseSeries(meta);
        var cover = await TryParseCoverAsync(zip, opf, ct);

        return new BookMetadata(title, authors, Genre: null, lang,
            seriesName, seriesNumber, description, published, isbn,
            cover, "image/jpeg");
    }

    private static async Task<string> FindOpfPathAsync(ZipArchive zip, CancellationToken ct)
    {
        var container = zip.GetEntry("META-INF/container.xml")
            ?? throw new InvalidDataException("Missing META-INF/container.xml in EPUB.");

        await using var stream = container.Open();
        var doc = await XDocument.LoadAsync(stream, LoadOptions.None, ct);

        return doc.Descendants(Cont + "rootfile")
                   .FirstOrDefault()
                   ?.Attribute("full-path")?.Value
               ?? throw new InvalidDataException("Cannot locate OPF root file.");
    }

    private static AuthorMetadata ParseEpubAuthor(XElement el)
    {
        // "Last, First" or "First Last" convention
        var raw = el.Value.Trim();
        var parts = raw.Split(',', 2);
        return parts.Length == 2
            ? new AuthorMetadata(parts[1].Trim(), parts[0].Trim(), null)
            : new AuthorMetadata(raw, null, null);
    }

    private static (string? Name, string? Number) ParseSeries(XElement? meta)
    {
        // Calibre series extension
        var name   = meta?.Elements()
            .FirstOrDefault(e => e.Attribute("{http://www.idpf.org/2007/opf}property")
                                  ?.Value == "belongs-to-collection")?.Value;
        var number = meta?.Elements()
            .FirstOrDefault(e => e.Attribute("{http://www.idpf.org/2007/opf}property")
                                  ?.Value == "group-position")?.Value;
        return (name, number);
    }

    private static async Task<byte[]?> TryParseCoverAsync(
        ZipArchive zip, XDocument opf, CancellationToken ct)
    {
        var coverId = opf.Descendants(Opf + "meta")
            .FirstOrDefault(m => m.Attribute("name")?.Value == "cover")
            ?.Attribute("content")?.Value;

        if (coverId is null) return null;

        var href = opf.Descendants(Opf + "item")
            .FirstOrDefault(i => i.Attribute("id")?.Value == coverId)
            ?.Attribute("href")?.Value;

        if (href is null) return null;

        // Resolve relative to OPF location
        var opfDir = opf.BaseUri is { Length: > 0 } bu
            ? Path.GetDirectoryName(bu)!.Replace('\\', '/')
            : string.Empty;

        var coverPath = string.IsNullOrEmpty(opfDir) ? href : $"{opfDir}/{href}";
        var entry = zip.GetEntry(coverPath);
        if (entry is null) return null;

        await using var stream = entry.Open();
        using var ms = new MemoryStream((int)entry.Length);
        await stream.CopyToAsync(ms, ct);
        return ms.ToArray();
    }
}