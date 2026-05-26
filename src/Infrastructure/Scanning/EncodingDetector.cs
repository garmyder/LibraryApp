using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using LibraryApp.Core.Interfaces;

namespace LibraryApp.Infrastructure.Scanning;

/// <summary>
/// Detects encoding via BOM → XML declaration → byte-frequency heuristic.
/// Handles windows-1251, UTF-8, UTF-16 LE/BE.
/// </summary>
public sealed partial class EncodingDetector : IEncodingDetector
{
    // <?xml version="1.0" encoding="windows-1251"?>
    [GeneratedRegex("""encoding=["']([^"']+)["']""",
        RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex XmlEncodingRegex();

    static EncodingDetector()
    {
        // Required in .NET 5+ for non-Unicode code pages (windows-1251, cp866, etc.)
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    public Encoding Detect(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        Span<byte> header = stackalloc byte[256];
        int read = stream.Read(header);
        stream.Seek(0, SeekOrigin.Begin);

        return TryBom(header[..read])
            ?? TryXmlDeclaration(header[..read])
            ?? TryHeuristic(header[..read])
            ?? Encoding.UTF8;
    }

    public Encoding DetectFile(string filePath)
    {
        using var fs = new FileStream(
            filePath, FileMode.Open, FileAccess.Read,
            FileShare.Read, bufferSize: 256);
        return Detect(fs);
    }

    // ── Strategies ────────────────────────────────────────────────────────

    private static Encoding? TryBom(ReadOnlySpan<byte> header)
    {
        if (header.Length >= 3 &&
            header[0] == 0xEF && header[1] == 0xBB && header[2] == 0xBF)
            return Encoding.UTF8;

        if (header.Length >= 2)
        {
            if (header[0] == 0xFF && header[1] == 0xFE) return Encoding.Unicode;      // UTF-16 LE
            if (header[0] == 0xFE && header[1] == 0xFF) return Encoding.BigEndianUnicode; // UTF-16 BE
        }

        return null;
    }

    private static Encoding? TryXmlDeclaration(ReadOnlySpan<byte> header)
    {
        // Read as Latin-1 to safely decode any single-byte encoding
        var text = Encoding.Latin1.GetString(header);
        var match = XmlEncodingRegex().Match(text);
        if (!match.Success) return null;

        try   { return Encoding.GetEncoding(match.Groups[1].Value); }
        catch { return null; }
    }

    /// <summary>
    /// Simple Cyrillic heuristic: windows-1251 Cyrillic bytes (0xC0-0xFF)
    /// are far more frequent than UTF-8 multi-byte sequences in CYR text.
    /// </summary>
    private static Encoding? TryHeuristic(ReadOnlySpan<byte> header)
    {
        int win1251 = 0, utf8Seq = 0;

        for (int i = 0; i < header.Length - 1; i++)
        {
            byte b = header[i];
            if (b >= 0xC0)                                              win1251++;
            if ((b & 0xE0) == 0xC0 && (header[i + 1] & 0xC0) == 0x80) utf8Seq++;
        }

        if (win1251 > utf8Seq * 2 && win1251 > 5)
            return Encoding.GetEncoding("windows-1251");

        return null;
    }
}