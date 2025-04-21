// App.xaml.cs
using Microsoft.UI.Xaml;
using WinUIEx; // If using WindowEx

namespace AirDensity.NET
{
    public partial class App : Application
    {
        // Expose the main window instance - Make it nullable
        public static WindowEx? MainWindow { get; private set; }

        public App()
        {
            this.InitializeComponent();
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            MainWindow = new MainWindow(); // Create instance
            MainWindow.Activate();
        }
    }
}