// src/UI/ViewModels/DesignTime/ImportWindowVmDesign.cs

using LibraryApp.Core.Interfaces;

namespace LibraryApp.UI.ViewModels.DesignTime;

/// <summary>
/// Parameterless design-time stub — used only by the XAML designer via d:DataContext.
/// Never instantiated at runtime.
/// </summary>
public sealed class ImportWindowVmDesign() : ImportWindowVm(null!, null!, null!)
{
    public static ImportWindowVmDesign Instance { get; } = new()
    {
        FolderPath = @"C:\Books\MyLibrary",
        StatusText = "Ready to import.",
        ImportMode = ImportMode.Update,
        Recursive  = true,
    };
}
