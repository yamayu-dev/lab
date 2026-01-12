using Telerik.Maui.Controls.BottomSheet;
using Microsoft.Maui.Controls;

namespace TelerikMauiApp2.Controls
{
    /// <summary>
    /// XAMLなしのシンプルな RadBottomSheet 継承版。
    /// MainContent / SheetContent を差し込み、外部から Open/Close/Toggle コマンドで操作可能。
    /// Peek/Partial/Full の3ステートを固定で持ち、Hidden へ落ちないよう最小状態でクローズする。
    /// </summary>
    public class SmartBottomSheet : BaseBottomSheet
    {
        private readonly BottomSheetState _peekState = new() { Name = "Peek" };
        private readonly BottomSheetState _partialState = new() { Name = "Partial" };
        private readonly BottomSheetState _fullState = new() { Name = "Full" };

        public SmartBottomSheet()
        {
            IsSwipeEnabled = true;
            States.Clear();
            States.Add(_peekState);
            States.Add(_partialState);
            States.Add(_fullState);

            // 初期高さを反映
            UpdateStateHeights();
        }

        // ==== Public surface ====

        public static readonly BindableProperty PeekHeightProperty = BindableProperty.Create(
            nameof(PeekHeight), typeof(BottomSheetLength), typeof(SmartBottomSheet), new BottomSheetLength(80, false),
            propertyChanged: (b, _, __) => ((SmartBottomSheet)b).UpdateStateHeights());

        public BottomSheetLength PeekHeight
        {
            get => (BottomSheetLength)GetValue(PeekHeightProperty);
            set => SetValue(PeekHeightProperty, value);
        }

        public static readonly BindableProperty PartialHeightProperty = BindableProperty.Create(
            nameof(PartialHeight), typeof(BottomSheetLength), typeof(SmartBottomSheet), BottomSheetLength.Partial,
            propertyChanged: (b, _, __) => ((SmartBottomSheet)b).UpdateStateHeights());

        public BottomSheetLength PartialHeight
        {
            get => (BottomSheetLength)GetValue(PartialHeightProperty);
            set => SetValue(PartialHeightProperty, value);
        }

        public static readonly BindableProperty FullHeightProperty = BindableProperty.Create(
            nameof(FullHeight), typeof(BottomSheetLength), typeof(SmartBottomSheet), BottomSheetLength.Full,
            propertyChanged: (b, _, __) => ((SmartBottomSheet)b).UpdateStateHeights());

        public BottomSheetLength FullHeight
        {
            get => (BottomSheetLength)GetValue(FullHeightProperty);
            set => SetValue(FullHeightProperty, value);
        }

        private void UpdateStateHeights()
        {
            _peekState.Height = PeekHeight;
            _partialState.Height = PartialHeight;
            _fullState.Height = FullHeight;
        }
    }
}
