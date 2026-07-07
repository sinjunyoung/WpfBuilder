using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using WpfBuilder.ViewModels;

namespace WpfBuilder.Controls;

public partial class PatchTabItem : UserControl
{
    public PatchTabItem()
    {
        InitializeComponent();
    }

    private void BtnPickBackground_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not DiskRow row)
            return;

        var dlg = new OpenFileDialog { Filter = "이미지 파일|*.png;*.jpg;*.jpeg" };

        if (dlg.ShowDialog() == true)
            row.BgPath = dlg.FileName;
    }

    private void BtnPickOriginal_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not DiskRow row)
            return;

        var dlg = new OpenFileDialog { Filter = "모든 파일|*.*" };

        if (dlg.ShowDialog() == true)
            row.OriginalPath = dlg.FileName;
    }

    private void BtnPickModified_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not DiskRow row)
            return;

        var dlg = new OpenFileDialog { Filter = "모든 파일|*.*" };

        if (dlg.ShowDialog() == true)
            row.ModifiedPath = dlg.FileName;
    }

    private void FileDragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        e.Handled = true;
    }

    private static string? GetFirstDroppedFile(DragEventArgs e)
        => e.Data.GetData(DataFormats.FileDrop) is string[] files && files.Length > 0 ? files[0] : null;

    private void BgDrop(object sender, DragEventArgs e)
    {
        if (DataContext is not DiskRow row) 
            return;

        var path = GetFirstDroppedFile(e);

        if (path != null) 
            row.BgPath = path;
    }

    private void OriginalDrop(object sender, DragEventArgs e)
    {
        if (DataContext is not DiskRow row) 
            return;

        var path = GetFirstDroppedFile(e);

        if (path != null) 
            row.OriginalPath = path;
    }

    private void ModifiedDrop(object sender, DragEventArgs e)
    {
        if (DataContext is not DiskRow row) 
            return;

        var path = GetFirstDroppedFile(e);

        if (path != null) 
            row.ModifiedPath = path;
    }
}