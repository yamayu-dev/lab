using TelerikMauiApp2.Services;
using TelerikMauiApp2.ViewModels;

namespace TelerikMauiApp2.Views
{
    public partial class BottomSheetTabPage : ContentPage
    {
        public BottomSheetTabPage() : this(null, null)
        {
        }
        
        public BottomSheetTabPage(BottomSheetTabPageViewModel? viewModel, IBottomSheetDialogService? bottomSheetService)
        {
            InitializeComponent();
            
            IBottomSheetDialogService serviceToUse;
            
            if (bottomSheetService == null)
            {
                serviceToUse = new BottomSheetDialogService();
            }
            else
            {
                serviceToUse = bottomSheetService;
            }
            
            if (viewModel == null)
            {
                viewModel = new BottomSheetTabPageViewModel(serviceToUse);
            }
            
            BindingContext = viewModel;
            
            if (serviceToUse is BottomSheetDialogService service)
            {
                service.SetModalSheet(ModalSheet);
            }
            
            viewModel.ModalSheet = ModalSheet;
        }
    }
}
