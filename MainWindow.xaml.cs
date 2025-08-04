using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace ToolX
{
    public sealed partial class MainWindow : Window
    {
        public static Window? m_window;

        public MainWindow()
        {
            this.InitializeComponent();
            m_window = this;
            this.Title = "ToolX - Image & Video Toolkit";

            // Set Icons using C# code for reliability
            HomeNavItem.Icon = new SymbolIcon(Symbol.Home);
            SplitterNavItem.Icon = new SymbolIcon(Symbol.ViewAll);
            UpscalerNavItem.Icon = new SymbolIcon(Symbol.ZoomIn);
            ExtractFramesNavItem.Icon = new SymbolIcon(Symbol.Video);
            FindBestNavItem.Icon = new SymbolIcon(Symbol.Pictures);

            // Enable the tool buttons
            SplitterNavItem.IsEnabled = true;
            UpscalerNavItem.IsEnabled = true;
            ExtractFramesNavItem.IsEnabled = true;
            FindBestNavItem.IsEnabled = true;

            // Start on the Home page
            NavView.SelectedItem = HomeNavItem;
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            // This logic correctly handles both the settings button and the regular menu items
            if (args.IsSettingsSelected == true)
            {
                // Navigate to settings page if the settings item is selected
                if (ContentFrame.CurrentSourcePageType != typeof(SettingsPage))
                {
                    ContentFrame.Navigate(typeof(SettingsPage));
                }
            }
            else if (args.SelectedItem is NavigationViewItem item && item.Tag is string pageTag)
            {
                // Navigate to other pages
                string pageTypeName = $"ToolX.{pageTag}";
                Type? pageType = Type.GetType(pageTypeName);
                if (pageType != null && pageType != ContentFrame.CurrentSourcePageType)
                {
                    ContentFrame.Navigate(pageType);
                }
            }
        }
    }
}