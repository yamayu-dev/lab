using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace TelerikMauiApp2.Controls;

/// <summary>
/// ModalBottomSheet の SheetContent 内で「ViewModel(object) -> View(DataTemplate)」を解決するためのホスト。
/// 
/// - DataTemplateSelector が ContentPresenter に存在しない MAUI バージョンでも確実に動かすため、
///   生成した View を Content として差し込む。
/// - 生成した View の BindingContext は ViewModel に固定。
/// </summary>
public class ModalBottomSheetTemplateHost : ContentView
{
    public static readonly BindableProperty ViewModelProperty = BindableProperty.Create(
        nameof(ViewModel),
        typeof(object),
        typeof(ModalBottomSheetTemplateHost),
        default(object),
        propertyChanged: (_, __, ___) => ((ModalBottomSheetTemplateHost)_).Refresh());

    public object? ViewModel
    {
        get => GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public static readonly BindableProperty TemplateSelectorProperty = BindableProperty.Create(
        nameof(TemplateSelector),
        typeof(DataTemplateSelector),
        typeof(ModalBottomSheetTemplateHost),
        default(DataTemplateSelector),
        propertyChanged: (_, __, ___) => ((ModalBottomSheetTemplateHost)_).Refresh());

    public DataTemplateSelector? TemplateSelector
    {
        get => (DataTemplateSelector?)GetValue(TemplateSelectorProperty);
        set => SetValue(TemplateSelectorProperty, value);
    }

    void Refresh()
    {
        // VM が無いなら表示しない
        if (ViewModel is null)
        {
            Content = null;
            return;
        }

        // Selector が無いなら表示しない（後で DefaultTemplate を足す余地はある）
        if (TemplateSelector is null)
        {
            Content = CreateFallback($"TemplateSelector is null. VM: {ViewModel.GetType().FullName}");
            return;
        }

        var template = TemplateSelector.SelectTemplate(ViewModel, this);
        if (template is null)
        {
            Content = CreateFallback($"No DataTemplate found for VM: {ViewModel.GetType().FullName}");
            return;
        }

        var created = template.CreateContent();

        // CreateContent は View or ViewCell を返す可能性がある
        View? view = created switch
        {
            View v => v,
            ViewCell cell => cell.View,
            _ => null
        };

        if (view is null)
        {
            Content = CreateFallback($"Template.CreateContent() did not produce a View/ViewCell. VM: {ViewModel.GetType().FullName}");
            return;
        }

        view.BindingContext = ViewModel;
        Content = view;
    }

    static View CreateFallback(string message)
    {
        return new Grid
        {
            BackgroundColor = Colors.White,
            Padding = new Thickness(16),
            Children =
            {
                new VerticalStackLayout
                {
                    Spacing = 8,
                    Children =
                    {
                        new Label { Text = "Modal content is empty", FontAttributes = FontAttributes.Bold, FontSize = 18, TextColor = Colors.Black },
                        new Label { Text = message, FontSize = 12, TextColor = Colors.Gray }
                    }
                }
            }
        };
    }
}
