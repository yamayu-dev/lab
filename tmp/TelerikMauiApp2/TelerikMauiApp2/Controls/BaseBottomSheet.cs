using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Telerik.Maui.Controls;
using Telerik.Maui.Controls.BottomSheet;

namespace TelerikMauiApp2.Controls
{
    /// <summary>
    /// RadBottomSheetの基底クラス。
    /// 共通機能（コマンド、状態管理、デバッグ機能）を提供。
    /// </summary>
    public abstract class BaseBottomSheet : RadBottomSheet
    {
        private CancellationTokenSource? _autoCloseCts;
        private bool _isGuardingState;

        protected BaseBottomSheet()
        {
            AutoGenerateStates = false;
            AnimationDuration = 300;
            Loaded += OnLoaded;

            // コマンドを先に用意
            OpenCommand = new RelayCommand(() => Open(OpenStateName));
            CloseCommand = new RelayCommand(() => Close());
            ToggleCommand = new RelayCommand(() =>
            {
                var s = State;
                if (string.Equals(s, MinimumState, StringComparison.Ordinal))
                    Open(OpenStateName);
                else
                    Close();
            });
        }

        private void OnLoaded(object? sender, EventArgs e)
        {
            Loaded -= OnLoaded;
            try
            {
                State = InitialState;
                UpdateIsMinimumState();
                
                if (ShowDebug) DebugText = $"Loaded: State={State}, Min={MinimumState}, AtMin={IsMinimumState}";
            }
            catch (Exception ex)
            {
                if (ShowDebug) DebugText = $"Loaded: set State NG - {ex.Message}";
            }
        }

        protected override void OnPropertyChanged(string? propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
            
            if (propertyName == nameof(State))
            {
                if (_isGuardingState) return;

                UpdateIsMinimumState();

                if (ShowDebug)
                {
                    DebugText = $"State={State}, Min={MinimumState}, AtMin={IsMinimumState}";
                }

                if (AutoCloseOnCollapse && IsMinimumState)
                {
                    ScheduleAutoClose();
                }
                else
                {
                    CancelAutoClose();
                }
            }
        }

        private void UpdateIsMinimumState()
        {
            var currentState = State ?? string.Empty;
            var isMin = string.Equals(currentState, MinimumState, StringComparison.Ordinal);
            IsMinimumState = isMin;
        }

        private void CancelAutoClose()
        {
            _autoCloseCts?.Cancel();
            _autoCloseCts = null;
        }

        private void ScheduleAutoClose()
        {
            CancelAutoClose();
            _autoCloseCts = new CancellationTokenSource();
            var token = _autoCloseCts.Token;

            _ = MainThread.InvokeOnMainThreadAsync(async () =>
            {
                try
                {
                    if (AutoCloseDelay > TimeSpan.Zero)
                    {
                        await Task.Delay(AutoCloseDelay, token);
                    }

                    if (!token.IsCancellationRequested)
                    {
                        Close();
                    }
                }
                catch (TaskCanceledException)
                {
                    // ignore
                }
            });
        }

        // ==== Public surface ====

        public static readonly BindableProperty MainContentProperty = BindableProperty.Create(
            nameof(MainContent), typeof(View), typeof(BaseBottomSheet), default(View),
            propertyChanged: (b, _, n) => ((BaseBottomSheet)b).Content = (View?)n);

        public View? MainContent
        {
            get => (View?)GetValue(MainContentProperty);
            set => SetValue(MainContentProperty, value);
        }

        public static readonly BindableProperty SheetContentProperty = BindableProperty.Create(
            nameof(SheetContent), typeof(View), typeof(BaseBottomSheet), default(View),
            propertyChanged: (b, _, n) => ((BaseBottomSheet)b).BottomSheetContent = (View?)n);

        public View? SheetContent
        {
            get => (View?)GetValue(SheetContentProperty);
            set => SetValue(SheetContentProperty, value);
        }

        public static readonly BindableProperty InitialStateProperty = BindableProperty.Create(
            nameof(InitialState), typeof(string), typeof(BaseBottomSheet), "Peek");

        public string InitialState
        {
            get => (string)GetValue(InitialStateProperty);
            set => SetValue(InitialStateProperty, value);
        }

        public static readonly BindableProperty MinimumStateProperty = BindableProperty.Create(
            nameof(MinimumState), typeof(string), typeof(BaseBottomSheet), "Peek");

        public string MinimumState
        {
            get => (string)GetValue(MinimumStateProperty);
            set => SetValue(MinimumStateProperty, value);
        }

        public static readonly BindableProperty CollapseThresholdProperty = BindableProperty.Create(
            nameof(CollapseThreshold), typeof(double), typeof(BaseBottomSheet), 0.5d);

        public double CollapseThreshold
        {
            get => (double)GetValue(CollapseThresholdProperty);
            set => SetValue(CollapseThresholdProperty, value);
        }

        public static readonly BindableProperty IsMinimumStateProperty = BindableProperty.Create(
            nameof(IsMinimumState), typeof(bool), typeof(BaseBottomSheet), false, BindingMode.TwoWay);

        public bool IsMinimumState
        {
            get => (bool)GetValue(IsMinimumStateProperty);
            private set => SetValue(IsMinimumStateProperty, value);
        }

        public static readonly BindableProperty ShowDebugProperty = BindableProperty.Create(
            nameof(ShowDebug), typeof(bool), typeof(BaseBottomSheet), false);

        public bool ShowDebug
        {
            get => (bool)GetValue(ShowDebugProperty);
            set => SetValue(ShowDebugProperty, value);
        }

        public static readonly BindableProperty DebugTextProperty = BindableProperty.Create(
            nameof(DebugText), typeof(string), typeof(BaseBottomSheet), string.Empty);

        public string DebugText
        {
            get => (string)GetValue(DebugTextProperty);
            set => SetValue(DebugTextProperty, value);
        }

        public static readonly BindableProperty OpenCommandProperty = BindableProperty.Create(
            nameof(OpenCommand), typeof(IRelayCommand), typeof(BaseBottomSheet));

        public IRelayCommand OpenCommand
        {
            get => (IRelayCommand)GetValue(OpenCommandProperty);
            private set => SetValue(OpenCommandProperty, value);
        }

        public static readonly BindableProperty CloseCommandProperty = BindableProperty.Create(
            nameof(CloseCommand), typeof(IRelayCommand), typeof(BaseBottomSheet));

        public IRelayCommand CloseCommand
        {
            get => (IRelayCommand)GetValue(CloseCommandProperty);
            private set => SetValue(CloseCommandProperty, value);
        }

        public static readonly BindableProperty ToggleCommandProperty = BindableProperty.Create(
            nameof(ToggleCommand), typeof(IRelayCommand), typeof(BaseBottomSheet));

        public IRelayCommand ToggleCommand
        {
            get => (IRelayCommand)GetValue(ToggleCommandProperty);
            private set => SetValue(ToggleCommandProperty, value);
        }

        public static readonly BindableProperty OpenStateNameProperty = BindableProperty.Create(
            nameof(OpenStateName), typeof(string), typeof(BaseBottomSheet), "Full");

        public string OpenStateName
        {
            get => (string)GetValue(OpenStateNameProperty);
            set => SetValue(OpenStateNameProperty, value);
        }

        public static readonly BindableProperty AutoCloseOnCollapseProperty = BindableProperty.Create(
            nameof(AutoCloseOnCollapse), typeof(bool), typeof(BaseBottomSheet), false);

        public bool AutoCloseOnCollapse
        {
            get => (bool)GetValue(AutoCloseOnCollapseProperty);
            set => SetValue(AutoCloseOnCollapseProperty, value);
        }

        public static readonly BindableProperty AutoCloseDelayProperty = BindableProperty.Create(
            nameof(AutoCloseDelay), typeof(TimeSpan), typeof(BaseBottomSheet), TimeSpan.Zero);

        public TimeSpan AutoCloseDelay
        {
            get => (TimeSpan)GetValue(AutoCloseDelayProperty);
            set => SetValue(AutoCloseDelayProperty, value);
        }

        // ==== public methods ====
        public void Open(string? stateName = null) => TryGoToState(stateName ?? OpenStateName, "OpenCommand");
        public void Close() => TryGoToState(MinimumState, "CloseCommand");

        protected void TryGoToState(string target, string reason)
        {
            try
            {
                GoToBottomSheetState(target);
                if (ShowDebug) DebugText = $"{reason}: GoTo({target}) OK";
            }
            catch (Exception ex)
            {
                if (ShowDebug) DebugText = $"{reason}: GoTo({target}) NG - {ex.Message}";
            }
        }
    }
}
