using System.IO;
using System.Security.Cryptography;
using LibraryApp.Core.Interfaces;

namespace LibraryApp.Infrastructure.Scanning;

internal sealed class FileHasher : IFileHasher
{
    public async Task<string> ComputeHashAsync(string filePath, CancellationToken ct = default)
    {
        await using var stream = new FileStream(
            filePath, FileMode.Open, FileAccess.Read,
            FileShare.Read, bufferSize: 81920, useAsync: true);

        var hash = await SHA256.HashDataAsync(stream, ct);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}