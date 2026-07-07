using Common;
using Microsoft.Win32;
using Patch.Core.Formats;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using WpfBuilder.Models;
using WpfBuilder.Services;
using WpfBuilder.ViewModels;

namespace WpfBuilder;

public partial class MainWindow : Window
{
    private readonly ObservableCollection<DiskRow> _tabs = [];

    private string _iconPath = "";
    private readonly string _stubPath = "";

    public MainWindow()
    {
        InitializeComponent();
        TabPatchList.ItemsSource = _tabs;
        _tabs.Add(new DiskRow());

        var defaultStub = Path.Combine(AppContext.BaseDirectory, "StubPatcher.exe");

        if (File.Exists(defaultStub))
            _stubPath = defaultStub;
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        IntPtr hWnd = new WindowInteropHelper(this).Handle;
        int value = 1;

        _ = Win32API.DwmSetWindowAttribute(hWnd, 20, ref value, sizeof(int));
    }

    private void BtnPickIcon_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog { Filter = "아이콘 파일 (*.ico)|*.ico" };
        if (dlg.ShowDialog() == true)
            ApplyIcon(dlg.FileName);
    }

    private void ApplyIcon(string path)
    {
        _iconPath = path;
        TxtIconPath.Text = path;
        Icon = new System.Windows.Media.Imaging.BitmapImage(new Uri(path));
    }

    private void FileDragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        e.Handled = true;
    }

    private static string? GetFirstDroppedFile(DragEventArgs e) => e.Data.GetData(DataFormats.FileDrop) is string[] files && files.Length > 0 ? files[0] : null;

    private void IconDrop(object sender, DragEventArgs e)
    {
        var path = GetFirstDroppedFile(e);

        if (path != null)
            ApplyIcon(path);
    }

    private void BgDrop(object sender, DragEventArgs e)
    {
        if (((FrameworkElement)sender).DataContext is not DiskRow row) 
            return;

        var path = GetFirstDroppedFile(e);

        if (path != null) 
            row.BgPath = path;
    }

    private void OriginalDrop(object sender, DragEventArgs e)
    {
        if (((FrameworkElement)sender).DataContext is not DiskRow row) 
            return;

        var path = GetFirstDroppedFile(e);

        if (path != null) 
            row.OriginalPath = path;
    }

    private void ModifiedDrop(object sender, DragEventArgs e)
    {
        if (((FrameworkElement)sender).DataContext is not DiskRow row) 
            return;

        var path = GetFirstDroppedFile(e);

        if (path != null) 
            row.ModifiedPath = path;
    }

    private void BtnAddTab_Click(object sender, RoutedEventArgs e)
    {
        if (_tabs.Count >= FooterLayout.MaxDisks)
        {
            MessageBox.Show($"탭은 최대 {FooterLayout.MaxDisks}개까지만 지원돼요!");
            return;
        }

        var row = new DiskRow();

        _tabs.Add(row);
        TabPatchList.SelectedItem = row;
    }

    private void BtnRemoveTab_Click(object sender, RoutedEventArgs e)
    {
        if (TabPatchList.Items.Count <= 1)
            return;

        if (TabPatchList.SelectedItem is DiskRow row) 
            _tabs.Remove(row);
    }

    private async void BtnBuild_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_stubPath) || !File.Exists(_stubPath))
        {
            MessageBox.Show("Stub.exe가 Resources 폴더에 없음! (WpfBuilder.csproj 기준 Resources\\StubPatcher.exe)");
            return;
        }
        if (_tabs.Any(t => string.IsNullOrWhiteSpace(t.OriginalPath) || string.IsNullOrWhiteSpace(t.ModifiedPath)))
        {
            MessageBox.Show("모든 탭에 원본/수정본 파일이 채워져 있어야 함!");
            return;
        }

        var saveDlg = new SaveFileDialog { Filter = "실행 파일 (*.exe)|*.exe", FileName = "Patcher.exe" };
        if (saveDlg.ShowDialog() != true) return;
        string outputPath = saveDlg.FileName;

        var tempXdeltaFiles = new List<string>();
        BuildProgress.Value = 0;
        TxtStatus.Text = "xdelta 생성 중...";
        var title = TxtWindowTitle.Text;

        try
        {
            var disks = new List<DiskInput>();

            for (int i = 0; i < _tabs.Count; i++)
            {
                var tab = _tabs[i];
                string tempXdelta = Path.Combine(AppContext.BaseDirectory, $"{Guid.NewGuid()}.xdelta");
                tempXdeltaFiles.Add(tempXdelta);

                var tabProgress = new Progress<ProgressInfo>(p =>
                {
                    double perTab = 0.9 / _tabs.Count;
                    BuildProgress.Value = (i * perTab + p.Percent * perTab) * 100;
                });

                await Task.Run(() => Xdelta3.CreatePatch(
                    tab.OriginalPath, tab.ModifiedPath, tempXdelta, tabProgress));

                disks.Add(new DiskInput
                {
                    TargetFileName = Path.GetFileName(tab.OriginalPath),
                    XdeltaFilePath = tempXdelta,
                    OriginalFilePathForMd5 = tab.OriginalPath,
                    BgImagePath = string.IsNullOrWhiteSpace(tab.BgPath) ? null : tab.BgPath,
                });
            }

            TxtStatus.Text = "패처 조립 중...";
            var assembleProgress = new Progress<double>(p => BuildProgress.Value = 90 + p * 10);

            await Task.Run(() => PatchBuilder.Build(
                _stubPath, outputPath, title, disks, _iconPath, assembleProgress));

            if (string.IsNullOrWhiteSpace(_iconPath))
            {
                TxtStatus.Text = "완료! (아이콘 미지정) ⚠️";
                MessageBox.Show($"패처 생성 완료!\n{outputPath}\n\n※ 아이콘을 지정 안 해서 기본 아이콘으로 나갑니다.");
            }
            else
            {
                TxtStatus.Text = "완료! ✅";
                MessageBox.Show($"패처 생성 완료! (아이콘 적용됨: {Path.GetFileName(_iconPath)})\n{outputPath}");
            }
        }
        catch (Exception ex)
        {
            TxtStatus.Text = "실패 ❌";
            MessageBox.Show($"빌드 실패: {ex.Message}");
        }
        finally
        {
            foreach (var f in tempXdeltaFiles)
            {
                try { if (File.Exists(f)) File.Delete(f); } catch {  }
            }
        }
    }
}