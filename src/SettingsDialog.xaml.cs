// SettingsDialog.xaml.cs
using Microsoft.UI.Xaml.Controls;
using System;
using System.Globalization;

namespace AirDensity.NET
{
    public sealed partial class SettingsDialog : ContentDialog
    {
        public double GravityValue { get; private set; }
        public double GasConstantValue { get; private set; }

        public SettingsDialog(double currentG, double currentR)
        {
            this.InitializeComponent();
            // Set XamlRoot for WinUI 3 ContentDialog
            this.XamlRoot = App.MainWindow?.Content.XamlRoot;

            // Initialize with current values
            GravityTextBox.Text = currentG.ToString(CultureInfo.InvariantCulture);
            GasConstantTextBox.Text = currentR.ToString(CultureInfo.InvariantCulture);
            GravityValue = currentG; // Store initial values in case of Cancel
            GasConstantValue = currentR;
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // Validate and save the values
            bool isValid = true;
            ValidationTextBlock.Text = string.Empty;
            ValidationTextBlock.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;

            if (!double.TryParse(GravityTextBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double gVal) || gVal <= 0)
            {
                ValidationTextBlock.Text += "Invalid value for Gravity. Must be a positive number.\n";
                isValid = false;
            }

            if (!double.TryParse(GasConstantTextBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double rVal) || rVal <= 0)
            {
                ValidationTextBlock.Text += "Invalid value for Gas Constant. Must be a positive number.\n";
                isValid = false;
            }

            if (!isValid)
            {
                ValidationTextBlock.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                args.Cancel = true; // Prevent dialog from closing
            }
            else
            {
                GravityValue = gVal;
                GasConstantValue = rVal;
                // Let the dialog close
            }
        }
    }
}