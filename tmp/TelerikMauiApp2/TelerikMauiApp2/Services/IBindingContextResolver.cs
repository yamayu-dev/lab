namespace TelerikMauiApp2.Services;

/// <summary>
/// ページの BindingContext を「規約」で解決するためのサービス。
/// 例: FooPage -> FooPageViewModel を DI から取得してセットする。
/// </summary>
public interface IBindingContextResolver
{
    /// <summary>
    /// 解決できた場合は target.BindingContext をセットして true を返す。
    /// すでに BindingContext がある場合は何もしない（false）。
    /// </summary>
    bool TryApply(Page target);

    /// <summary>
    /// App起動時やサービス初期化時に、View と ViewModel の対応を登録する。
    /// </summary>
    void Register<TView, TViewModel>() where TView : Page;
}
