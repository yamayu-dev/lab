using System.Windows.Input;
using Telerik.Maui.Controls.BottomSheet;

namespace TelerikMauiApp2.Behaviors;

public sealed class BottomSheetStateChangingToCommandBehavior : Behavior<Telerik.Maui.Controls.RadBottomSheet>
{
    public static readonly BindableProperty CommandProperty = BindableProperty.Create(
        nameof(Command),
        typeof(ICommand),
        typeof(BottomSheetStateChangingToCommandBehavior));

    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    protected override void OnAttachedTo(Telerik.Maui.Controls.RadBottomSheet bindable)
    {
        base.OnAttachedTo(bindable);
        bindable.StateChanging += OnStateChanging;
    }

    protected override void OnDetachingFrom(Telerik.Maui.Controls.RadBottomSheet bindable)
    {
        bindable.StateChanging -= OnStateChanging;
        base.OnDetachingFrom(bindable);
    }

    private void OnStateChanging(object sender, BottomSheetStateChangingEventArgs e)
    {
        var command = Command;
        if (command == null)
        {
            return;
        }

        // ViewModel 側で BottomSheetStateChangingEventArgs を直接受け取って判定できる。
        if (command.CanExecute(e))
        {
            command.Execute(e);
        }
    }
}
