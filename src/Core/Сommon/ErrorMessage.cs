namespace LibraryApp.Core.Сommon;

public abstract record ErrorMessage
{
    /// <summary>Predefined error types mapped to WPF resource keys.</summary>
    public enum Predefined
    {
        NoAuthorsFound,
        AuthorNotFound,
        NoSeriesFound,
        SeriesNotFound,
        NoSeriesOrBooks,
        NoBooksFound,
        BookNotFound,
        StoragePermissionDenied,
        InvalidFolder,
        AddFolderToScan,
        MappingFailed,
        DatabaseError,
        UnknownError,
    }

    /// <summary>Predefined error resolved via WPF resource dictionary.</summary>
    public sealed record Resource(Predefined Kind) : ErrorMessage;

    /// <summary>Arbitrary plain-text error message.</summary>
    public sealed record Custom(string Message) : ErrorMessage;

    /// <summary>Formatted error with positional arguments (string.Format-compatible).</summary>
    public sealed record Formatted(string Template, params object[] Args) : ErrorMessage
    {
        public string Resolve() => string.Format(Template, Args);
    }

    // --- Convenience factory methods ---
    public static ErrorMessage FromPredefined(Predefined kind) => new Resource(kind);
    public static ErrorMessage FromMessage(string message)     => new Custom(message);
    public static ErrorMessage FromTemplate(string template, params object[] args)
        => new Formatted(template, args);

    // --- Backward-compatible static shortcuts ---
    public static readonly ErrorMessage NoAuthorsFound       = new Resource(Predefined.NoAuthorsFound);
    public static readonly ErrorMessage AuthorNotFound       = new Resource(Predefined.AuthorNotFound);
    public static readonly ErrorMessage NoSeriesFound        = new Resource(Predefined.NoSeriesFound);
    public static readonly ErrorMessage SeriesNotFound       = new Resource(Predefined.SeriesNotFound);
    public static readonly ErrorMessage NoSeriesOrBooks      = new Resource(Predefined.NoSeriesOrBooks);
    public static readonly ErrorMessage NoBooksFound         = new Resource(Predefined.NoBooksFound);
    public static readonly ErrorMessage BookNotFound         = new Resource(Predefined.BookNotFound);
    public static readonly ErrorMessage StoragePermissionDenied = new Resource(Predefined.StoragePermissionDenied);
    public static readonly ErrorMessage InvalidFolder        = new Resource(Predefined.InvalidFolder);
    public static readonly ErrorMessage AddFolderToScan      = new Resource(Predefined.AddFolderToScan);
    public static readonly ErrorMessage MappingFailed        = new Resource(Predefined.MappingFailed);
    public static readonly ErrorMessage DatabaseError        = new Resource(Predefined.DatabaseError);
    public static readonly ErrorMessage UnknownError         = new Resource(Predefined.UnknownError);
}
