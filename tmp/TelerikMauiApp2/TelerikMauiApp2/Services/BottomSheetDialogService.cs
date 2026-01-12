using Microsoft.Maui.Controls;
using TelerikMauiApp2.Controls;

namespace TelerikMauiApp2.Services
{
    /// <summary>
    /// ボトムシートダイアログサービスの実装。
    /// ModalBottomSheetへの参照を保持し、ViewModelから操作可能にする。
    /// </summary>
    public class BottomSheetDialogService : IBottomSheetDialogService
    {
        private ModalBottomSheet? _modalSheet;
        private View? _currentContent;

        /// <summary>
        /// ModalBottomSheetへの参照を設定する。
        /// ページのコードビハインドから呼び出される。
        /// </summary>
        public void SetModalSheet(ModalBottomSheet modalSheet)
        {
            _modalSheet = modalSheet;
        }

        /// <summary>
        /// 指定されたContentViewをモーダルボトムシートで開く。
        /// </summary>
        public void Show(View content)
        {
            if (_modalSheet == null)
            {
                throw new InvalidOperationException("ModalSheet is not set. Call SetModalSheet first.");
            }

            _currentContent = content;
            
            // ModalSheetのSheetContentにContentViewを設定
            _modalSheet.SheetContent = content;
            
            // ModalSheetを開く
            _modalSheet.Open();
        }

        /// <summary>
        /// 現在開いているモーダルボトムシートを閉じる。
        /// </summary>
        public void Close()
        {
            if (_modalSheet == null)
            {
                return;
            }

            // ModalSheetを閉じる
            _modalSheet.Close();
            
            // コンテンツをクリア（メモリ解放）
            _modalSheet.SheetContent = null;
            _currentContent = null;
        }

        /// <summary>
        /// モーダルボトムシートが現在開いているかどうかを取得。
        /// </summary>
        public bool IsOpen => _currentContent != null;
    }
}
