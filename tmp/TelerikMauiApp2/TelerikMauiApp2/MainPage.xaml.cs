using TelerikMauiApp2.ViewModels;

namespace TelerikMauiApp2
{
    public partial class MainPage : ContentPage
    {
        public MainPage(MainPageViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
