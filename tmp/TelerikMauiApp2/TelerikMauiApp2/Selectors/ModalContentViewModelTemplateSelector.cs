using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;

namespace TelerikMauiApp2.Selectors;

/// <summary>
/// XAML 側で (ViewModel型, DataTemplate) の組を任意個登録できる汎用 DataTemplateSelector。
/// 
/// 例:
/// <selectors:ModalContentViewModelTemplateSelector>
///   <selectors:ModalContentViewModelTemplateSelector.Mappings>
///     <selectors:ViewModelTemplateMapping ViewModelType="{x:Type vm:ProductSearchViewModel}" Template="{StaticResource ProductSearchViewTemplate}" />
///   </selectors:ModalContentViewModelTemplateSelector.Mappings>
/// </selectors:ModalContentViewModelTemplateSelector>
/// </summary>
public sealed class ModalContentViewModelTemplateSelector : DataTemplateSelector
{
    public IList<ViewModelTemplateMapping> Mappings { get; } = new List<ViewModelTemplateMapping>();

    public DataTemplate? DefaultTemplate { get; set; }

    protected override DataTemplate? OnSelectTemplate(object item, BindableObject container)
    {
        var itemType = item.GetType();

        foreach (var m in Mappings)
        {
            if (m.ViewModelType is null || m.Template is null)
                continue;

            // item が ViewModelType と一致 or 派生なら採用
            if (m.ViewModelType.IsAssignableFrom(itemType))
                return m.Template;
        }

        return DefaultTemplate;
    }
}

public sealed class ViewModelTemplateMapping
{
    public Type? ViewModelType { get; set; }
    public DataTemplate? Template { get; set; }
}
