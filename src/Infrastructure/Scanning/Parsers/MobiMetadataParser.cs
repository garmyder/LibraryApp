using System.IO;
using System.Text;
using LibraryApp.Core.Domain;
using LibraryApp.Core.Interfaces;

namespace LibraryApp.Infrastructure.Scanning.Parsers;

/// <summary>
/// Parses MOBI/AZW3 files by reading the PalmDB header and EXTH block.
/// Extracts title, author, and series where available.
/// For full DRM-free parsing consider the Mobi NuGet package.
/// </summary>
internal sealed class MobiMetadataParser : IMetadataParser
{
    // EXTH record types
    private const int ExthAuthor     = 100;
    private const int ExthPublisher  = 101;
    private const int ExthIsbn       = 104;
    private const int ExthSubject    = 105;
    private const int ExthPublished  = 106;
    private const int ExthSeriesName = 517;
    private const int ExthSeriesNum  = 518;

    public bool CanParse(string extension)
        => extension.Equals(".mobi", StringComparison.OrdinalIgnoreCase)
        || extension.Equals(".azw3", StringComparison.OrdinalIgnoreCase)
        || extension.Equals(".azw",  StringComparison.OrdinalIgnoreCase);

    public async Task<BookMetadata> ParseAsync(string filePath, CancellationToken ct = default)
    {
        await using var stream = File.OpenRead(filePath);
        using var reader = new BinaryReader(stream, Encoding.ASCII, leaveOpen: true);

        // PalmDB header: bytes 0-31 = database name (null-terminated)
        var nameBytes = reader.ReadBytes(32);
        var palmTitle = Encoding.UTF8.GetString(nameBytes).TrimEnd('\0').Trim();

        // Skip to record list: offset 32
        reader.BaseStream.Seek(32, SeekOrigin.Begin);
        reader.ReadBytes(24); // attributes, version, dates, etc.

        var numRecords = ReadBigEndianUInt16(reader);
        if (numRecords == 0)
            return Fallback(filePath);

        // Read first record offset (record 0 = PalmDOC header + MOBI header)
        var record0Offset = ReadBigEndianUInt32(reader);

        reader.BaseStream.Seek(record0Offset, SeekOrigin.Begin);
        reader.ReadBytes(16); // PalmDOC header (16 bytes)

        // MOBI header
        var mobi = Encoding.ASCII.GetString(reader.ReadBytes(4));
        if (mobi != "MOBI")
            return Fallback(filePath, palmTitle);

        var mobiHeaderLen = ReadBigEndianUInt32(reader);
        var mobiType      = ReadBigEndianUInt32(reader); // 2=MOBI, 6=Periodical
        reader.ReadBytes(4); // encoding
        reader.ReadBytes(4); // unique id
        reader.ReadBytes(4); // file version

        // Skip to full name offset (bytes 20-23 of MOBI header = offset from record 0)
        reader.BaseStream.Seek(record0Offset + 16 + 20, SeekOrigin.Begin);
        var titleOffset = ReadBigEndianUInt32(reader);
        var titleLen    = ReadBigEndianUInt32(reader);

        // EXTH flag at offset 16+128 of MOBI header
        reader.BaseStream.Seek(record0Offset + 16 + 128, SeekOrigin.Begin);
        var exthFlags = ReadBigEndianUInt32(reader);

        // Full title
        string? fullTitle = null;
        if (titleLen > 0 && titleLen < 2048)
        {
            reader.BaseStream.Seek(record0Offset + titleOffset, SeekOrigin.Begin);
            fullTitle = Encoding.UTF8.GetString(reader.ReadBytes((int)titleLen)).Trim();
        }

        var title = fullTitle ?? palmTitle;
        if (string.IsNullOrWhiteSpace(title))
            title = Path.GetFileNameWithoutExtension(filePath);

        // EXTH block (if present)
        Dictionary<int, List<string>> exth = [];
        if ((exthFlags & 0x40) != 0)
        {
            // EXTH starts at record0Offset + 16 (PalmDOC) + mobiHeaderLen
            reader.BaseStream.Seek(record0Offset + 16 + mobiHeaderLen, SeekOrigin.Begin);
            exth = TryReadExth(reader);
        }

        var authors = (exth.GetValueOrDefault(ExthAuthor) ?? [])
            .Select(ParseExthAuthor)
            .ToList();

        return new BookMetadata(
            Title:        title,
            Authors:      authors,
            Genre:        exth.GetValueOrDefault(ExthSubject)?.FirstOrDefault(),
            Language:     null,
            SeriesName:   exth.GetValueOrDefault(ExthSeriesName)?.FirstOrDefault(),
            SeriesNumber: exth.GetValueOrDefault(ExthSeriesNum)?.FirstOrDefault(),
            Annotation:   null,
            Published:    exth.GetValueOrDefault(ExthPublished)?.FirstOrDefault(),
            Isbn:         exth.GetValueOrDefault(ExthIsbn)?.FirstOrDefault(),
            CoverBytes:   null,
            CoverMimeType: null);
    }

    private static Dictionary<int, List<string>> TryReadExth(BinaryReader reader)
    {
        var result = new Dictionary<int, List<string>>();
        try
        {
            var identifier = Encoding.ASCII.GetString(reader.ReadBytes(4));
            if (identifier != "EXTH") return result;

            var headerLen = ReadBigEndianUInt32(reader);
            var count     = ReadBigEndianUInt32(reader);

            for (var i = 0; i < count; i++)
            {
                var type   = (int)ReadBigEndianUInt32(reader);
                var length = (int)ReadBigEndianUInt32(reader) - 8;
                if (length <= 0 || length > 4096) { reader.ReadBytes(Math.Max(length, 0)); continue; }

                var value = Encoding.UTF8.GetString(reader.ReadBytes(length)).Trim('\0').Trim();
                if (!result.TryGetValue(type, out var list))
                    result[type] = list = [];
                list.Add(value);
            }
        }
        catch { /* Malformed EXTH — return what we have */ }
        return result;
    }

    private static AuthorMetadata ParseExthAuthor(string raw)
    {
        var parts = raw.Split(',', 2);
        return parts.Length == 2
            ? new AuthorMetadata(parts[1].Trim(), parts[0].Trim(), null)
            : new AuthorMetadata(raw.Trim(), null, null);
    }

    private static BookMetadata Fallback(string filePath, string? title = null) =>
        new(title ?? Path.GetFileNameWithoutExtension(filePath),
            [], null, null, null, null, null, null, null, null, null);

    private static ushort ReadBigEndianUInt16(BinaryReader r)
    {
        var b = r.ReadBytes(2);
        return (ushort)((b[0] << 8) | b[1]);
    }

    private static uint ReadBigEndianUInt32(BinaryReader r)
    {
        var b = r.ReadBytes(4);
        return (uint)((b[0] << 24) | (b[1] << 16) | (b[2] << 8) | b[3]);
    }
}