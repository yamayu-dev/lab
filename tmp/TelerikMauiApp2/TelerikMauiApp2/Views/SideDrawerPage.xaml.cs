using TelerikMauiApp2.ViewModels;

namespace TelerikMauiApp2.Views
{
    public partial class SideDrawerPage : ContentPage
    {
        public SideDrawerPage()
        {
            InitializeComponent();
            
            // Subscribe to drawer state changes
            Drawer.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(Drawer.IsOpen))
                {
                    // Hide navigation bar when drawer is open
                    Shell.SetNavBarIsVisible(this, !Drawer.IsOpen);
                }
            };
        }
    }
}
