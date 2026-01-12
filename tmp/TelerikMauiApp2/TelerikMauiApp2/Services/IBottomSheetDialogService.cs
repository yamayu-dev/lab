using Microsoft.Maui.Controls;

namespace TelerikMauiApp2.Services
{
    /// <summary>
    /// ボトムシートダイアログサービスのインターフェース。
    /// ViewModelから動的にモーダルボトムシートを開くことができる。
    /// </summary>
    public interface IBottomSheetDialogService
    {
        /// <summary>
        /// 指定されたContentViewをモーダルボトムシートで開く。
        /// </summary>
        /// <param name="content">表示するContentView</param>
        void Show(View content);

        /// <summary>
        /// 現在開いているモーダルボトムシートを閉じる。
        /// </summary>
        void Close();

        /// <summary>
        /// モーダルボトムシートが現在開いているかどうかを取得。
        /// </summary>
        bool IsOpen { get; }
    }
}
