using Microsoft.UI.Xaml;
using Windows.Storage;

namespace ToolX.Helpers
{
    public static class ThemeManager
    {
        private const string ThemeSettingName = "AppTheme";

        public static void ApplyTheme(ElementTheme theme)
        {
            if (MainWindow.m_window.Content is FrameworkElement rootElement)
            {
                rootElement.RequestedTheme = theme;
            }
        }

        public static void SaveTheme(ElementTheme theme)
        {
            ApplicationData.Current.LocalSettings.Values[ThemeSettingName] = (int)theme;
        }

        public static ElementTheme LoadTheme()
        {
            if (ApplicationData.Current.LocalSettings.Values.TryGetValue(ThemeSettingName, out var themeValue) && themeValue is int themeInt)
            {
                return (ElementTheme)themeInt;
            }
            return ElementTheme.Default; // Default theme
        }
    }
}
