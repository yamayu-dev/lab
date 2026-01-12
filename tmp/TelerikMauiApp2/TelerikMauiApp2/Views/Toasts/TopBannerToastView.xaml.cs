using AiForms.Dialogs;

namespace TelerikMauiApp2.Views.Toasts;

public partial class TopBannerToastView : ToastView
{
    public TopBannerToastView()
    {
        InitializeComponent();
    }

    // ここは「試すだけ」なのでアニメーションは未実装でもOK。
    // 必要なら TranslateTo/FadeTo で上からスッと出る動きにできます。
    public override void RunPresentationAnimation()
    {
    }

    public override void RunDismissalAnimation()
    {
    }

    public override void Destroy()
    {
    }
}
