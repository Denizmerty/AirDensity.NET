// Settings.cs
using System.Text.Json.Serialization;

namespace AirDensity.NET
{
    public class Settings
    {
        // Default ISA values
        public double Gravity { get; set; } = 9.80665; // m/s²
        public double GasConstantR { get; set; } = 287.05; // J/(kg·K)

        // UI Preferences (Indices for ComboBoxes)
        public int AltitudeUnitIndex { get; set; } = 1; // Default to Meters
        public int TemperatureUnitIndex { get; set; } = 0; // Default to °C
        public int PressureUnitIndex { get; set; } = 0; // Default to hPa
        public int ModelIndex { get; set; } = 1; // Default to Extended ISA

        // Parameterless constructor for JSON deserialization
        public Settings() { }
    }
}