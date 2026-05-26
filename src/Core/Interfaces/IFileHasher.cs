namespace LibraryApp.Core.Interfaces;

public interface IFileHasher
{
    /// <summary>Computes SHA-256 hex hash of a file.</summary>
    Task<string> ComputeHashAsync(string filePath, CancellationToken ct = default);
}