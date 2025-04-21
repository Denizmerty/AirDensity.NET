// AboutDialog.xaml.cs
using Microsoft.UI.Xaml.Controls;

namespace AirDensity.NET
{
    public sealed partial class AboutDialog : ContentDialog
    {
        public AboutDialog()
        {
            this.InitializeComponent();
            // Set XamlRoot for WinUI 3 ContentDialog
            this.XamlRoot = App.MainWindow?.Content.XamlRoot;
        }
    }
}