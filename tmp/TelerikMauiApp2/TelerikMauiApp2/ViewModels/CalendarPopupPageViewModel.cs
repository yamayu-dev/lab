using CommunityToolkit.Mvvm.ComponentModel;

namespace TelerikMauiApp2.ViewModels
{
    public partial class CalendarPopupPageViewModel : ObservableObject
    {
        [ObservableProperty]
        private DateTime? _selectedDate = DateTime.Today;
    }
}
