namespace TelerikMauiApp2
{
    public partial class App : Application
    {
        private readonly IServiceProvider _services;

        public App(IServiceProvider services)
        {
            InitializeComponent();

            _services = services;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var mainPage = _services.GetService(typeof(MainPage)) as MainPage
                           ?? throw new InvalidOperationException("MainPage is not registered in DI.");

            return new Window(new NavigationPage(mainPage));
        }
    }
}
