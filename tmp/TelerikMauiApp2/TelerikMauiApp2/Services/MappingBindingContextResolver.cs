using System.Collections.Concurrent;

namespace TelerikMauiApp2.Services;

/// <summary>
/// MauiProgram で Register&lt;View,ViewModel&gt; した組み合わせに基づいて
/// Page の BindingContext を DI から解決して適用する。
/// 
/// - 文字列/名前規約に依存しない
/// - ViewModel の解決は IServiceProvider に委譲
/// </summary>
public sealed class MappingBindingContextResolver : IBindingContextResolver
{
    private readonly IServiceProvider _services;
    private readonly ConcurrentDictionary<Type, Type> _map = new();

    public MappingBindingContextResolver(IServiceProvider services)
    {
        _services = services;
    }

    public void Register<TView, TViewModel>() where TView : Page
    {
        _map[typeof(TView)] = typeof(TViewModel);
    }

    public bool TryApply(Page target)
    {
        if (target.BindingContext != null)
            return false;

        var viewType = target.GetType();
        if (!_map.TryGetValue(viewType, out var vmType))
            return false;

        var vm = _services.GetService(vmType);
        if (vm is null)
            return false;

        target.BindingContext = vm;
        return true;
    }
}
