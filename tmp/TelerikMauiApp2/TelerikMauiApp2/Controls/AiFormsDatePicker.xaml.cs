using AiForms.Dialogs;
using CommunityToolkit.Mvvm.Input;
using System.Runtime.CompilerServices;

namespace TelerikMauiApp2.Controls;

public partial class AiFormsDatePicker : ContentView
{
    public static readonly BindableProperty SelectedDateProperty =
        BindableProperty.Create(
            nameof(SelectedDate),
            typeof(DateTime?),
            typeof(AiFormsDatePicker),
            default(DateTime?),
            BindingMode.TwoWay);

    public DateTime? SelectedDate
    {
        get => (DateTime?)GetValue(SelectedDateProperty);
        set => SetValue(SelectedDateProperty, value);
    }

    public AiFormsDatePicker()
    {
        InitializeComponent();
    }

    [RelayCommand]
    private async Task OnShowCalendar()
    {
        var dialogView = new AiFormsDatePickerDialogView
        {
            SelectedDate = this.SelectedDate,
        };

        await Dialog.Instance.ShowAsync<AiFormsDatePickerDialogView>(dialogView);
    }

    [RelayCommand]
    private async Task OnCloseCalendar()
    {
        //_lastDialogNotifier?.Cancel();
    }
}