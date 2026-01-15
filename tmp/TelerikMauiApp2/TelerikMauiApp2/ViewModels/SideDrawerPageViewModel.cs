using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TelerikMauiApp2.Models;

namespace TelerikMauiApp2.ViewModels;

public partial class SideDrawerPageViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<DrawerItemModel> items;

    [ObservableProperty]
    private DrawerItemModel? selectedItem;

    [ObservableProperty]
    private bool isDrawerOpen;

    public bool HasSelectedItem => SelectedItem != null;

    public SideDrawerPageViewModel()
    {
        // Initialize with sample data
        Items = new ObservableCollection<DrawerItemModel>
        {
            new DrawerItemModel
            {
                Icon = "📱",
                Title = "スマートフォン",
                Subtitle = "最新のモバイルデバイス",
                Description = "高性能なプロセッサと大容量バッテリーを搭載したスマートフォンです。",
                Category = "電子機器",
                CreatedDate = DateTime.Now.AddDays(-30),
                Status = "在庫あり"
            },
            new DrawerItemModel
            {
                Icon = "💻",
                Title = "ノートパソコン",
                Subtitle = "ビジネス向けPC",
                Description = "軽量で高性能なノートパソコン。長時間のバッテリー駆動が可能です。",
                Category = "電子機器",
                CreatedDate = DateTime.Now.AddDays(-15),
                Status = "在庫あり"
            },
            new DrawerItemModel
            {
                Icon = "⌚",
                Title = "スマートウォッチ",
                Subtitle = "健康管理デバイス",
                Description = "心拍数や睡眠の質を測定できるスマートウォッチです。",
                Category = "ウェアラブル",
                CreatedDate = DateTime.Now.AddDays(-7),
                Status = "予約受付中"
            },
            new DrawerItemModel
            {
                Icon = "🎧",
                Title = "ワイヤレスイヤホン",
                Subtitle = "ノイズキャンセリング対応",
                Description = "高音質でノイズキャンセリング機能を備えたワイヤレスイヤホンです。",
                Category = "オーディオ",
                CreatedDate = DateTime.Now.AddDays(-20),
                Status = "在庫あり"
            },
            new DrawerItemModel
            {
                Icon = "📷",
                Title = "デジタルカメラ",
                Subtitle = "プロ仕様の撮影機材",
                Description = "高解像度の写真と4K動画を撮影できるデジタルカメラです。",
                Category = "カメラ",
                CreatedDate = DateTime.Now.AddDays(-45),
                Status = "在庫僅少"
            },
            new DrawerItemModel
            {
                Icon = "🖥️",
                Title = "デスクトップPC",
                Subtitle = "ハイエンドワークステーション",
                Description = "クリエイティブワークに最適な高性能デスクトップパソコンです。",
                Category = "電子機器",
                CreatedDate = DateTime.Now.AddDays(-60),
                Status = "在庫あり"
            },
            new DrawerItemModel
            {
                Icon = "🎮",
                Title = "ゲームコンソール",
                Subtitle = "次世代ゲーム機",
                Description = "4K解像度とレイトレーシングに対応した次世代ゲーム機です。",
                Category = "ゲーム",
                CreatedDate = DateTime.Now.AddDays(-10),
                Status = "予約受付中"
            },
            new DrawerItemModel
            {
                Icon = "🖨️",
                Title = "プリンター",
                Subtitle = "多機能プリンター",
                Description = "印刷、スキャン、コピーが可能な多機能プリンターです。",
                Category = "周辺機器",
                CreatedDate = DateTime.Now.AddDays(-25),
                Status = "在庫あり"
            }
        };
    }

    partial void OnSelectedItemChanged(DrawerItemModel? value)
    {
        OnPropertyChanged(nameof(HasSelectedItem));
    }

    [RelayCommand]
    private void OpenDrawer(DrawerItemModel item)
    {
        SelectedItem = item;
        IsDrawerOpen = true;
    }

    [RelayCommand]
    private void CloseDrawer()
    {
        IsDrawerOpen = false;
    }

    [RelayCommand]
    private void ExecuteAction()
    {
        if (SelectedItem != null)
        {
            // Execute action on selected item
            // For now, just close the drawer
            IsDrawerOpen = false;
        }
    }
}
