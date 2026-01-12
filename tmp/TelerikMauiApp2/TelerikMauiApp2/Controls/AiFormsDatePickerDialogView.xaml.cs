using AiForms.Dialogs;
using CommunityToolkit.Mvvm.Input;

namespace TelerikMauiApp2.Controls;

public partial class AiFormsDatePickerDialogView : DialogView
{
    public static readonly BindableProperty SelectedDateProperty =
        BindableProperty.Create(
            nameof(SelectedDate),
            typeof(DateTime?),
            typeof(AiFormsDatePickerDialogView),
            default(DateTime?),
            BindingMode.TwoWay);

    public DateTime? SelectedDate
    {
        get => (DateTime?)GetValue(SelectedDateProperty);
        set => SetValue(SelectedDateProperty, value);
    }

    public AiFormsDatePickerDialogView()
    {
        InitializeComponent();
    }

    [RelayCommand]
    private void OnClose()
    {
        DialogNotifier.Cancel();
    }
}
