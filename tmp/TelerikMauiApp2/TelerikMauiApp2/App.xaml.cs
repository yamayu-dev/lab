namespace TelerikMauiApp2
{
    public partial class App : Application
    {
        private readonly IServiceProvider _services;
        private readonly Services.IBindingContextResolver _bindingContextResolver;

        private bool _bindingMapInitialized;

        public App(IServiceProvider services, Services.IBindingContextResolver bindingContextResolver)
        {
            InitializeComponent();

            _services = services;
            _bindingContextResolver = bindingContextResolver;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            if (!_bindingMapInitialized)
            {
                // 名前規約ではなく、MauiProgramで登録した組合せと同じ考え方で明示マッピングする
                // (ここで1回だけ)
                _bindingContextResolver.Register<MainPage, ViewModels.MainPageViewModel>();
                _bindingContextResolver.Register<Views.BottomSheetTabPage, ViewModels.BottomSheetTabPageViewModel>();
                _bindingMapInitialized = true;
            }

            var mainPage = _services.GetService(typeof(MainPage)) as MainPage
                           ?? throw new InvalidOperationException("MainPage is not registered in DI.");

            // 起動時も NavigationService と同じく、規約で BindingContext を自動解決して統一する
            _bindingContextResolver.TryApply(mainPage);

            return new Window(new NavigationPage(mainPage));
        }
    }
}
