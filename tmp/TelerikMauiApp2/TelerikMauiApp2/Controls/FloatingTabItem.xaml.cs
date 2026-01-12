using System.Windows.Input;

namespace TelerikMauiApp2.Controls;

public partial class FloatingTabItem : ContentView
{
    public FloatingTabItem()
    {
        InitializeComponent();
        UpdateVisualState();
    }

    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(FloatingTabItem), default(string));

    public string? Text
    {
        get => (string?)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly BindableProperty IconTextProperty =
        BindableProperty.Create(nameof(IconText), typeof(string), typeof(FloatingTabItem), default(string));

    public string? IconText
    {
        get => (string?)GetValue(IconTextProperty);
        set => SetValue(IconTextProperty, value);
    }

    public static readonly BindableProperty CommandProperty =
        BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(FloatingTabItem));

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public static readonly BindableProperty CommandParameterProperty =
        BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(FloatingTabItem));

    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    public static readonly BindableProperty IsSelectedProperty =
        BindableProperty.Create(
            nameof(IsSelected),
            typeof(bool),
            typeof(FloatingTabItem),
            false,
            BindingMode.TwoWay,
            propertyChanged: (b, _, __) => ((FloatingTabItem)b).UpdateVisualState());

    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    public static readonly BindableProperty SelectedBackgroundColorProperty =
        BindableProperty.Create(nameof(SelectedBackgroundColor), typeof(Color), typeof(FloatingTabItem), Color.FromArgb("#512BD4"));

    public Color SelectedBackgroundColor
    {
        get => (Color)GetValue(SelectedBackgroundColorProperty);
        set => SetValue(SelectedBackgroundColorProperty, value);
    }

    public static readonly BindableProperty UnselectedBackgroundColorProperty =
        BindableProperty.Create(nameof(UnselectedBackgroundColor), typeof(Color), typeof(FloatingTabItem), Color.FromArgb("#2C2C2E"));

    public Color UnselectedBackgroundColor
    {
        get => (Color)GetValue(UnselectedBackgroundColorProperty);
        set => SetValue(UnselectedBackgroundColorProperty, value);
    }

    public Color BackgroundColor => IsSelected ? SelectedBackgroundColor : UnselectedBackgroundColor;

    private void OnClicked(object sender, EventArgs e)
    {
        // コマンド実行（VMで切替する想定）
        if (Command?.CanExecute(CommandParameter) == true)
        {
            Command.Execute(CommandParameter);
        }
    }

    private void UpdateVisualState()
    {
        OnPropertyChanged(nameof(BackgroundColor));
    }
}
