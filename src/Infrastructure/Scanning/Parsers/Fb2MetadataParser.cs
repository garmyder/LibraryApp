using System.IO;
using System.Xml;
using System.Xml.Linq;
using LibraryApp.Core.Domain;
using LibraryApp.Core.Interfaces;

namespace LibraryApp.Infrastructure.Scanning.Parsers;

public sealed class Fb2MetadataParser(IEncodingDetector encodingDetector) : IMetadataParser
{
    private static readonly XNamespace Fb2Ns =
        "http://www.gribuser.ru/xml/fictionbook/2.0";

    public bool CanParse(string extension)
        => extension.Equals(".fb2", StringComparison.OrdinalIgnoreCase);

    public async Task<BookMetadata> ParseAsync(string filePath, CancellationToken ct = default)
    {
        var encoding = encodingDetector.DetectFile(filePath);
        var settings = new XmlReaderSettings { Async = true };

        await using var fs = new FileStream(
            filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

        using var reader = XmlReader.Create(
            new StreamReader(fs, encoding, detectEncodingFromByteOrderMarks: true),
            settings);

        // Load through the encoding-aware reader — fixes windows-1251
        var doc  = await XDocument.LoadAsync(reader, LoadOptions.None, ct);
        var desc = doc.Root?.Element(Fb2Ns + "description");
        var ti   = desc?.Element(Fb2Ns + "title-info");
        var pi   = desc?.Element(Fb2Ns + "publish-info");

        return new BookMetadata(
            Title:        ti?.Element(Fb2Ns + "book-title")?.Value.Trim()
                          ?? Path.GetFileNameWithoutExtension(filePath),
            Authors:      ParseAuthors(ti),
            Genre:        ti?.Element(Fb2Ns + "genre")?.Value.Trim(),
            Language:     ti?.Element(Fb2Ns + "lang")?.Value.Trim(),
            SeriesName:   ti?.Element(Fb2Ns + "sequence")?.Attribute("name")?.Value.Trim(),
            SeriesNumber: ti?.Element(Fb2Ns + "sequence")?.Attribute("number")?.Value.Trim(),
            Annotation:   CleanAnnotation(ti?.Element(Fb2Ns + "annotation")),
            Published:    pi?.Element(Fb2Ns + "year")?.Value.Trim(),
            Isbn:         pi?.Element(Fb2Ns + "isbn")?.Value.Trim(),
            CoverBytes:   ParseCover(doc),
            CoverMimeType: "image/jpeg");
    }
    
    private IReadOnlyList<AuthorMetadata> ParseAuthors(XElement? ti)
        => ti?.Elements(Fb2Ns + "author")
              .Select(a => new AuthorMetadata(
                  a.Element(Fb2Ns + "first-name")?.Value.Trim(),
                  a.Element(Fb2Ns + "last-name")?.Value.Trim(),
                  a.Element(Fb2Ns + "middle-name")?.Value.Trim()))
              .ToList()
           ?? [];

    private static string? CleanAnnotation(XElement? el)
        => el is null ? null : string.Join(' ', el.DescendantNodes()
            .OfType<XText>()
            .Select(t => t.Value.Trim())
            .Where(t => t.Length > 0));

    private static byte[]? ParseCover(XDocument doc)
    {
        var coverRef = doc.Descendants(Fb2Ns + "coverpage")
            .Descendants(Fb2Ns + "image")
            .FirstOrDefault()
            ?.Attribute("{http://www.w3.org/1999/xlink}href")
            ?.Value.TrimStart('#');

        if (coverRef is null) return null;

        var binary = doc.Descendants(Fb2Ns + "binary")
            .FirstOrDefault(b => b.Attribute("id")?.Value == coverRef);

        return binary is null ? null : Convert.FromBase64String(
            binary.Value.Replace("\n", "").Replace("\r", ""));
    }
}