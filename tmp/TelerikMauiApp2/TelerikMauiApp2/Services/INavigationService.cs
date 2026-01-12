namespace TelerikMauiApp2.Services;

public interface INavigationService
{
    Task PushAsync(Page page);
    Task PopAsync();
}
