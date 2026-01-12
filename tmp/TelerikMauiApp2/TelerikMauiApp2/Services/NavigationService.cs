namespace TelerikMauiApp2.Services;

public sealed class NavigationService : INavigationService
{
    private readonly IServiceProvider _services;
    private readonly IBindingContextResolver _bindingContextResolver;

    public NavigationService(IServiceProvider services, IBindingContextResolver bindingContextResolver)
    {
        _services = services;
        _bindingContextResolver = bindingContextResolver;
    }

    public Task PushAsync(Page page)
    {
        var nav = Application.Current?.MainPage?.Navigation;
        if (nav is null)
        {
            throw new InvalidOperationException("Navigation is not available. Ensure the root page is a NavigationPage (or has a NavigationPage). ");
        }

        return nav.PushAsync(page);
    }

    public Task PushAsync<TPage>() where TPage : Page
    {
        var page = _services.GetService(typeof(TPage)) as TPage
                   ?? throw new InvalidOperationException($"{typeof(TPage).Name} is not registered in DI.");

        // Page側のコンストラクタで BindingContext を設定しない方針の場合は、ここで規約適用する。
        _bindingContextResolver.TryApply(page);

        return PushAsync(page);
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
