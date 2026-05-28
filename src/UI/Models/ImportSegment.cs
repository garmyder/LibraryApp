using System.Windows.Media;
using LibraryApp.Core.Domain;

namespace LibraryApp.UI.Models;

/// <summary>Represents a single colored segment in the import progress bar.</summary>
public sealed class ImportSegment
{
    // Frozen brushes — safe to share across threads
    private static readonly Brush BrushAdded   = Freeze(Color.FromRgb(76,  175,  80));  // #4CAF50
    private static readonly Brush BrushUpdated = Freeze(Color.FromRgb(139, 195,  74));  // #8BC34A
    private static readonly Brush BrushSkipped = Freeze(Color.FromRgb(158, 158, 158));  // #9E9E9E
    private static readonly Brush BrushFailed  = Freeze(Color.FromRgb(244,  67,  54));  // #F44336

    public ImportStatus Status   { get; init; }
    public string?      Title    { get; init; }
    public string?      Author   { get; init; }
    public string?      FilePath { get; init; }
    public string?      Error    { get; init; }

    public Brush Fill => Status switch
    {
        ImportStatus.Added   => BrushAdded,
        ImportStatus.Updated => BrushUpdated,
        ImportStatus.Skipped => BrushSkipped,
        ImportStatus.Failed  => BrushFailed,
        _                    => Brushes.Transparent
    };

    public string Tooltip
    {
        get
        {
            var label = Status switch
            {
                ImportStatus.Added   => "✓ Added",
                ImportStatus.Updated => "↺ Updated",
                ImportStatus.Skipped => "⏭ Skipped",
                ImportStatus.Failed  => "❌ Failed",
                _                    => "?"
            };

            if (Status == ImportStatus.Failed)
                return $"{label}: {Error ?? "Unknown error"}\n{FilePath}";

            var nameparts = new[] { Author, Title }.Where(s => !string.IsNullOrEmpty(s));
            return $"{label}: {string.Join(" — ", nameparts)}";
        }
    }

    private static SolidColorBrush Freeze(Color c)
    {
        var b = new SolidColorBrush(c);
        b.Freeze();
        return b;
    }
}
