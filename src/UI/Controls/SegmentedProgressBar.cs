using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using LibraryApp.UI.Models;

namespace LibraryApp.UI.Controls;

public partial class SegmentedProgressBar
{
    private readonly List<ImportSegment> _data = [];

    // ── Dependency properties ─────────────────────────────────────────────

    public static readonly DependencyProperty TotalProperty =
        DependencyProperty.Register(nameof(Total), typeof(int), typeof(SegmentedProgressBar),
            new PropertyMetadata(0, (d, _) => ((SegmentedProgressBar)d).RefreshLayout()));

    public static readonly DependencyProperty SegmentsSourceProperty =
        DependencyProperty.Register(nameof(SegmentsSource), typeof(IEnumerable<ImportSegment>),
            typeof(SegmentedProgressBar),
            new PropertyMetadata(null, OnSegmentsSourceChanged));

    public int Total
    {
        get => (int)GetValue(TotalProperty);
        set => SetValue(TotalProperty, value);
    }

    public IEnumerable<ImportSegment> SegmentsSource
    {
        get => (IEnumerable<ImportSegment>)GetValue(SegmentsSourceProperty);
        set => SetValue(SegmentsSourceProperty, value);
    }

    // ── Constructor ───────────────────────────────────────────────────────

    public SegmentedProgressBar()
    {
        InitializeComponent();
        Loaded      += (_, _) => RefreshLayout();
        SizeChanged += (_, _) => RefreshLayout();
    }

    // ── Collection tracking ───────────────────────────────────────────────

    private static void OnSegmentsSourceChanged(
        DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not SegmentedProgressBar bar) return;

        if (e.OldValue is INotifyCollectionChanged old)
            old.CollectionChanged -= bar.OnCollectionChanged;

        bar._data.Clear();
        bar.SegmentCanvas.Children.Clear();

        if (e.NewValue is INotifyCollectionChanged newColl)
            newColl.CollectionChanged += bar.OnCollectionChanged;

        if (e.NewValue is IEnumerable<ImportSegment> items)
        {
            bar._data.AddRange(items);
            bar.RefreshLayout();
        }
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems is not null)
        {
            // Append only the new segment — avoids O(n²) full redraws
            foreach (ImportSegment seg in e.NewItems)
            {
                var index = _data.Count;
                _data.Add(seg);
                AppendRect(seg, index);
            }
        }
        else
        {
            // Reset or any other action — full redraw
            _data.Clear();
            if (sender is IEnumerable<ImportSegment> all) _data.AddRange(all);
            RefreshLayout();
        }
    }

    // ── Rendering ─────────────────────────────────────────────────────────

    /// <summary>Appends a single rectangle for the newly added segment (O(1)).</summary>
    private void AppendRect(ImportSegment seg, int index)
    {
        if (Total <= 0 || SegmentCanvas.ActualWidth <= 0) return;

        var segW = SegmentCanvas.ActualWidth / Total;
        SegmentCanvas.Children.Add(BuildRect(seg, index * segW, segW, SegmentCanvas.ActualHeight));
    }

    /// <summary>Redraws all segments from scratch (called on resize or full reset).</summary>
    private void RefreshLayout()
    {
        SegmentCanvas.Children.Clear();
        if (Total <= 0 || SegmentCanvas.ActualWidth <= 0) return;

        var segW = SegmentCanvas.ActualWidth / Total;
        var h    = SegmentCanvas.ActualHeight;

        for (var i = 0; i < _data.Count; i++)
            SegmentCanvas.Children.Add(BuildRect(_data[i], i * segW, segW, h));
    }

    private static Rectangle BuildRect(ImportSegment seg, double left, double width, double height)
    {
        var rect = new Rectangle
        {
            Width   = Math.Max(width, 1),
            Height  = height,
            Fill    = seg.Fill,
            ToolTip = seg.Tooltip
        };
        Canvas.SetLeft(rect, left);
        return rect;
    }
}
