#if IOS
using System;
using Microsoft.Maui.Hosting;

namespace TelerikMauiApp2.Platforms.iOS
{
    internal sealed class MopupsWindowLevelInitializer : IMauiInitializeService
    {
        public void Initialize(IServiceProvider services)
        {
            // 現時点では追加処理なし。必要なら将来ここでウィンドウ制御を行う。
        }
    }
}
#endif