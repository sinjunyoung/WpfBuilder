namespace WpfBuilder.ViewModels
{
    public class PatchTabItem : BaseViewModel
    {
        private string _header = "새 패치";
        private string _gameName = "";
        private string _iconPath = "";
        private string _bgPath = "";
        private string _originalPath = "";
        private string _modifiedPath = "";

        public string Header { get => _header; set { _header = value; OnPropertyChanged(); } }

        public string GameName { get => _gameName; set { _gameName = value; OnPropertyChanged(); } }

        public string IconPath { get => _iconPath; set { _iconPath = value; OnPropertyChanged(); } }

        public string BgPath { get => _bgPath; set { _bgPath = value; OnPropertyChanged(); } }

        public string OriginalPath { get => _originalPath; set { _originalPath = value; OnPropertyChanged(); } }

        public string ModifiedPath { get => _modifiedPath; set { _modifiedPath = value; OnPropertyChanged(); } }
    }
}