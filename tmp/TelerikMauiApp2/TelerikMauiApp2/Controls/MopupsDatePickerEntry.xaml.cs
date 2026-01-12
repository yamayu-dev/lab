using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Mopups.Pages;
using Mopups.Services;
using Telerik.Maui.Controls;

namespace TelerikMauiApp2.Controls
{
    public partial class MopupsDatePickerEntry : Label
    {
        public static readonly BindableProperty SelectedDateProperty =
            BindableProperty.Create(nameof(SelectedDate), typeof(DateTime?), typeof(MopupsDatePickerEntry), DateTime.Today,
                BindingMode.TwoWay, propertyChanged: OnSelectedDateChanged);

        public static readonly BindableProperty PlaceholderProperty =
            BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(MopupsDatePickerEntry),
                "日付を選択するにはタップしてください", propertyChanged: OnPlaceholderChanged);

        public DateTime? SelectedDate
        {
            get => (DateTime?)GetValue(SelectedDateProperty);
            set => SetValue(SelectedDateProperty, value);
        }

        public string Placeholder
        {
            get => (string)GetValue(PlaceholderProperty);
            set => SetValue(PlaceholderProperty, value);
        }

        public MopupsDatePickerEntry()
        {
            InitializeComponent();
            BindingContext = this;
            UpdateDisplayText();
        }

        private static void OnSelectedDateChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is MopupsDatePickerEntry control)
            {
                control.UpdateDisplayText();
            }
        }

        private static void OnPlaceholderChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is MopupsDatePickerEntry control)
            {
                control.UpdateDisplayText();
            }
        }

        private void UpdateDisplayText()
        {
            var formatted = SelectedDate?.ToString("yyyy/MM/dd");
            Text = string.IsNullOrEmpty(formatted) ? Placeholder : formatted;
        }

        [RelayCommand]
        private async Task ShowCalendarAsync()
        {
            var popup = new InternalDateSelectionPopup(this, SelectedDate, OnDateSelected);
            await MopupService.Instance.PushAsync(popup);
        }

        private void OnDateSelected(DateTime? date)
        {
            SelectedDate = date;
        }

        private sealed class InternalDateSelectionPopup : PopupPage
        {
            private readonly MopupsDatePickerEntry _owner;
            private readonly Action<DateTime?> _onDateSelected;
            private readonly RadCalendar _calendar;

            public InternalDateSelectionPopup(MopupsDatePickerEntry owner, DateTime? initialDate, Action<DateTime?> onDateSelected)
            {
                _owner = owner;
                _onDateSelected = onDateSelected;

                _calendar = new RadCalendar
                {
                    HeightRequest = 350,
                    WidthRequest = 350,
                    SelectedDate = initialDate ?? DateTime.Today
                };

                Content = BuildPopupContent();
                BackgroundColor = Color.FromArgb("#80000000");
            }

            private View BuildPopupContent()
            {
                var applyButton = new Button
                {
                    Text = "決定",
                    BackgroundColor = Color.FromArgb("#512BD4"),
                    TextColor = Colors.White,
                    CornerRadius = 8,
                    Padding = new Thickness(20, 10)
                };
                applyButton.Clicked += async (sender, args) =>
                {
                    _onDateSelected?.Invoke(_calendar.SelectedDate);
                    await CloseAsync();
                };

                var cancelButton = new Button
                {
                    Text = "キャンセル",
                    BackgroundColor = Color.FromArgb("#CCCCCC"),
                    TextColor = Colors.Black,
                    CornerRadius = 8,
                    Padding = new Thickness(20, 10)
                };
                cancelButton.Clicked += async (sender, args) => await CloseAsync();

                var container = new Border
                {
                    Background = new SolidColorBrush(Colors.White),
                    Content = new VerticalStackLayout
                    {
                        Spacing = 20,
                        Children =
                        {
                            new Label
                            {
                                Text = "日付を選択",
                                FontSize = 18,
                                FontAttributes = FontAttributes.Bold,
                                HorizontalOptions = LayoutOptions.Center
                            },
                            _calendar,
                            new HorizontalStackLayout
                            {
                                Spacing = 12,
                                HorizontalOptions = LayoutOptions.Center,
                                Children = { applyButton, cancelButton }
                            }
                        }
                    }
                };

                ApplyContainerStyle(container);

                return container;
            }

            private void ApplyContainerStyle(Border container)
            {
                if (_owner?.Resources.TryGetValue("PopupContainerStyle", out var localStyle) == true)
                {
                    if (localStyle is Style directStyle)
                    {
                        container.Style = directStyle;
                        return;
                    }

                    container.SetDynamicResource(VisualElement.StyleProperty, "PopupContainerStyle");
                    return;
                }

                if (Application.Current?.Resources.TryGetValue("PopupContainerStyle", out var appStyle) == true)
                {
                    if (appStyle is Style typedStyle)
                    {
                        container.Style = typedStyle;
                        return;
                    }

                    container.SetDynamicResource(VisualElement.StyleProperty, "PopupContainerStyle");
                }
            }

            private static Task CloseAsync() => MopupService.Instance.PopAsync();
        }
    }
}
