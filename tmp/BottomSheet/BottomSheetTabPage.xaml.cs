using Telerik.Maui.Controls;

namespace TelerikMauiApp2.Views
{
    public partial class BottomSheetTabPage : ContentPage
    {
        public BottomSheetTabPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // ViewModelにBottomSheetの参照を渡す
            if (BindingContext is ViewModels.BottomSheetTabPageViewModel viewModel)
            {
                viewModel.BottomSheet = bottomSheet;
            }
        }
    }
}
