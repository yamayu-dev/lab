using System.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Telerik.Maui.Controls;

namespace TelerikMauiApp2.Controls
{
    public partial class DatePickerEntry : Label
    {
        // プラットフォーム固有処理用の部分メソッド
        partial void OnPopupOpenedPlatform();
        partial void OnPopupClosedPlatform();

        public static readonly BindableProperty SelectedDateProperty =
            BindableProperty.Create(nameof(SelectedDate), typeof(DateTime?), typeof(DatePickerEntry), DateTime.Today,
                BindingMode.TwoWay, propertyChanged: OnSelectedDateChanged);

        public DateTime? SelectedDate
        {
            get => (DateTime?)GetValue(SelectedDateProperty);
            set => SetValue(SelectedDateProperty, value);
        }

        public DatePickerEntry()
        {
            InitializeComponent();
            this.BindingContext = this;
            UpdateDisplayDate();
            
            // CalendarのSelectedDateをバインド
            calendar.SetBinding(RadCalendar.SelectedDateProperty,
                new Binding(nameof(SelectedDate), BindingMode.TwoWay, source: this));

            calendarPopup.PropertyChanged += CalendarPopup_PropertyChanged;
        }

        protected override void OnHandlerChanged()
        {
            base.OnHandlerChanged();

            if (Handler is null)
            {
                calendarPopup.PropertyChanged -= CalendarPopup_PropertyChanged;
            }
        }

        private static void OnSelectedDateChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is DatePickerEntry control)
            {
                control.UpdateDisplayDate();
            }
        }

        private void UpdateDisplayDate()
        {
            Text = SelectedDate?.ToString("yyyy/MM/dd") ?? string.Empty;
        }

        [RelayCommand]
        private void ShowCalendar()
        {
            // RadPopupを表示
            calendarPopup.IsOpen = true;
        }

        private void OnCalendarSelectionChanged(object sender, EventArgs e)
        {
            // カレンダーの日付選択が変更されたときに表示を更新
            UpdateDisplayDate();
        }

        [RelayCommand]
        private void Close()
        {
            // RadPopupを閉じる
            calendarPopup.IsOpen = false;
        }

    private void CalendarPopup_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(RadPopup.IsOpen) || string.IsNullOrEmpty(e.PropertyName))
            {
                if (calendarPopup.IsOpen)
                {
                    OnPopupOpenedPlatform();
                }
                else
                {
                    OnPopupClosedPlatform();
                }
            }
        }
    }
}
