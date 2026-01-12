using System.Windows.Input;

namespace TelerikMauiApp2.Controls;

/// <summary>
/// FloatingTab 用の表示/操作モデル。
/// ViewModel からこのモデル群を ItemsSource に渡して使う想定。
/// </summary>
public sealed class FloatingTabItemModel : BindableObject
{
    public string? Text { get; set; }
    public string? IconText { get; set; }

    public ICommand? Command { get; set; }
    public object? CommandParameter { get; set; }

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value) return;
            _isSelected = value;
            OnPropertyChanged();
        }
    }
}
