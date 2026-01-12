using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TelerikMauiApp2.Models;

namespace TelerikMauiApp2.ViewModels
{
    public partial class ProductSearchViewModel : ObservableObject
    {
        private readonly Action<Product?> _onClose;

        // サンプルデータ
        private readonly Product[] _allProducts = new[]
        {
            new Product { Code = "P001", Name = "ノートパソコン", Price = 120000 },
            new Product { Code = "P002", Name = "デスクトップPC", Price = 150000 },
            new Product { Code = "P003", Name = "マウス", Price = 3000 },
            new Product { Code = "P004", Name = "キーボード", Price = 8000 },
            new Product { Code = "P005", Name = "モニター", Price = 45000 },
            new Product { Code = "P006", Name = "プリンター", Price = 35000 },
            new Product { Code = "P007", Name = "スキャナー", Price = 25000 },
            new Product { Code = "P008", Name = "Webカメラ", Price = 12000 },
        };

        [ObservableProperty]
        private string searchText = string.Empty;

        [ObservableProperty]
        private ObservableCollection<Product> searchResults = new();

        [ObservableProperty]
        private string resultMessage = "商品名を入力してください";

        public bool HasResults => SearchResults.Any();
        public bool HasNoResults => !HasResults;

        public ProductSearchViewModel(Action<Product?> onClose)
        {
            _onClose = onClose;
        }

        partial void OnSearchTextChanged(string value)
        {
            PerformSearch();
        }

        private void PerformSearch()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                SearchResults.Clear();
                ResultMessage = "商品名を入力してください";
                OnPropertyChanged(nameof(HasResults));
                OnPropertyChanged(nameof(HasNoResults));
                return;
            }

            var results = _allProducts
                .Where(p => p.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                           p.Code.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                .ToList();

            SearchResults.Clear();
            foreach (var product in results)
            {
                SearchResults.Add(product);
            }

            ResultMessage = SearchResults.Any() 
                ? $"{SearchResults.Count}件の商品が見つかりました" 
                : "該当する商品が見つかりません";

            OnPropertyChanged(nameof(HasResults));
            OnPropertyChanged(nameof(HasNoResults));
        }

        [RelayCommand]
        private void SelectItem(Product product)
        {
            _onClose?.Invoke(product);
        }

        [RelayCommand]
        private void Close()
        {
            _onClose?.Invoke(null);
        }
    }
}
