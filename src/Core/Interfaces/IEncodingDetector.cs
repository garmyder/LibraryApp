using System.IO;
using System.Text;

namespace LibraryApp.Core.Interfaces;

/// <summary>Detects the character encoding of a byte stream.</summary>
public interface IEncodingDetector
{
    /// <summary>
    /// Returns the detected encoding.
    /// Falls back to UTF-8 if detection is inconclusive.
    /// </summary>
    Encoding Detect(Stream stream);

    /// <summary>Convenience overload for file paths.</summary>
    Encoding DetectFile(string filePath);
}