namespace TelerikMauiApp2.Services;

public sealed class NavigationService : INavigationService
{
    public Task PushAsync(Page page)
    {
        var nav = Application.Current?.MainPage?.Navigation;
        if (nav is null)
        {
            throw new InvalidOperationException("Navigation is not available. Ensure the root page is a NavigationPage (or has a NavigationPage). ");
        }

        return nav.PushAsync(page);
    }

    public Task PopAsync()
    {
        var nav = Application.Current?.MainPage?.Navigation;
        if (nav is null)
        {
            throw new InvalidOperationException("Navigation is not available. Ensure the root page is a NavigationPage (or has a NavigationPage). ");
        }

        return nav.PopAsync();
    }
}
