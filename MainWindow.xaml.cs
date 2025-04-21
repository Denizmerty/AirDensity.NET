// MainWindow.xaml.cs
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop; // Needed for FileSavePicker interop
using WinUIEx;

namespace AirDensity.NET
{
    public record CalculationResult(
        double TemperatureC,
        double PressureHpa,
        double DensityKgM3,
        double PercentPressure,
        double PercentDensity,
        double SpecificHumidityGKg,
        double VaporPressureHpa);

    public sealed partial class MainWindow : WindowEx
    {
        private Settings _appSettings = new();
        private const string SettingsFileName = "airsensity_settings.json";
        private const string LogFileName = "airsensity_app.log";
        private StorageFolder _localFolder = ApplicationData.Current.LocalFolder;

        // Calculation Cache (Key: altM_tempC_pressPa_g_R, Value: formatted result string)
        private Dictionary<string, string> _calculationCache = new();

        private bool _isInitialized = false;

        public MainWindow()
        {
            this.InitializeComponent();
            this.Title = "Extended ISA Model Calculator";

            // Start initialization asynchronously
            _ = InitializeAppAsync();

            ResultBorder.Visibility = Visibility.Collapsed;
            this.Closed += MainWindow_Closed;
        }

        private async Task InitializeAppAsync()
        {
            PopulateComboBoxes();
            await LoadSettingsDataAsync();
            ApplySettingsToUI();
            _isInitialized = true;
            LogEvent("Application initialized.");
        }


        private async void MainWindow_Closed(object sender, WindowEventArgs args)
        {
            if (!_isInitialized) return;
            await SaveSettingsAsync();
            LogEvent("Application closed.");
        }

        private async Task LoadSettingsDataAsync()
        {
            try
            {
                var file = await _localFolder.TryGetItemAsync(SettingsFileName) as StorageFile;
                if (file != null)
                {
                    string json = await FileIO.ReadTextAsync(file);
                    // Use case-insensitive deserialization for robustness
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    _appSettings = JsonSerializer.Deserialize<Settings>(json, options) ?? new Settings();
                    LogEvent("Settings data loaded from " + SettingsFileName);
                }
                else
                {
                    _appSettings = new Settings(); // Use defaults
                    LogEvent(SettingsFileName + " not found. Using default settings data.");
                }
            }
            catch (Exception ex)
            {
                LogEvent($"Error loading settings data: {ex.Message}");
                _appSettings = new Settings(); // Fallback to defaults
                if (_isInitialized) // Only show status if UI is likely ready
                {
                    ShowStatus($"Error loading settings: {ex.Message}", InfoBarSeverity.Warning);
                }
            }
        }

        private async Task SaveSettingsAsync()
        {
            if (!_isInitialized) return;

            try
            {
                // Update settings object from UI before saving
                if (AltitudeUnitComboBox.SelectedIndex >= 0)
                    _appSettings.AltitudeUnitIndex = AltitudeUnitComboBox.SelectedIndex;
                if (TemperatureUnitComboBox.SelectedIndex >= 0)
                    _appSettings.TemperatureUnitIndex = TemperatureUnitComboBox.SelectedIndex;
                if (PressureUnitComboBox.SelectedIndex >= 0)
                    _appSettings.PressureUnitIndex = PressureUnitComboBox.SelectedIndex;
                if (ModelComboBox.SelectedIndex >= 0)
                    _appSettings.ModelIndex = ModelComboBox.SelectedIndex;
                // g and R are updated via the Settings Dialog result

                string json = JsonSerializer.Serialize(_appSettings, new JsonSerializerOptions { WriteIndented = true });
                StorageFile file = await _localFolder.CreateFileAsync(SettingsFileName, CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(file, json);
                LogEvent("Settings saved to " + SettingsFileName);
            }
            catch (Exception ex)
            {
                LogEvent($"Error saving settings: {ex.Message}");
            }
        }

        private void ApplySettingsToUI()
        {
            if (AltitudeUnitComboBox.Items.Count > 0)
            {
                int index = _appSettings.AltitudeUnitIndex;
                AltitudeUnitComboBox.SelectedIndex = (index >= 0 && index < AltitudeUnitComboBox.Items.Count) ? index : 1; // Default Meters
            }
            else { AltitudeUnitComboBox.SelectedIndex = -1; }

            if (TemperatureUnitComboBox.Items.Count > 0)
            {
                int index = _appSettings.TemperatureUnitIndex;
                TemperatureUnitComboBox.SelectedIndex = (index >= 0 && index < TemperatureUnitComboBox.Items.Count) ? index : 0; // Default °C
            }
            else { TemperatureUnitComboBox.SelectedIndex = -1; }

            if (PressureUnitComboBox.Items.Count > 0)
            {
                int index = _appSettings.PressureUnitIndex;
                PressureUnitComboBox.SelectedIndex = (index >= 0 && index < PressureUnitComboBox.Items.Count) ? index : 0; // Default hPa
            }
            else { PressureUnitComboBox.SelectedIndex = -1; }

            if (ModelComboBox.Items.Count > 0)
            {
                int index = _appSettings.ModelIndex;
                ModelComboBox.SelectedIndex = (index >= 0 && index < ModelComboBox.Items.Count) ? index : 1; // Default Extended ISA
            }
            else { ModelComboBox.SelectedIndex = -1; }
        }

        private void PopulateComboBoxes()
        {
            AltitudeUnitComboBox.Items.Clear();
            TemperatureUnitComboBox.Items.Clear();
            PressureUnitComboBox.Items.Clear();
            ModelComboBox.Items.Clear();

            AltitudeUnitComboBox.Items.Add("Feet");
            AltitudeUnitComboBox.Items.Add("Meters");

            TemperatureUnitComboBox.Items.Add("°C");
            TemperatureUnitComboBox.Items.Add("°F");

            PressureUnitComboBox.Items.Add("hPa");
            PressureUnitComboBox.Items.Add("inHg");

            ModelComboBox.Items.Add("ISA"); // Placeholder
            ModelComboBox.Items.Add("Extended ISA");
        }

        private async void LogEvent(string message)
        {
            try
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string logEntry = $"{timestamp} - {message}{Environment.NewLine}";
                string logFilePath = Path.Combine(_localFolder.Path, LogFileName);
                using (StreamWriter writer = new StreamWriter(logFilePath, true, Encoding.UTF8))
                {
                    await writer.WriteAsync(logEntry);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Logging failed: {ex.Message}");
            }
        }

        private void ShowStatus(string message, InfoBarSeverity severity, string? title = null)
        {
            StatusInfoBar.Title = title ?? severity.ToString();
            StatusInfoBar.Message = message;
            StatusInfoBar.Severity = severity;
            StatusInfoBar.IsOpen = true;
        }

        private void ClearStatus()
        {
            StatusInfoBar.IsOpen = false;
        }


        private bool ParseDouble(string text, out double value)
        {
            // Use InvariantCulture for consistent decimal point parsing regardless of system locale
            return double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
        }

        private (bool IsValid, double ValueM, double ValueC, double ValueHpa) GetAndValidateInputs()
        {
            if (!_isInitialized)
            {
                ShowStatus("Application is initializing, please wait.", InfoBarSeverity.Informational);
                return (false, 0, 0, 0);
            }

            ClearStatus();

            if (string.IsNullOrWhiteSpace(AltitudeTextBox.Text) ||
                string.IsNullOrWhiteSpace(TemperatureTextBox.Text) ||
                string.IsNullOrWhiteSpace(PressureTextBox.Text))
            {
                ShowStatus("Please enter values for Altitude, Temperature, and Pressure.", InfoBarSeverity.Warning, "Input Required");
                return (false, 0, 0, 0);
            }

            if (AltitudeUnitComboBox.SelectedIndex < 0 ||
                TemperatureUnitComboBox.SelectedIndex < 0 ||
                PressureUnitComboBox.SelectedIndex < 0 ||
                ModelComboBox.SelectedIndex < 0)
            {
                ShowStatus("Please ensure unit and model selections are made.", InfoBarSeverity.Warning, "Input Required");
                return (false, 0, 0, 0);
            }

            if (!ParseDouble(AltitudeTextBox.Text, out double altitudeInput) ||
                !ParseDouble(TemperatureTextBox.Text, out double tempInput) ||
                !ParseDouble(PressureTextBox.Text, out double pressureInput))
            {
                ShowStatus("Invalid numeric input detected. Please check values.", InfoBarSeverity.Error, "Input Error");
                return (false, 0, 0, 0);
            }

            // --- Unit Conversion to SI for calculation ---
            bool isFeet = AltitudeUnitComboBox.SelectedIndex == 0;
            bool isFahrenheit = TemperatureUnitComboBox.SelectedIndex == 1;
            bool isInHg = PressureUnitComboBox.SelectedIndex == 1;

            double altitudeM = isFeet ? altitudeInput * 0.3048 : altitudeInput;
            double tempC = isFahrenheit ? (tempInput - 32.0) * 5.0 / 9.0 : tempInput;
            double pressureHpa = isInHg ? pressureInput * 33.8639 : pressureInput;

            // --- Validation using SI units ---
            if (altitudeM < -5000.0 || altitudeM > 85000.0) // ISA Model altitude limits
            {
                ShowStatus("Altitude is outside the model range (approx -5km to 85km).", InfoBarSeverity.Error, "Validation Error");
                AltitudeTextBox.Focus(FocusState.Programmatic);
                return (false, 0, 0, 0);
            }

            // Show warnings for potentially unusual, but not strictly invalid, inputs
            bool warningShown = false;
            if (tempC < -100.0 || tempC > 100.0) // Plausible temperature range check
            {
                if (!StatusInfoBar.IsOpen || StatusInfoBar.Severity < InfoBarSeverity.Warning)
                {
                    ShowStatus("Sea-level temperature is outside the typical range (-100°C to 100°C).", InfoBarSeverity.Warning, "Validation Warning");
                    warningShown = true;
                }
            }
            if (pressureHpa < 800.0 || pressureHpa > 1100.0) // Plausible pressure range check
            {
                if (!warningShown && (!StatusInfoBar.IsOpen || StatusInfoBar.Severity < InfoBarSeverity.Warning))
                {
                    ShowStatus("Sea-level pressure is outside the typical range (800 hPa to 1100 hPa).", InfoBarSeverity.Warning, "Validation Warning");
                }
            }

            return (true, altitudeM, tempC, pressureHpa);
        }


        // --- Core Calculation Logic (Based on Extended ISA Model) ---
        private CalculationResult ExtendedIsaModelAdvanced(double altitudeM, double pSeaLevelPa, double tSeaLevelC)
        {
            double g = _appSettings.Gravity;
            double R = _appSettings.GasConstantR;
            double tSeaLevelK = tSeaLevelC + 273.15;

            // --- ISA Layer Definitions ---
            // Using geopotential heights (standard model practice). Geometric altitude (input) is used against these boundaries.
            double[] hLayerGp = { 0.0, 11000.0, 20000.0, 32000.0, 47000.0, 51000.0, 71000.0, 84852.0 }; // meters
            // Base Temperatures (K) - Index 0 is overridden by sea level input
            double[] tBaseK = { tSeaLevelK, 216.65, 216.65, 228.65, 270.65, 270.65, 214.65 };
            // Base Pressures (Pa) - Index 0 is overridden by sea level input
            double[] pBasePa = { pSeaLevelPa, 22632.1, 5474.89, 868.019, 110.906, 66.9389, 3.95642 };
            // Lapse Rates (K/m)
            double[] lapseRate = { -0.0065, 0.0, 0.001, 0.0028, 0.0, -0.0028, -0.002 };

            // --- Find Correct Atmospheric Layer ---
            int layerIdx = -1;
            for (int i = 0; i < hLayerGp.Length - 1; ++i)
            {
                if (altitudeM <= hLayerGp[i + 1])
                {
                    layerIdx = i;
                    break;
                }
            }

            if (layerIdx == -1 || altitudeM > hLayerGp[^1]) // Check model limits
            {
                throw new ArgumentOutOfRangeException(nameof(altitudeM), "Altitude exceeds the limits of this extended ISA model (max ~85km)");
            }

            // --- Get Layer Base Parameters ---
            double t0K = tBaseK[layerIdx];
            double p0Pa = pBasePa[layerIdx];
            double h0Gp = hLayerGp[layerIdx];
            double L = lapseRate[layerIdx];

            // Override with actual sea level inputs for the first layer (troposphere base)
            if (layerIdx == 0)
            {
                t0K = tSeaLevelK;
                p0Pa = pSeaLevelPa;
            }

            // --- Calculate Temperature at Altitude ---
            // T = T_base + L * (h - h_base)
            double tK = t0K + L * (altitudeM - h0Gp);
            tK = Math.Max(0.0, tK); // Temperature cannot physically be below 0 K

            // --- Calculate Pressure at Altitude ---
            double pPa;
            const double epsilon = 1e-9; // Tolerance for floating point comparisons

            if (Math.Abs(L) < epsilon) // Isothermal layer (L = 0)
            {
                // P = P_base * exp[-g * (h - h_base) / (R * T_base)]
                if (Math.Abs(t0K) < epsilon)
                    throw new InvalidOperationException("Base temperature zero in isothermal layer calculation.");
                pPa = p0Pa * Math.Exp(-g * (altitudeM - h0Gp) / (R * t0K));
            }
            else // Layer with non-zero lapse rate
            {
                // P = P_base * (T / T_base) ^ (-g / (L * R))
                if (Math.Abs(t0K) < epsilon)
                    throw new InvalidOperationException("Base temperature zero in non-isothermal layer calculation.");

                double tempRatio = tK / t0K;
                // Handle non-positive temperature ratio before exponentiation
                if (tempRatio <= 0)
                {
                    pPa = 0.0; // Pressure approaches zero as temperature approaches absolute zero
                }
                else
                {
                    double exponent = -g / (L * R);
                    pPa = p0Pa * Math.Pow(tempRatio, exponent);
                }
            }
            pPa = Math.Max(0.0, pPa); // Pressure cannot be negative

            // --- Calculate Derived Properties ---
            // Density (rho = P / (R * T))
            double rhoKgM3 = (tK > epsilon) ? pPa / (R * tK) : 0.0;
            rhoKgM3 = Math.Max(0.0, rhoKgM3);

            // Temperature in Celsius
            double tC = tK - 273.15;
            // Pressure in Hectopascals
            double pHpa = pPa / 100.0;

            // --- Percentage Calculations vs Sea Level ---
            double rhoSeaLevel = (tSeaLevelK > epsilon) ? pSeaLevelPa / (R * tSeaLevelK) : 0.0;
            double percentPressure = (pSeaLevelPa > epsilon) ? (pPa / pSeaLevelPa) * 100.0 : 0.0;
            double percentDensity = (rhoSeaLevel > epsilon) ? (rhoKgM3 / rhoSeaLevel) * 100.0 : 0.0;

            // --- Humidity Calculation (Simplified - Assumes 50% Relative Humidity) ---
            const double RH = 50.0; // Assumed Relative Humidity (%)
            // Saturation vapor pressure (e_s) using Magnus formula (output in hPa)
            double tempDenominator = tC + 243.04;
            double e_s = (Math.Abs(tempDenominator) > epsilon)
                       ? 6.1094 * Math.Exp((17.625 * tC) / tempDenominator)
                       : 0.0;
            e_s = Math.Max(0.0, e_s);

            // Actual vapor pressure (e = RH * e_s / 100)
            double eVaporHpa = RH / 100.0 * e_s;

            // Vapor pressure cannot exceed total air pressure
            if (eVaporHpa >= pHpa) eVaporHpa = pHpa * 0.99; // Use a value slightly less than total P
            eVaporHpa = Math.Max(0.0, eVaporHpa);

            // Specific Humidity (q ≈ w = 0.622 * e / (P - e))
            // Where e and P must be in the same units (using hPa here)
            double w_kg_kg = 0.0; // Mixing ratio (kg water vapor / kg dry air)
            double pressure_diff_hpa = pHpa - eVaporHpa;
            if (pressure_diff_hpa > epsilon)
            {
                // Using 0.622 which is ratio of molar mass of water to dry air (Rd/Rv)
                w_kg_kg = 0.622 * eVaporHpa / pressure_diff_hpa;
            }
            w_kg_kg = Math.Max(0.0, w_kg_kg);

            double specificHumidityGKg = w_kg_kg * 1000.0; // Convert kg/kg to g/kg

            return new CalculationResult(tC, pHpa, rhoKgM3, percentPressure, percentDensity, specificHumidityGKg, eVaporHpa);
        }


        private void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized) return;

            var (isValid, altitudeM, tempC, pressureHpa) = GetAndValidateInputs();
            if (!isValid) return;

            double pressurePa = pressureHpa * 100.0;

            string key = string.Format(CultureInfo.InvariantCulture,
                "{0:F5}_{1:F5}_{2:F5}_{3:F5}_{4:F5}",
                altitudeM, tempC, pressurePa, _appSettings.Gravity, _appSettings.GasConstantR);

            if (_calculationCache.TryGetValue(key, out string? cachedResult))
            {
                ResultTextBlock.Text = cachedResult;
                ResultBorder.Visibility = Visibility.Visible;
                LogEvent("Used cached results for key: " + key);
                ShowStatus("Calculation successful (from cache).", InfoBarSeverity.Success);
                return;
            }

            try
            {
                CalculationResult result = ExtendedIsaModelAdvanced(altitudeM, pressurePa, tempC);
                string formattedResult = FormatResults(result, altitudeM, tempC, pressureHpa);
                ResultTextBlock.Text = formattedResult;
                ResultBorder.Visibility = Visibility.Visible;
                _calculationCache[key] = formattedResult;

                LogEvent("Calculation successful for key: " + key);
                if (!StatusInfoBar.IsOpen || StatusInfoBar.Severity < InfoBarSeverity.Warning)
                {
                    ShowStatus("Calculation successful.", InfoBarSeverity.Success);
                }
            }
            catch (ArgumentOutOfRangeException ex)
            {
                ShowStatus($"Input Error: {ex.ParamName} - {ex.Message}", InfoBarSeverity.Error, "Calculation Error");
                LogEvent($"Calculation Error: {ex.Message}");
                ResultBorder.Visibility = Visibility.Collapsed;
            }
            catch (InvalidOperationException ex)
            {
                ShowStatus($"Calculation Error: {ex.Message}. Check inputs.", InfoBarSeverity.Error, "Calculation Error");
                LogEvent($"Calculation Error: {ex.Message}");
                ResultBorder.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                ShowStatus($"An unexpected calculation error occurred: {ex.Message}", InfoBarSeverity.Error, "Calculation Error");
                LogEvent($"Unexpected Calculation Error: {ex.GetType().Name} - {ex.Message}");
                ResultBorder.Visibility = Visibility.Collapsed;
            }
        }

        private string FormatResults(CalculationResult result, double altitudeM, double tempSeaLevelC, double pressureSeaLevelHpa)
        {
            if (!_isInitialized) return "Error: Application not fully initialized.";
            if (AltitudeUnitComboBox.SelectedIndex < 0 || TemperatureUnitComboBox.SelectedIndex < 0 || PressureUnitComboBox.SelectedIndex < 0)
            {
                return "Error: Invalid unit selection.";
            }

            bool displayFeet = AltitudeUnitComboBox.SelectedIndex == 0;
            bool displayFahrenheit = TemperatureUnitComboBox.SelectedIndex == 1;
            bool displayInHg = PressureUnitComboBox.SelectedIndex == 1;

            double displayAltitudeVal = displayFeet ? altitudeM / 0.3048 : altitudeM;
            string displayAltitudeUnit = displayFeet ? "Feet" : "Meters";

            double displayTempVal = displayFahrenheit ? result.TemperatureC * 9.0 / 5.0 + 32.0 : result.TemperatureC;
            string displayTempUnit = displayFahrenheit ? "°F" : "°C";

            double displayPressureVal = displayInHg ? result.PressureHpa / 33.8639 : result.PressureHpa;
            string displayPressureUnit = displayInHg ? "inHg" : "hPa";

            var ci = CultureInfo.InvariantCulture; // Use invariant culture for consistent formatting
            return string.Format(ci,
                "At {0:F2} {1}:\r\n" +
                "--------------------------\r\n" +
                "Temperature = {2:F2} {3}\r\n" +
                "Pressure = {4:F2} {5} ({6:F2}% of sea level)\r\n" +
                "Density = {7:F6} kg/m³ ({8:F2}% of sea level)\r\n" +
                "Specific Humidity = {9:F4} g/kg\r\n" +
                "Vapor Pressure = {10:F2} hPa",
                displayAltitudeVal, displayAltitudeUnit,
                displayTempVal, displayTempUnit,
                displayPressureVal, displayPressureUnit, result.PercentPressure,
                result.DensityKgM3, result.PercentDensity,
                result.SpecificHumidityGKg, result.VaporPressureHpa);
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized) return;

            AltitudeTextBox.Text = string.Empty;
            TemperatureTextBox.Text = string.Empty;
            PressureTextBox.Text = string.Empty;

            ApplySettingsToUI(); // Reset combos to saved/default state

            ResultTextBlock.Text = string.Empty;
            ResultBorder.Visibility = Visibility.Collapsed;
            _calculationCache.Clear();
            ClearStatus();
            LogEvent("Inputs cleared.");

            AltitudeTextBox.Focus(FocusState.Programmatic);
        }

        private async void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized) return;
            await ExportResultsAsync();
        }

        private async void ExportMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized) return;
            await ExportResultsAsync();
        }

        private async Task ExportResultsAsync()
        {
            if (string.IsNullOrWhiteSpace(ResultTextBlock.Text))
            {
                ShowStatus("No results to export.", InfoBarSeverity.Informational);
                return;
            }

            FileSavePicker savePicker = new();
            savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            savePicker.FileTypeChoices.Add("Text File", new List<string>() { ".txt" });
            savePicker.FileTypeChoices.Add("CSV File", new List<string>() { ".csv" });
            savePicker.FileTypeChoices.Add("All Files", new List<string>() { "." });
            savePicker.SuggestedFileName = "isa_results";

            // Initialize file picker with window handle for WinUI 3 Desktop
            IntPtr hwnd = WindowNative.GetWindowHandle(this);
            InitializeWithWindow.Initialize(savePicker, hwnd);

            StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                try
                {
                    CachedFileManager.DeferUpdates(file);
                    await FileIO.WriteTextAsync(file, ResultTextBlock.Text);
                    Windows.Storage.Provider.FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                    if (status == Windows.Storage.Provider.FileUpdateStatus.Complete)
                    {
                        ShowStatus($"Results exported successfully to {file.Name}", InfoBarSeverity.Success);
                        LogEvent($"Results exported to {file.Path}");
                    }
                    else
                    {
                        ShowStatus($"File '{file.Name}' couldn't be saved.", InfoBarSeverity.Error, "Export Error");
                        LogEvent($"Export failed: File status not complete for {file.Path}");
                    }
                }
                catch (Exception ex)
                {
                    ShowStatus($"Error exporting file: {ex.Message}", InfoBarSeverity.Error, "Export Error");
                    LogEvent($"Export failed: {ex.Message}");
                }
            }
            else
            {
                LogEvent("Export cancelled by user.");
            }
        }

        private async void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized) return;
            await ShowSettingsDialogAsync();
        }
        private async void SettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized) return;
            await ShowSettingsDialogAsync();
        }

        private async Task ShowSettingsDialogAsync()
        {
            var dialog = new SettingsDialog(_appSettings.Gravity, _appSettings.GasConstantR);
            dialog.XamlRoot = this.Content.XamlRoot; // Associate with current window XAML root

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                // Check if values actually changed to avoid unnecessary cache clear / recalc
                bool changed = Math.Abs(_appSettings.Gravity - dialog.GravityValue) > 1e-9 ||
                               Math.Abs(_appSettings.GasConstantR - dialog.GasConstantValue) > 1e-9;

                _appSettings.Gravity = dialog.GravityValue;
                _appSettings.GasConstantR = dialog.GasConstantValue;
                LogEvent($"Settings updated: g={_appSettings.Gravity}, R={_appSettings.GasConstantR}");
                await SaveSettingsAsync(); // Save immediately

                if (changed)
                {
                    _calculationCache.Clear(); // Invalidate cache if constants affecting calculation change
                    LogEvent("Calculation cache cleared due to settings change.");
                    // Trigger recalculation if inputs are present
                    if (!string.IsNullOrWhiteSpace(AltitudeTextBox.Text) &&
                        !string.IsNullOrWhiteSpace(TemperatureTextBox.Text) &&
                        !string.IsNullOrWhiteSpace(PressureTextBox.Text))
                    {
                        CalculateButton_Click(this, new RoutedEventArgs()); // Use valid arguments
                    }
                }
                ShowStatus("Settings saved.", InfoBarSeverity.Success);
            }
            else
            {
                LogEvent("Settings change cancelled.");
            }
        }

        private async void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AboutDialog();
            dialog.XamlRoot = this.Content.XamlRoot;
            await dialog.ShowAsync();
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}