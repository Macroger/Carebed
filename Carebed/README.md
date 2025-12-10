# Carebed System

Carebed is a modular, event-driven .NET 8 application for monitoring and managing sensors and actuators in a care environment. It is designed for extensibility, testability, and clear separation of concerns, with a Windows Forms UI and robust infrastructure.

## Features

- **Sensor Management:** Polls and aggregates data from multiple sensors.
- **Actuator Management:** Controls actuators such as motors, lamps, and alarms.
- **Event Bus:** Decoupled event-driven communication between modules.
- **Message Envelopes:** Standardized message wrapping with metadata for routing and logging.
- **Extensible Architecture:** Easily add new sensors, actuators, or managers.
- **Unit & Integration Testing:** Includes a test project for core infrastructure and messaging.

## Project Structure

- `Carebed/` - Main application (WinForms UI, managers, infrastructure)
- `Carebed.Tests/` - Unit and integration tests for infrastructure and messaging
- `Class Sheets/` - Documentation for core classes and interfaces

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- Windows OS (for WinForms support)

### Building the Project

```
git clone https://github.com/Macroger/Carebed.git
cd Carebed
dotnet build
```

### Running the Application

```
cd Carebed
dotnet run
```
Or launch `Carebed.exe` from the build output directory.

### Running Tests

```
cd Carebed.Tests
dotnet test
```

## Documentation

- See the `Class Sheets` folder for detailed documentation on core classes and interfaces.
- Additional design notes may be available in the Azure DevOps Wiki.

## Contributing

Contributions are welcome! Please open issues or submit pull requests for bug fixes, enhancements, or new features.

## License

This project is licensed under the GNU General Public License v3.0. See the `LICENSE.txt` file for details.

---

**Copilot AI Acknowledgement:**  
Some or all of this documentation was generated or assisted by GitHub Copilot AI. Please review and validate for correctness and completeness.
