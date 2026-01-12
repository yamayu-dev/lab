using Telerik.Maui.Controls.BottomSheet;

namespace TelerikMauiApp2.Controls
{
    /// <summary>
    /// モーダルダイアログ的に使用するボトムシート。
    /// スワイプ無効、Hidden/Full の2状態のみ。
    /// 明示的な閉じるボタンでのみクローズする。
    /// </summary>
    public class ModalBottomSheet : BaseBottomSheet
    {
        private readonly BottomSheetState _hiddenState = new() { Name = "Hidden" };
        private readonly BottomSheetState _fullState = new() { Name = "Full" };

        public ModalBottomSheet()
        {
            // スワイプ無効（モーダルダイアログ的な動作のため）
            IsSwipeEnabled = false;

            // Hidden/Full の2状態のみ
            States.Clear();
            States.Add(_hiddenState);
            States.Add(_fullState);

            // 初期状態は Hidden
            InitialState = "Hidden";
            MinimumState = "Hidden";
            OpenStateName = "Full";

            // 自動クローズは無効（明示的なボタンでのみ閉じる）
            AutoCloseOnCollapse = false;

            // Hidden状態の時はタッチイベントを透過する
            InputTransparent = true;

            // 初期高さを反映
            UpdateStateHeights();
        }

        protected override void OnPropertyChanged(string? propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
            
            // State が変わったら InputTransparent を更新
            if (propertyName == nameof(State))
            {
                InputTransparent = State == "Hidden";
            }
        }

        // ==== Public surface ====

        public static readonly BindableProperty FullHeightProperty = BindableProperty.Create(
            nameof(FullHeight), typeof(BottomSheetLength), typeof(ModalBottomSheet), new BottomSheetLength(0.9, true),
            propertyChanged: (b, _, __) => ((ModalBottomSheet)b).UpdateStateHeights());

        public BottomSheetLength FullHeight
        {
            get => (BottomSheetLength)GetValue(FullHeightProperty);
            set => SetValue(FullHeightProperty, value);
        }

        private void UpdateStateHeights()
        {
            _hiddenState.Height = new BottomSheetLength(0, false);
            _fullState.Height = FullHeight;
        }
    }
}
