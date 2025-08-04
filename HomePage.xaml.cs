using Microsoft.UI.Xaml.Controls;

// Ensure the namespace is "ToolX"
namespace ToolX
{
    // Ensure the class name is "public sealed partial class HomePage : Page"
    public sealed partial class HomePage : Page
    {
        public HomePage()
        {
            this.InitializeComponent(); // This line will now work correctly
        }
    }
}