using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.Maui.Controls.Shapes;
using Telerik.Maui.Controls;

namespace TelerikMauiApp2.Controls;

public partial class FloatingToolbar : ContentView
{
    private const string DefaultRadioGroupName = "FloatingToolbar";

    private INotifyCollectionChanged? _notifyCollection;
    private List<FloatingTabItemModel> _cachedItems = new();
    private readonly List<RadioButtonToolbarItem> _toolbarItems = new();

    // Telerik の RadioButtonToolbarItem を使い、選択状態を標準で表現する
    private static readonly Color ItemForeground = Colors.White;
    private static readonly Color SelectedBackground = Color.FromArgb("#2B4CFF");
    private static readonly Color UnselectedBackground = Colors.Transparent;
    private static readonly Color ItemBorderTransparent = Colors.Transparent;

    public FloatingToolbar()
    {
        InitializeComponent();
    }

    /// <summary>
    /// タブ要素（FloatingTabItemModel）の列。
    /// </summary>
    public static readonly BindableProperty ItemsSourceProperty =
        BindableProperty.Create(
            nameof(ItemsSource),
            typeof(IEnumerable),
            typeof(FloatingToolbar),
            propertyChanged: (b, _, __) => ((FloatingToolbar)b).OnItemsSourceChanged());

    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>
    /// 選択中のインデックス（未選択は -1）。
    /// </summary>
    public static readonly BindableProperty SelectedIndexProperty =
        BindableProperty.Create(
            nameof(SelectedIndex),
            typeof(int),
            typeof(FloatingToolbar),
            -1,
            BindingMode.TwoWay,
            propertyChanged: (b, _, __) => ((FloatingToolbar)b).ApplySelectionToToolbarItems());

    public int SelectedIndex
    {
        get => (int)GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);

        if (propertyName == SelectedIndexProperty.PropertyName)
        {
            ApplySelectionToToolbarItems();
        }
    }

    private void OnItemsSourceChanged()
    {
        if (_notifyCollection != null)
        {
            _notifyCollection.CollectionChanged -= OnCollectionChanged;
            _notifyCollection = null;
        }

        _cachedItems = ItemsSource?.OfType<FloatingTabItemModel>().ToList() ?? new List<FloatingTabItemModel>();

        _notifyCollection = ItemsSource as INotifyCollectionChanged;
        if (_notifyCollection != null)
        {
            _notifyCollection.CollectionChanged += OnCollectionChanged;
        }

        RebuildToolbarItems();
        ApplySelectionToToolbarItems();
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        _cachedItems = ItemsSource?.OfType<FloatingTabItemModel>().ToList() ?? new List<FloatingTabItemModel>();
        RebuildToolbarItems();
        ApplySelectionToToolbarItems();
    }

    private void RebuildToolbarItems()
    {
        Toolbar.Items.Clear();
        _toolbarItems.Clear();

        if (_cachedItems.Count == 0)
        {
            return;
        }

        for (var i = 0; i < _cachedItems.Count; i++)
        {
            var model = _cachedItems[i];

            var item = new RadioButtonToolbarItem
            {
                // Text が描画されないケースがあるため、まずは Text を必ず設定する。
                Text = CreateItemText(model),
                GroupName = DefaultRadioGroupName,
                Command = model.Command,
                CommandParameter = model.CommandParameter,
            };

            item.Style = CreateRadioToolbarItemStyle();

            // クリックで SelectedIndex を更新（VM 側が TwoWay binding で追従できるように）。
            var capturedIndex = i;
            item.Clicked += (_, __) => SelectedIndex = capturedIndex;

            _toolbarItems.Add(item);
            Toolbar.Items.Add(item);
        }

        ApplySelectionToToolbarItems();
    }

    private static string CreateItemText(FloatingTabItemModel model)
    {
        var text = model.Text ?? string.Empty;
        if (string.IsNullOrWhiteSpace(model.IconText))
            return text;

        if (string.IsNullOrWhiteSpace(text))
            return model.IconText;

        return $"{model.IconText} {text}";
    }

    private static Style CreateRadioToolbarItemStyle()
    {
        // RadioButtonToolbarItemView に対し、Text/背景/角丸を付ける。
        // DisplayOptions の存在に依存しない（アイコンは Text に合成）。
        var style = new Style(typeof(RadioButtonToolbarItemView));
        style.Setters.Add(new Setter { Property = RadioButtonToolbarItemView.TextColorProperty, Value = ItemForeground });
        style.Setters.Add(new Setter { Property = RadioButtonToolbarItemView.BackgroundColorProperty, Value = UnselectedBackground });
        style.Setters.Add(new Setter { Property = RadioButtonToolbarItemView.BorderColorProperty, Value = ItemBorderTransparent });
        style.Setters.Add(new Setter { Property = RadioButtonToolbarItemView.BorderThicknessProperty, Value = 0d });
        style.Setters.Add(new Setter { Property = RadioButtonToolbarItemView.CornerRadiusProperty, Value = 14d });
        style.Setters.Add(new Setter { Property = RadioButtonToolbarItemView.PaddingProperty, Value = new Thickness(12, 10) });

        // 選択時だけ背景色を付ける
        var trigger = new DataTrigger(typeof(RadioButtonToolbarItemView))
        {
            Binding = new Binding("IsSelected"),
            Value = true,
        };
        trigger.Setters.Add(new Setter { Property = RadioButtonToolbarItemView.BackgroundColorProperty, Value = SelectedBackground });
        style.Triggers.Add(trigger);

        return style;
    }

    private void ApplySelectionToToolbarItems()
    {
        if (_toolbarItems.Count == 0)
            return;

        for (var i = 0; i < _toolbarItems.Count; i++)
        {
            _toolbarItems[i].IsSelected = i == SelectedIndex;
        }

        // 互換のため、ItemsSource 側の IsSelected も同期しておく（FloatingTab と同じ）。
        for (var i = 0; i < _cachedItems.Count; i++)
        {
            _cachedItems[i].IsSelected = i == SelectedIndex;
        }
    }
}
