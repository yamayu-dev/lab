using Telerik.Maui.Controls.Compatibility;
using Mopups.Hosting;
using Microsoft.Maui.Controls.Xaml;
using CommunityToolkit.Maui;
using TelerikMauiApp2.Services;
using TelerikMauiApp2.ViewModels;
using TelerikMauiApp2.Views;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]

namespace TelerikMauiApp2
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseTelerik()
                .UseMauiApp<App>()
                .ConfigureMopups()
                .UseMauiCommunityToolkit(options =>
                {
                    // Windows で Snackbar/Toast を有効にする
                    options.SetShouldEnableSnackbarOnWindows(true);
                })
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Navigation (VMからページ遷移するため)
            builder.Services.AddSingleton<MappingBindingContextResolver>();
            builder.Services.AddSingleton<IBindingContextResolver>(sp => sp.GetRequiredService<MappingBindingContextResolver>());
            builder.Services.AddSingleton<INavigationService, NavigationService>();

            // Pages / VMs (DI一本化)
            builder.Services.AddTransient<MainPageViewModel>();
            builder.Services.AddTransient<MainPage>();
            
            // BottomSheetTabPage / ViewModel
            builder.Services.AddTransient<BottomSheetTabPageViewModel>();
            builder.Services.AddTransient<BottomSheetTabPage>();

            // FloatingToolbarPage / ViewModel
            builder.Services.AddTransient<FloatingToolbarPageViewModel>();
            builder.Services.AddTransient<FloatingToolbarPage>();

            // SideDrawerPage / ViewModel
            builder.Services.AddTransient<SideDrawerPageViewModel>();
            builder.Services.AddTransient<SideDrawerPage>();

            return builder.Build();
        }
    }
}
