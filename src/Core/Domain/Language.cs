// LibraryApp.Core/Domain/Language.cs
namespace LibraryApp.Core.Domain;

public sealed class Language
{
    public long LanguageId { get; private set; }

    /// <summary>ISO 639-1 code, e.g. "uk", "en", "ru".</summary>
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;

    // Navigation
    public ICollection<Book> Books { get; private set; } = [];

    private Language() { }

    public Language(string code, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Code = code;
        Name = name;
    }
}