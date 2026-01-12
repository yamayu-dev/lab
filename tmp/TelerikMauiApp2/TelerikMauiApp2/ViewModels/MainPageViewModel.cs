using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mopups.Services;
using TelerikMauiApp2.Services;
using TelerikMauiApp2.Views;

namespace TelerikMauiApp2.ViewModels
{
    public partial class MainPageViewModel : ObservableObject
    {
        private readonly INavigationService _navigation;

        public MainPageViewModel(INavigationService navigation)
        {
            _navigation = navigation;
        }

        [RelayCommand]
        private async Task OpenPopup()
        {
            var popupPage = new CalendarPopupPage();
            await MopupService.Instance.PushAsync(popupPage);
        }

        [RelayCommand]
        private async Task OpenMopupsOnlyPopup()
        {
            var popupPage = new MopupsCalendarPopupPage();
            await MopupService.Instance.PushAsync(popupPage);
        }

        [RelayCommand]
        private Task OpenBottomSheetTab()
        {
            // BottomSheetTabPage 自体は通常の ContentPage。表示はそのページ内の RadBottomSheet が担当。
            return _navigation.PushAsync<BottomSheetTabPage>();
        }

        [RelayCommand]
        private Task OpenTopBannerTest()
        {
            return _navigation.PushAsync(new TopBannerTestPage());
        }
    }
}
