using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TelerikMauiApp2.Models;

namespace TelerikMauiApp2.ViewModels
{
    public partial class CustomerSearchViewModel : ObservableObject
    {
        private readonly Action<Customer?> _onClose;

        // サンプルデータ
        private readonly Customer[] _allCustomers = new[]
        {
            new Customer { Code = "C001", Name = "株式会社ABC商事", Address = "東京都千代田区" },
            new Customer { Code = "C002", Name = "株式会社XYZ物産", Address = "大阪府大阪市" },
            new Customer { Code = "C003", Name = "山田商店", Address = "愛知県名古屋市" },
            new Customer { Code = "C004", Name = "鈴木工業", Address = "神奈川県横浜市" },
            new Customer { Code = "C005", Name = "田中電機", Address = "福岡県福岡市" },
            new Customer { Code = "C006", Name = "佐藤建設", Address = "北海道札幌市" },
        };

        [ObservableProperty]
        private string searchText = string.Empty;

        [ObservableProperty]
        private ObservableCollection<Customer> searchResults = new();

        [ObservableProperty]
        private string resultMessage = "得意先名を入力してください";

        public bool HasResults => SearchResults.Any();
        public bool HasNoResults => !HasResults;

        public CustomerSearchViewModel(Action<Customer?> onClose)
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
                ResultMessage = "得意先名を入力してください";
                OnPropertyChanged(nameof(HasResults));
                OnPropertyChanged(nameof(HasNoResults));
                return;
            }

            var results = _allCustomers
                .Where(c => c.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                           c.Code.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                .ToList();

            SearchResults.Clear();
            foreach (var customer in results)
            {
                SearchResults.Add(customer);
            }

            ResultMessage = SearchResults.Any() 
                ? $"{SearchResults.Count}件の得意先が見つかりました" 
                : "該当する得意先が見つかりません";

            OnPropertyChanged(nameof(HasResults));
            OnPropertyChanged(nameof(HasNoResults));
        }

        [RelayCommand]
        private void SelectItem(Customer customer)
        {
            _onClose?.Invoke(customer);
        }

        [RelayCommand]
        private void Close()
        {
            _onClose?.Invoke(null);
        }
    }
}
