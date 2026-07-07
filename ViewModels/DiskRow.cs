using System.IO;

namespace WpfBuilder.ViewModels;

public class DiskRow : BaseViewModel
{
    private string _gameName = "";
    private string _bgPath = "";
    private string _changeLog = "";
    private string _originalPath = "";
    private string _modifiedPath = "";

    public string GameName
    {
        get => _gameName;
        set { _gameName = value; OnPropertyChanged(); OnPropertyChanged(nameof(Header)); }
    }

    public string BgPath
    {
        get => _bgPath;
        set { _bgPath = value; OnPropertyChanged(); }
    }

    public string ChangeLog
    {
        get => _changeLog;
        set { _changeLog = value; OnPropertyChanged(); }
    }

    public string OriginalPath
    {
        get => _originalPath;
        set
        {
            _originalPath = value;
            OnPropertyChanged();

            if (string.IsNullOrWhiteSpace(_gameName) && !string.IsNullOrWhiteSpace(value))
            {
                GameName = Path.GetFileNameWithoutExtension(value);
            }
        }
    }

    public string ModifiedPath
    {
        get => _modifiedPath;
        set { _modifiedPath = value; OnPropertyChanged(); }
    }

    public string Header => string.IsNullOrWhiteSpace(GameName) ? "(이름 없음)" : GameName;
}