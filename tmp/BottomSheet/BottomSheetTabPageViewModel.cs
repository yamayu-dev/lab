using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Telerik.Maui.Controls;
using Telerik.Maui.Controls.BottomSheet;

namespace TelerikMauiApp2.ViewModels
{
    public enum DockTab
    {
        Home,
        Settings,
        Info
    }

    public class BottomSheetTabPageViewModel : INotifyPropertyChanged
    {
        private string _selectedInfo = "BottomSheetを開いてタブを選択してください";
        private string _homeText = "";
        private bool _notificationEnabled = true;
        private double _volumeValue = 50;
        
    private bool _isDockVisible = true;
    private DockTab _selectedDockTab = DockTab.Home;

    private bool _isDockHomeVisible = true;
    private bool _isDockSettingsVisible;
    private bool _isDockInfoVisible;

    private Color _dockHomeBackground = Color.FromArgb("#2C2C2E");
    private Color _dockSettingsBackground = Colors.Transparent;
    private Color _dockInfoBackground = Colors.Transparent;

        public RadBottomSheet BottomSheet { get; set; }

        public bool IsDockVisible
        {
            get => _isDockVisible;
            set
            {
                if (_isDockVisible == value)
                {
                    return;
                }

                _isDockVisible = value;
                OnPropertyChanged();
            }
        }

        public DockTab SelectedDockTab
        {
            get => _selectedDockTab;
            set
            {
                if (_selectedDockTab == value)
                {
                    return;
                }

                _selectedDockTab = value;
                OnPropertyChanged();
                ApplyDockSelection();
            }
        }

        public string SelectedInfo
        {
            get => _selectedInfo;
            set
            {
                _selectedInfo = value;
                OnPropertyChanged();
            }
        }

        public string HomeText
        {
            get => _homeText;
            set
            {
                _homeText = value;
                OnPropertyChanged();
            }
        }

        public bool NotificationEnabled
        {
            get => _notificationEnabled;
            set
            {
                _notificationEnabled = value;
                OnPropertyChanged();
            }
        }

        public double VolumeValue
        {
            get => _volumeValue;
            set
            {
                _volumeValue = value;
                OnPropertyChanged();
            }
        }

        public bool IsDockHomeVisible
        {
            get => _isDockHomeVisible;
            set
            {
                if (_isDockHomeVisible == value)
                {
                    return;
                }

                _isDockHomeVisible = value;
                OnPropertyChanged();
            }
        }

        public bool IsDockSettingsVisible
        {
            get => _isDockSettingsVisible;
            set
            {
                if (_isDockSettingsVisible == value)
                {
                    return;
                }

                _isDockSettingsVisible = value;
                OnPropertyChanged();
            }
        }

        public bool IsDockInfoVisible
        {
            get => _isDockInfoVisible;
            set
            {
                if (_isDockInfoVisible == value)
                {
                    return;
                }

                _isDockInfoVisible = value;
                OnPropertyChanged();
            }
        }

        public Color DockHomeBackground
        {
            get => _dockHomeBackground;
            set
            {
                if (_dockHomeBackground == value)
                {
                    return;
                }

                _dockHomeBackground = value;
                OnPropertyChanged();
            }
        }

        public Color DockSettingsBackground
        {
            get => _dockSettingsBackground;
            set
            {
                if (_dockSettingsBackground == value)
                {
                    return;
                }

                _dockSettingsBackground = value;
                OnPropertyChanged();
            }
        }

        public Color DockInfoBackground
        {
            get => _dockInfoBackground;
            set
            {
                if (_dockInfoBackground == value)
                {
                    return;
                }

                _dockInfoBackground = value;
                OnPropertyChanged();
            }
        }

        public ICommand SelectHomeCommand { get; }
        public ICommand SelectSettingsCommand { get; }
        public ICommand SelectInfoCommand { get; }
    public ICommand BottomSheetStateChangingCommand { get; }
    public ICommand OpenBottomSheetCommand { get; }
        public ICommand SelectDockTabCommand { get; }

        public BottomSheetTabPageViewModel()
        {
            // Dock クリックは「切り替え」なので BottomSheet を閉じない。
            SelectDockTabCommand = new Command<DockTab>(OnSelectDockTab);

            // 既存の “保存/確認” などの動作は残す（BottomSheet を閉じる用途）。
            SelectHomeCommand = new Command(SelectHome);
            SelectSettingsCommand = new Command(SelectSettings);
            SelectInfoCommand = new Command(SelectInfo);
            BottomSheetStateChangingCommand = new Command<BottomSheetStateChangingEventArgs>(OnBottomSheetStateChanging);
            OpenBottomSheetCommand = new Command(OpenBottomSheet);

            // 初期選択
            ApplyDockSelection();
        }

        private void OnSelectDockTab(DockTab tab)
        {
            SelectedDockTab = tab;
        }

        private void OpenBottomSheet()
        {
            BottomSheet?.GoToBottomSheetState("Full");
        }

        private void OnBottomSheetStateChanging(BottomSheetStateChangingEventArgs args)
        {
            // State 名（Full/Hidden 等）が取れない環境があるため、Position ベースで「Hidden 近傍」を判定する。
            // 0 に張り付かずごく小さい値で止まることがあるため、少し広めに閾値を取る。
            IsDockVisible = args.Position > 0.02;

            // 念のため再適用（イベント終端で残る取りこぼし対策）。
            if (!IsDockVisible)
            {
                MainThread.BeginInvokeOnMainThread(() => IsDockVisible = false);
            }
        }

        public void SelectDockTab(DockTab tab)
        {
            SelectedDockTab = tab;
        }

        private void ApplyDockSelection()
        {
            IsDockHomeVisible = SelectedDockTab == DockTab.Home;
            IsDockSettingsVisible = SelectedDockTab == DockTab.Settings;
            IsDockInfoVisible = SelectedDockTab == DockTab.Info;

            // “MacのDockっぽさ”として、選択中だけ薄いハイライト背景をつける
            DockHomeBackground = SelectedDockTab == DockTab.Home ? Color.FromArgb("#2C2C2E") : Colors.Transparent;
            DockSettingsBackground = SelectedDockTab == DockTab.Settings ? Color.FromArgb("#2C2C2E") : Colors.Transparent;
            DockInfoBackground = SelectedDockTab == DockTab.Info ? Color.FromArgb("#2C2C2E") : Colors.Transparent;
        }

        private void SelectHome()
        {
            var text = string.IsNullOrEmpty(HomeText) ? "(空)" : HomeText;
            SelectedInfo = $"ホームタブで選択されました: {text}";
            
            if (BottomSheet != null)
            {
                BottomSheet.GoToBottomSheetState("Hidden");
            }
        }

        private void SelectSettings()
        {
            SelectedInfo = $"設定タブで保存されました - 通知: {(NotificationEnabled ? "ON" : "OFF")}, ボリューム: {VolumeValue:F0}";
            
            if (BottomSheet != null)
            {
                BottomSheet.GoToBottomSheetState("Hidden");
            }
        }

        private void SelectInfo()
        {
            SelectedInfo = "情報タブが確認されました - バージョン: 1.0.0";
            
            if (BottomSheet != null)
            {
                BottomSheet.GoToBottomSheetState("Hidden");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
