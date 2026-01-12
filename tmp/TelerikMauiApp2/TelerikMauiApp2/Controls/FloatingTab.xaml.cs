using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace TelerikMauiApp2.Controls;

public partial class FloatingTab : ContentView
{
    private INotifyCollectionChanged? _notifyCollection;
    private List<FloatingTabItemModel> _cachedItems = new();

    public FloatingTab()
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
            typeof(FloatingTab),
            propertyChanged: (b, _, __) => ((FloatingTab)b).OnItemsSourceChanged());

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
            typeof(FloatingTab),
            -1,
            BindingMode.TwoWay);

    public int SelectedIndex
    {
        get => (int)GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    /// <summary>
    /// 選択中のアイテム（未選択は null）。
    /// </summary>
    public static readonly BindableProperty SelectedItemProperty =
        BindableProperty.Create(
            nameof(SelectedItem),
            typeof(object),
            typeof(FloatingTab),
            default,
            BindingMode.TwoWay);

    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);

        if (propertyName == SelectedIndexProperty.PropertyName || propertyName == SelectedItemProperty.PropertyName)
        {
            ApplySelectionToItems();
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

        ApplySelectionToItems();
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        _cachedItems = ItemsSource?.OfType<FloatingTabItemModel>().ToList() ?? new List<FloatingTabItemModel>();
        ApplySelectionToItems();
    }

    private void ApplySelectionToItems()
    {
        if (_cachedItems.Count == 0)
            return;

        var selectedItem = SelectedItem;
        var selectedIndex = SelectedIndex;

        // SelectedItem 優先。null のときは SelectedIndex を使う。
        for (int i = 0; i < _cachedItems.Count; i++)
        {
            var it = _cachedItems[i];
            var isSel = selectedItem != null ? ReferenceEquals(it, selectedItem) : i == selectedIndex;
            it.IsSelected = isSel;
        }
    }

    /// <summary>
    /// 既定の見た目用: アイテムの幅。
    /// </summary>
    public static readonly BindableProperty ItemWidthProperty =
        BindableProperty.Create(
            nameof(ItemWidth),
            typeof(double),
            typeof(FloatingTab),
            64d);

    public double ItemWidth
    {
        get => (double)GetValue(ItemWidthProperty);
        set => SetValue(ItemWidthProperty, value);
    }

    /// <summary>
    /// 既定の見た目用: アイテムの高さ。
    /// </summary>
    public static readonly BindableProperty ItemHeightProperty =
        BindableProperty.Create(
            nameof(ItemHeight),
            typeof(double),
            typeof(FloatingTab),
            52d);

    public double ItemHeight
    {
        get => (double)GetValue(ItemHeightProperty);
        set => SetValue(ItemHeightProperty, value);
    }
}
