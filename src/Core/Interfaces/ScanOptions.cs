namespace LibraryApp.Core.Interfaces;

/// <summary>Defines how the import operation interacts with existing database data.</summary>
public enum ImportMode
{
    /// <summary>Clears all library tables before import, producing a clean database state.</summary>
    Recreate,

    /// <summary>Adds new books/authors and optionally removes books missing from disk.</summary>
    Update
}

public sealed record ScanOptions(
    bool       Recursive     = true,
    bool       RemoveMissing = false,
    ImportMode Mode          = ImportMode.Update);
