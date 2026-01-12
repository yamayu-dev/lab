using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TelerikMauiApp2.Controls;
using TelerikMauiApp2.Models;

namespace TelerikMauiApp2.ViewModels;

public enum DockTab
{
    Home,
    Settings,
    Info
}

public partial class BottomSheetTabPageViewModel : ObservableObject
{
    public IReadOnlyList<FloatingTabItemModel> DockTabs { get; }

    [ObservableProperty]
    private string selectedInfo = "BottomSheetを開いてタブを選択してください";

    [ObservableProperty]
    private string homeText = "";

    [ObservableProperty]
    private bool notificationEnabled = true;

    [ObservableProperty]
    private double volumeValue = 50;

    [ObservableProperty]
    private DockTab selectedDockTab = DockTab.Home;

    public int DockSelectedIndex
    {
        get => (int)SelectedDockTab;
        set
        {
            var clamped = Math.Clamp(value, 0, 2);
            var tab = (DockTab)clamped;
            if (tab == SelectedDockTab) return;
            SelectedDockTab = tab;
            OnPropertyChanged();
        }
    }

    [ObservableProperty]
    private bool isDockHomeVisible = true;

    [ObservableProperty]
    private bool isDockSettingsVisible;

    [ObservableProperty]
    private bool isDockInfoVisible;

    [ObservableProperty]
    private string selectedProductInfo = "";

    [ObservableProperty]
    private string selectedCustomerInfo = "";

    [ObservableProperty]
    private string modalState = "Hidden";

    // Modal content ViewModel (DataTemplate will select the appropriate view)
    [ObservableProperty]
    private object? modalContentViewModel;

    public BottomSheetTabPageViewModel()
    {
        DockTabs = new List<FloatingTabItemModel>
        {
            new() { Text = "Home", IconText = "🏠", Command = SelectDockTabCommand, CommandParameter = DockTab.Home },
            new() { Text = "Settings", IconText = "⚙", Command = SelectDockTabCommand, CommandParameter = DockTab.Settings },
            new() { Text = "Info", IconText = "ℹ", Command = SelectDockTabCommand, CommandParameter = DockTab.Info },
        };

        ApplyDockSelection();
    }

    partial void OnSelectedDockTabChanged(DockTab value)
    {
        ApplyDockSelection();
        OnPropertyChanged(nameof(DockSelectedIndex));
    }

    [RelayCommand]
    private void SelectDockTab(DockTab tab)
    {
        SelectedDockTab = tab;
    }

    private void ApplyDockSelection()
    {
        IsDockHomeVisible = SelectedDockTab == DockTab.Home;
        IsDockSettingsVisible = SelectedDockTab == DockTab.Settings;
        IsDockInfoVisible = SelectedDockTab == DockTab.Info;
    }

    [RelayCommand]
    private void SelectHome()
    {
        var text = string.IsNullOrEmpty(HomeText) ? "(空)" : HomeText;
        SelectedInfo = $"ホームタブで選択されました: {text}";
    }

    [RelayCommand]
    private void SelectSettings()
    {
        SelectedInfo = $"設定タブで保存されました - 通知: {(NotificationEnabled ? "ON" : "OFF")}, ボリューム: {VolumeValue:F0}";
    }

    [RelayCommand]
    private void SelectInfo()
    {
        SelectedInfo = "情報タブが確認されました - バージョン: 1.0.0";
    }

    [RelayCommand]
    private void OpenProductSearch()
    {
        // Set ViewModel and let DataTemplate select the view
        ModalContentViewModel = new ProductSearchViewModel(OnProductSearchClosed);
        ModalState = "Full";
    }

    [RelayCommand]
    private void OpenCustomerSearch()
    {
        // Set ViewModel and let DataTemplate select the view
        ModalContentViewModel = new CustomerSearchViewModel(OnCustomerSearchClosed);
        ModalState = "Full";
    }

    private void OnProductSearchClosed(Product? product)
    {
        ModalState = "Hidden";
        ModalContentViewModel = null;
        
        if (product != null)
        {
            SelectedProductInfo = $"商品: {product.Name} ({product.Code}) - ¥{product.Price:N0}";
            SelectedInfo = SelectedProductInfo;
        }
    }

    private void OnCustomerSearchClosed(Customer? customer)
    {
        ModalState = "Hidden";
        ModalContentViewModel = null;
        
        if (customer != null)
        {
            SelectedCustomerInfo = $"得意先: {customer.Name} ({customer.Code}) - {customer.Address}";
            SelectedInfo = SelectedCustomerInfo;
        }
    }

    [RelayCommand]
    private void CloseModal()
    {
        ModalState = "Hidden";
        ModalContentViewModel = null;
    }
}
