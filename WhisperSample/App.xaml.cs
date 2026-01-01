using System.Diagnostics;

namespace WhisperSample
{
    public partial class App : Application
    {
    public static Action<string>? ReportFatal { get; set; }

        public App()
        {
            InitializeComponent();

            HookGlobalExceptionHandlers();

            try
            {
                Debug.WriteLine($"[Startup] Process: {Environment.ProcessId}");
                Debug.WriteLine($"[Startup] OS: {DeviceInfo.Platform} {DeviceInfo.VersionString}");
                Debug.WriteLine($"[Startup] DebuggerAttached: {Debugger.IsAttached}");
            }
            catch { }

            // 画面に出すための簡易コールバック（MainPage から差し替え可能）
            ReportFatal ??= (msg) => Debug.WriteLine(msg);
        }

        private static void HookGlobalExceptionHandlers()
        {
            AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            {
                try
                {
                    var ex = e.ExceptionObject as Exception;
                    var msg = ex != null ? ex.ToString() : (e.ExceptionObject?.ToString() ?? "<unknown>");
                    Debug.WriteLine("[UnhandledException]\n" + msg);
                    ReportFatal?.Invoke("[UnhandledException]\n" + msg);
                }
                catch { }
            };

            TaskScheduler.UnobservedTaskException += (_, e) =>
            {
                try
                {
                    Debug.WriteLine("[UnobservedTaskException]\n" + e.Exception);
                    ReportFatal?.Invoke("[UnobservedTaskException]\n" + e.Exception);
                }
                catch { }

                e.SetObserved();
            };
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}