using Microsoft.UI.Xaml;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TelerikMauiApp2.WinUI
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : MauiWinUIApplication
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"[UnhandledException] {e.ExceptionObject}");
                    System.Console.WriteLine($"[UnhandledException] {e.ExceptionObject}");
                }
                catch
                {
                    // best effort
                }
            };

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"[UnobservedTaskException] {e.Exception}");
                    System.Console.WriteLine($"[UnobservedTaskException] {e.Exception}");
                }
                catch
                {
                    // best effort
                }
            };

            this.InitializeComponent();
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }

}
