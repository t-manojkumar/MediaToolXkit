using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ToolX
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();
            this.Loaded += SettingsPage_Loaded;
        }

        // This method runs when the page is loaded, ensuring the correct radio button is selected.
        private void SettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.XamlRoot.Content is FrameworkElement rootElement)
            {
                switch (rootElement.RequestedTheme)
                {
                    case ElementTheme.Light:
                        ThemeRadioButtons.SelectedIndex = 0;
                        break;
                    case ElementTheme.Dark:
                        ThemeRadioButtons.SelectedIndex = 1;
                        break;
                    case ElementTheme.Default:
                        ThemeRadioButtons.SelectedIndex = 2;
                        break;
                }
            }
        }

        // This method runs when the user selects a new theme.
        private void ThemeRadioButtons_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var selectedTheme = (e.AddedItems[0] as RadioButton)?.Content?.ToString();
                if (this.XamlRoot.Content is FrameworkElement rootElement)
                {
                    switch (selectedTheme)
                    {
                        case "Light":
                            rootElement.RequestedTheme = ElementTheme.Light;
                            break;
                        case "Dark":
                            rootElement.RequestedTheme = ElementTheme.Dark;
                            break;
                        case "Use system setting":
                            rootElement.RequestedTheme = ElementTheme.Default;
                            break;
                    }
                }
            }
        }
    }
}