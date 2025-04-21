# AirDensity.NET - Extended ISA Model Calculator

A Windows desktop application built with WinUI 3 and .NET 8 to calculate atmospheric properties based on the Extended International Standard Atmosphere(ISA) model.

## Overview

AirDensity.NET provides a user-friendly interface to calculate key atmospheric parameters like temperature, pressure, density, and humidity at a given geometric altitude. It takes sea-level temperature and pressure as inputs and uses the Extended ISA model for calculations up to approximately 85 km.

## Features

*   **Extended ISA Model:** Implements calculations based on the multi-layer Extended ISA model.
*   **Customizable Inputs:** Enter altitude, sea-level temperature, and sea-level pressure.
*   **Unit Selection:** Supports Meters/Feet for altitude, °C/°F for temperature, and hPa/inHg for pressure.
*   **Derived Properties:** Calculates temperature, pressure, density(kg/m³), pressure/density percentages relative to sea level, specific humidity(g/kg), and vapor pressure(hPa) at the specified altitude.
*   **Configurable Constants:** Adjust gravitational acceleration(g) and the specific gas constant for air(R) via settings.
*   **Results Export:** Export calculated results to a text or CSV file.
*   **Input Validation:** Basic checks for plausible input ranges.
*   **Caching:** Caches results for identical inputs to speed up repeated calculations.
*   **Logging:** Logs application events and potential errors to `airsensity_app.log`(located in the app's local data folder).
*   **Modern UI:** Built with WinUI 3, featuring Mica backdrop effect.

## Tech Stack

*   .NET 8
*   Windows App SDK / WinUI 3
*   WinUIEx(for enhanced windowing features)
*   C#

## Installation

You can download the latest installer package from the [Releases](https://github.com/Denizmerty/AirDensity.NET/releases) page or directly from the `dist` folder in this repository.

1.  Download the `.msix` file(e.g., `AirDensity.NET_1.0.1.0_x64.msix`).
2.  *(Optional)* If you encounter trust issues during installation(especially with the test certificate), you might need to download and install the associated `.cer` file first. Double-click the `.cer` file, click "Install Certificate...", choose "Local Machine", click "Next", select "Place all certificates in the following store", click "Browse...", choose "Trusted Root Certification Authorities", click "OK", "Next", and "Finish".
3.  Double-click the `.msix` file to install the application.

## How to Use

1.  Launch the **Extended ISA Model Calculator**.
2.  Enter the **Altitude**, **Sea-level Temperature**, and **Sea-level Pressure** in the respective text boxes.
3.  Select the appropriate **units** for each input using the dropdown menus.
4.  *(Optional)* Select the desired **Atmospheric Model**(Currently only "Extended ISA" is implemented).
5.  Click the **Calculate** button.
6.  The results will be displayed in the "Results" section.
7.  Use the **Export** button(or File > Export Results...) to save the results.
8.  Use the **Settings** button(or Tools > Settings...) to modify calculation constants(g, R).
9.  Use the **Clear** button to reset inputs and results.

## Building from Source

1.  Clone the repository: `git clone https://github.com/Denizmerty/AirDensity.NET.git`
2.  Open `AirDensity.NET.sln` in Visual Studio 2022(ensure the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) and the "Windows App SDK C# Templates" workload are installed).
3.  Select a Solution Configuration(e.g., `Debug`) and Platform(e.g., `x64`).
4.  Build the solution(F6 or Build > Build Solution).
5.  Run the project(F5 or Debug > Start Debugging).

## Contributing

Contributions are welcome! Please feel free to submit pull requests or open issues for bugs, feature requests, or improvements.

## Contact

For questions, suggestions, or issues, please open an issue on GitHub or contact me at [denizmerty@gmail.com](mailto:denizmerty@gmail.com).

## License

This project is licensed under the terms specified in the GNU General Public License v3.0
