using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ToolX.Helpers;

namespace ToolX
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();
            this.Loaded += SettingsPage_Loaded;
        }

        private void SettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            var currentTheme = ThemeManager.LoadTheme();
            switch (currentTheme)
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

        private void ThemeRadioButtons_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var selectedThemeName = (e.AddedItems[0] as RadioButton)?.Content?.ToString();
                ElementTheme newTheme;
                switch (selectedThemeName)
                {
                    case "Light":
                        newTheme = ElementTheme.Light;
                        break;
                    case "Dark":
                        newTheme = ElementTheme.Dark;
                        break;
                    default:
                        newTheme = ElementTheme.Default;
                        break;
                }

                ThemeManager.ApplyTheme(newTheme);
                ThemeManager.SaveTheme(newTheme);
            }
        }
    }
}