#if IOS
#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using UIKit;

namespace TelerikMauiApp2.Controls
{
    public partial class DatePickerEntry
    {
        private nfloat? _previousWindowLevel;
        private readonly List<(WeakReference<UIWindow> Window, nfloat Level)> _demotedWindows = new();

        partial void OnPopupOpenedPlatform()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var handler = (calendarPopup as IElement)?.Handler;
                if (handler?.PlatformView is not UIView popupView)
                {
                    return;
                }

                var popupWindow = popupView.Window ?? GetKeyWindow();
                if (popupWindow is null)
                {
                    return;
                }

                var originalLevel = popupWindow.WindowLevel;
                _previousWindowLevel = originalLevel;

                var targetLevel = (nfloat)Math.Max((double)UIWindowLevel.StatusBar + 1000.0, (double)originalLevel + 1000.0);
                popupWindow.WindowLevel = targetLevel;
                popupWindow.MakeKeyAndVisible();
                popupWindow.Layer.ZPosition = float.MaxValue;

                BringHierarchyToFront(popupView);

                _demotedWindows.Clear();
                foreach (var otherWindow in GetAllWindows().ToList())
                {
                    if (ReferenceEquals(otherWindow, popupWindow))
                    {
                        continue;
                    }

                    var isMopupsWindow = otherWindow.GetType().FullName?.Contains("Mopups", StringComparison.OrdinalIgnoreCase) == true
                        || otherWindow.RootViewController?.GetType().FullName?.Contains("Mopups", StringComparison.OrdinalIgnoreCase) == true;

                    if (otherWindow.WindowLevel >= targetLevel || isMopupsWindow)
                    {
                        _demotedWindows.Add((new WeakReference<UIWindow>(otherWindow), otherWindow.WindowLevel));
                        otherWindow.WindowLevel = isMopupsWindow ? UIWindowLevel.Normal - 2f : (nfloat)(targetLevel - 1f);
                    }
                }
            });
        }

        partial void OnPopupClosedPlatform()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                RestoreDemotedWindows();

                var handler = (calendarPopup as IElement)?.Handler;
                if (handler?.PlatformView is UIView popupView)
                {
                    popupView.Layer.ZPosition = 0;
                    RestoreHierarchyZPosition(popupView);

                    var popupWindow = popupView.Window ?? GetKeyWindow();
                    if (popupWindow is not null && _previousWindowLevel is nfloat prevLevel)
                    {
                        popupWindow.WindowLevel = prevLevel;
                        popupWindow.Layer.ZPosition = 0;
                    }
                }

                _previousWindowLevel = null;
            });
        }

        private void RestoreDemotedWindows()
        {
            if (_demotedWindows.Count == 0)
            {
                return;
            }

            foreach (var (windowRef, level) in _demotedWindows.ToList())
            {
                if (windowRef.TryGetTarget(out var window))
                {
                    window.WindowLevel = level;
                }
            }

            _demotedWindows.Clear();
        }

        private void BringHierarchyToFront(UIView popupView)
        {
            var current = popupView;
            while (current.Superview is not null)
            {
                current.Superview.BringSubviewToFront(current);
                current.Layer.ZPosition = float.MaxValue;
                current = current.Superview;
            }

            current.Layer.ZPosition = float.MaxValue;
        }

        private void RestoreHierarchyZPosition(UIView popupView)
        {
            var current = popupView;
            while (current.Superview is not null)
            {
                current.Layer.ZPosition = 0;
                current = current.Superview;
            }

            current.Layer.ZPosition = 0;
        }

        private static UIWindow? GetKeyWindow()
        {
            if (UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
            {
                var sceneWindow = UIApplication.SharedApplication
                    .ConnectedScenes
                    .OfType<UIWindowScene>()
                    .SelectMany(scene => scene.Windows)
                    .FirstOrDefault(w => w.IsKeyWindow);

                if (sceneWindow is not null)
                {
                    return sceneWindow;
                }
            }

#pragma warning disable CA1416
#pragma warning disable CA1422
            return UIApplication.SharedApplication.KeyWindow;
#pragma warning restore CA1422
#pragma warning restore CA1416
        }

        private static IEnumerable<UIWindow> GetAllWindows()
        {
            if (UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
            {
                return UIApplication.SharedApplication
                    .ConnectedScenes
                    .OfType<UIWindowScene>()
                    .SelectMany(scene => scene.Windows)
                    .Where(window => window is not null);
            }

#pragma warning disable CA1416
#pragma warning disable CA1422
            return UIApplication.SharedApplication.Windows;
#pragma warning restore CA1422
#pragma warning restore CA1416
        }
    }
}
#endif
