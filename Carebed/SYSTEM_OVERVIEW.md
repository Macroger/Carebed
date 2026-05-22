# Carebed System Overview

## Executive Summary

Carebed is a modular, event-driven .NET 8 application designed for real-time monitoring and control of medical bed sensors and actuators. The system enables continuous health monitoring through distributed sensors while providing automated control capabilities through actuators (bed motors, lights, etc.). Built on a loosely-coupled, message-based architecture, Carebed prioritizes extensibility, reliability, and clear separation of concerns.

---

## System Architecture

### Design Philosophy

Carebed employs an **event-driven, publish-subscribe architecture** with the following core principles:

- **Decoupling:** Components communicate via an event bus rather than direct dependencies
- **Modularity:** Each functional area (sensors, actuators, alerts, logging) operates independently
- **Extensibility:** New sensors, actuators, and managers can be added without modifying core infrastructure
- **Resilience:** Failures in one component do not cascade to others
- **Testability:** Clear interfaces and message contracts enable comprehensive unit and integration testing

### High-Level Module Interaction

```
┌─────────────────────────────────────────────────────────────────┐
│                         UI Layer (Windows Forms)                 │
│                    MainDashboard & Alert Popups                  │
└────────────────────────────┬──────────────────────────────────────┘
                             │
         ┌───────────────────┼───────────────────┐
         │                   │                   │
    ┌────▼────┐         ┌────▼────┐         ┌────▼────┐
    │ Sensor  │         │Actuator │         │ Alert   │
    │Manager  │         │Manager  │         │Manager  │
    └────┬────┘         └────┬────┘         └────┬────┘
         │                   │                   │
         └───────────────────┼───────────────────┘
                             │
                    ┌────────▼─────────┐
                    │   Event Bus      │
                    │(Publish-Subscribe)
                    └────────┬─────────┘
                             │
         ┌───────────────────┼───────────────────┐
         │                   │                   │
    ┌────▼────┐         ┌────▼────┐         ┌────▼────┐
    │ Sensors │         │Actuators│         │Logging  │
    │ (Physical)         │(Physical)         │Manager  │
    └─────────┘         └─────────┘         └─────────┘
```

---

## Core Components

### 1. **Event Bus** (`Infrastructure/EventBus/`)

The event bus is the nervous system of Carebed, enabling asynchronous, decoupled communication between all modules.

**Key Responsibilities:**
- Maintains a registry of subscribers for different message types
- Publishes messages asynchronously to all interested subscribers
- Supports both global message broadcasts and type-specific subscriptions
- Handles lifecycle management (initialization and shutdown)

**How It Works:**
- Components subscribe to specific message types using `Subscribe<T>(handler)` where T is a message type
- When a component publishes a message, the bus automatically routes it to all registered subscribers
- Each message is wrapped in a `MessageEnvelope` containing metadata (origin, type, timestamp)
- No direct dependency exists between publishers and subscribers—all communication is mediated

**Impact:**
- Enables true plug-and-play architecture: new managers can subscribe to existing messages without touching other code
- Supports dynamic behavior changes (e.g., adding alert conditions) by adding new subscribers

---

### 2. **Sensor Management** (`Managers/SensorManager.cs`, `Models/Sensors/`)

The sensor subsystem continuously collects health and environmental data from multiple data sources.

**Architecture:**
- **Sensors** are polling-based devices that read physical data (temperature, heart rate, blood oxygen, EEG, patient state)
- **SensorManager** operates a single timer that polls all registered sensors at fixed intervals (default 1500ms)
- Each sensor reading is packaged into a `SensorTelemetryMessage` and published to the event bus
- Sensor state changes (e.g., Error, Ready, Offline) trigger `SensorStatusMessage` events

**How It Works:**
1. On startup, SensorManager registers all available sensors and subscribes to sensor command messages
2. At each timer interval:
   - Manager calls `ReadData()` on each sensor
   - Each reading is wrapped in a telemetry message
   - Message is published to the event bus (AlertManager and LoggingManager listen)
3. If a sensor transitions states, a status message is published
4. Remote commands (e.g., "recalibrate sensor") are received as `SensorCommandMessage` events

**Supported Sensors:**
- Temperature Sensor (core vitals)
- Heart Rate Sensor (cardiac monitoring)
- Blood Oxygen Sensor (oxygenation status)
- EEG Sensor (neurological activity)
- Patient-Up Sensor (occupancy/alert trigger)

**Key Properties:**
- Fully simulated sensors for testing and development
- Non-blocking polling prevents UI freezes
- Extensible: add new sensor types by implementing `ISensor` interface

---

### 3. **Actuator Management** (`Managers/ActuatorManager.cs`, `Models/Actuators/`)

The actuator subsystem handles all motorized and electronic bed control components.

**Architecture:**
- **Actuators** are controllable devices that affect the physical environment (bed position, lighting, alarms)
- **ActuatorManager** maintains a registry of actuators indexed by ID for rapid command routing
- Commands are issued via `ActuatorCommandMessage` events from the UI or other managers
- Each state change produces status or error messages that propagate through the event bus

**How It Works:**
1. On startup, ActuatorManager registers all available actuators (indexed by unique ID)
2. When a command arrives (e.g., "raise bed head"):
   - Manager looks up the target actuator by ID
   - Calls `TryExecute(command)` on the actuator
   - Actuator validates the command against its current state machine
   - If valid, actuator transitions state; if invalid, command is rejected
3. Actuator state changes trigger `ActuatorStatusMessage` or `ActuatorErrorMessage` events
4. UI and AlertManager listen to these messages to update displays or trigger alerts

**Supported Actuators:**
- Bed Position Motor (bed height adjustment)
- Head Tilt Motor (upper body positioning)
- Leg Raise Motor (leg support positioning)
- Bed Lamp (adjustable lighting)

**Key Features:**
- State machine validation ensures only safe transitions occur
- Telemetry data (e.g., current position) available on demand
- Fully simulated for testing; can be replaced with real hardware drivers
- Extensible: add new actuator types by implementing `IActuator` interface

---

### 4. **Alert Management** (`Managers/AlertManager.cs`)

The alert subsystem monitors sensor and actuator streams to detect critical conditions and generate user-facing alerts.

**Architecture:**
- AlertManager subscribes to all sensor telemetry, sensor status, actuator status, and error messages
- When abnormal conditions are detected, an `AlertMessage` is published and a popup is displayed
- Alerts persist in an active alert cache and can be dismissed by the user
- User dismissals generate `AlertClearMessage` events that propagate back to the system

**How It Works:**
1. AlertManager receives telemetry and status messages from the event bus
2. For each message, it evaluates alert rules:
   - **Sensor alerts:** Triggered by out-of-range values, error conditions, or state transitions
   - **Actuator alerts:** Triggered by error states or unexpected state transitions
3. If an alert condition is met:
   - Alert is added to the active alert cache (prevents duplicates)
   - `AlertMessage` is published with severity, timestamp, and description
   - Alert popup is displayed on the MainDashboard UI
4. User dismissal:
   - UI sends `AlertClearMessage` with the alert ID
   - AlertManager removes from cache
   - `AlertClearAckMessage` confirms dismissal to the UI

**Alert Categories:**
- **Critical:** Sensor/actuator errors, out-of-range vitals
- **Warning:** State anomalies, command failures
- **Info:** Informational state changes or thresholds

**Impact:**
- Centralizes alarm logic: new alert rules can be added without modifying sensors or actuators
- Prevents alert storms through deduplication
- Maintains clear audit trail: all alerts are logged and timestamped

---

### 5. **Logging Management** (`Managers/LoggingManager.cs`)

The logging subsystem provides persistent audit trails and debugging information.

**Architecture:**
- LoggingManager subscribes to all events published on the event bus (global subscription)
- Messages are formatted and written to disk in configurable directories
- Supports log rotation, level filtering, and remote configuration
- UI can dynamically start/stop logging via `LoggerCommandMessage`

**How It Works:**
1. On startup, LoggingManager initializes the file logger with a specified path (default: `Logs/app_log.txt`)
2. For each event published:
   - LoggingManager receives the message envelope (via global subscription)
   - Extracts origin, type, payload, and metadata
   - Formats as a timestamped log entry
   - Asynchronously writes to the log file
3. UI can send commands (Start/Stop/ChangeLogPath) via `LoggerCommandMessage`
4. Commands are acknowledged with `LoggerCommandAckMessage`

**Key Features:**
- Non-intrusive: doesn't modify messages, just observes them
- Configurable output format and directory
- Async I/O prevents UI blocking
- Critical events marked for prioritization in log review

**Impact:**
- Provides compliance and audit trail for medical device scenarios
- Enables post-incident analysis and debugging
- Can be extended for remote logging, analytics, or compliance reporting

---

### 6. **Message Infrastructure** (`Infrastructure/Message/`)

Messages are the data contracts that connect all system components.

**Message Types:**
- **Sensor Messages:** `SensorTelemetryMessage`, `SensorStatusMessage`, `SensorErrorMessage`, `SensorCommandMessage`
- **Actuator Messages:** `ActuatorCommandMessage`, `ActuatorStatusMessage`, `ActuatorErrorMessage`, `ActuatorTelemetryMessage`
- **Alert Messages:** `AlertMessage`, `AlertClearMessage`, `AlertActionMessage`
- **Logger Messages:** `LoggerCommandMessage`, `LoggerCommandAckMessage`

**Message Envelope:**
- Each message is wrapped in a `MessageEnvelope<T>` containing:
  - Unique message ID (for tracing)
  - Timestamp (when created)
  - Origin (which manager sent it)
  - Type (what category of message)
  - Payload (the actual data)

**Impact:**
- Standardized format enables logging, routing, and debugging
- Metadata allows cross-cutting concerns (logging, security) to operate uniformly
- Loose coupling via interfaces: components only know about message contracts, not concrete types

---

### 7. **UI Layer** (`UI/`)

The user interface provides real-time monitoring and control capabilities.

**Architecture:**
- **MainDashboard:** Primary window displaying current sensor readings, actuator states, and system status
- **Alert Popups:** Non-blocking modals for urgent alerts with dismiss/acknowledge options
- **Dashboard Components:** Live sensor value displays, actuator control buttons, alert history

**How It Works:**
1. MainDashboard subscribes to relevant event bus messages upon initialization
2. When telemetry messages arrive, dashboard updates displays in real-time
3. When alert messages arrive, popups are created and displayed
4. User interactions (button clicks, dismissals) generate command messages and publish them to the event bus
5. The event bus routes commands to appropriate managers for execution

**User Actions → Command Flow:**
- Click "Raise Bed Head" → `ActuatorCommandMessage` published → ActuatorManager receives → Bed Position Motor adjusts
- Dismiss Alert → `AlertClearMessage` published → AlertManager removes from cache → LoggingManager records dismissal

**Impact:**
- Decoupled from business logic: UI changes don't require manager changes
- Real-time responsiveness: event-driven updates eliminate polling overhead
- Clear user feedback: all state changes are immediately visible

---

## Data Flow Examples

### Example 1: Sensor Monitoring Loop

```
1. SensorManager timer fires
   ↓
2. Manager calls sensor.ReadData() for each sensor (non-blocking)
   ↓
3. Each reading packaged into SensorTelemetryMessage
   ↓
4. Messages published to event bus
   ↓
5. AlertManager receives → evaluates thresholds
6. LoggingManager receives → writes to log file
7. MainDashboard receives → updates telemetry display
```

### Example 2: Actuator Command Execution

```
1. User clicks "Raise Bed" button in UI
   ↓
2. MainDashboard publishes ActuatorCommandMessage("bedPosition", "raise")
   ↓
3. ActuatorManager receives command
   ↓
4. ActuatorManager retrieves actuator by ID from registry
   ↓
5. Calls actuator.TryExecute(RaiseCommand)
   ↓
6. Actuator's state machine validates: Current state = Idle → Valid transition to Moving
   ↓
7. Actuator changes state and returns true
   ↓
8. ActuatorManager publishes ActuatorStatusMessage (Moving)
   ↓
9. AlertManager receives → no alert (expected state)
10. LoggingManager receives → logs command execution
11. MainDashboard receives → updates UI to show "Bed Raising"
```

### Example 3: Alert Detection and User Acknowledgment

```
1. BloodOxygenSensor.ReadData() returns value of 85% (below normal)
   ↓
2. SensorManager publishes SensorTelemetryMessage with value 85
   ↓
3. AlertManager receives message
   ↓
4. AlertManager checks thresholds: 85% < 90% minimum → ALERT
   ↓
5. AlertManager publishes AlertMessage (Critical: Low Blood Oxygen)
   ↓
6. AlertManager creates AlertPopup on MainDashboard
   ↓
7. LoggingManager logs the alert
   ↓
8. User reads "Low Blood Oxygen (85%)" and clicks Dismiss
   ↓
9. MainDashboard publishes AlertClearMessage
   ↓
10. AlertManager receives → removes from active alerts cache
11. LoggingManager receives → logs dismissal with user ID
12. AlertPopup is removed from screen
```

---

## System Lifecycle

### Startup Sequence

1. **Application Launch** (`Program.cs`)
   - Initializes Windows Forms
   - Calls `SystemInitializer.Initialize()`

2. **System Initialization** (`SystemInitializer.cs`)
   - Creates event bus (BasicEventBus)
   - Instantiates all sensors
   - Instantiates all actuators
   - Instantiates managers: SensorManager, ActuatorManager, AlertManager, LoggingManager
   - Creates MainDashboard UI
   - Calls `Start()` on each manager

3. **Manager Startup** (each manager's `Start()` method)
   - Subscribes to relevant message types on event bus
   - Publishes initial inventory messages (sensor/actuator lists)
   - Starts internal timers or housekeeping tasks
   - Emits status message "Manager Ready"

4. **Runtime**
   - Event bus begins routing messages between all components
   - SensorManager poll timer continuously collects sensor data
   - AlertManager actively monitors for anomalies
   - LoggingManager records all activity
   - UI remains responsive and updates in real-time

### Shutdown Sequence

1. User closes MainDashboard window
2. `FormClosed` event fires
3. Calls `eventBus.Shutdown()`
4. Event bus stops accepting new messages
5. All subscribers notified of shutdown
6. LoggingManager flushes final logs to disk
7. Process terminates cleanly

---

## Extensibility Points

### Adding a New Sensor Type

1. Create a new class inheriting from `AbstractSensor`
2. Implement `ReadData()` to return sensor-specific data
3. Optionally override `Start()` and `Stop()` for lifecycle
4. Register in `SystemInitializer.cs` in the sensors list
5. Existing infrastructure (polling, telemetry, alerts) automatically supports it

### Adding a New Actuator Type

1. Create a new class implementing `IActuator`
2. Implement state machine using `StateMachine` utility class
3. Implement command validation in `TryExecute()`
4. Implement state property and event handler
5. Register in `SystemInitializer.cs` in the actuators list
6. Existing infrastructure (command routing, status reporting) automatically supports it

### Adding a New Manager

1. Create a class implementing `IManager` (requires `Start()`, `Stop()`, `Dispose()`)
2. Subscribe to relevant message types in constructor
3. Implement event handlers for subscribed message types
4. Implement `Start()` to initialize background tasks
5. Implement `Stop()` to clean up resources
6. Register in `SystemInitializer.cs` in the managers list
7. New manager is automatically initialized and receives messages

### Adding a New Alert Rule

1. Open `AlertManager.cs`
2. Identify the handler method that receives the relevant messages (e.g., `HandleSensorMessage`)
3. Add condition check in the rule evaluation section
4. Create `AlertMessage` payload and publish if condition matches
5. AlertPopup automatically appears on UI without additional changes

---

## Key Design Patterns

### 1. **Event Bus / Pub-Sub Pattern**
- Loose coupling: components don't know about each other
- Scalability: adding components doesn't require modifying existing ones
- Testability: message streams can be simulated without real hardware

### 2. **Manager Pattern**
- Centralized lifecycle management
- Uniform interface for all long-running services
- Easy to add, remove, or replace managers

### 3. **State Machine Pattern**
- Ensures actuators only execute valid commands
- Prevents unsafe state transitions (e.g., moving bed while locked)
- Clear, auditable state history

### 4. **Message Envelope Pattern**
- Standardized format enables cross-cutting concerns (logging, security)
- Metadata (origin, timestamp) enables tracing and debugging
- Decouples message payload format from routing logic

### 5. **Strategy Pattern (Sensors/Actuators)**
- Multiple sensor/actuator implementations share common interface
- Easy to swap implementations (real vs. simulated, different manufacturers)
- Polymorphic behavior without tight coupling

---

## Performance & Reliability Considerations

### Non-Blocking I/O
- All event publishing is asynchronous (`PublishAsync`)
- File logging uses async writes to prevent UI freezes
- Sensor polling and command execution happen off the UI thread

### Fault Isolation
- Sensor reading failure doesn't crash the system
- Actuator error triggers alert message but doesn't prevent other actuators from operating
- Manager failure is contained; other managers continue

### Graceful Degradation
- Missing sensors: SensorManager continues polling others
- Missing actuators: ActuatorManager continues serving others
- Alert generation failure: system logs error but continues monitoring

### Traceability
- Every message has unique ID, timestamp, and origin
- All events logged to disk for compliance and debugging
- Message correlation IDs link related events across the system

---

## Security & Validation

### Command Validation
- ActuatorManager validates all actuator commands via state machines
- Only commands from trusted UI are published (Windows Forms app, no remote access in v1)
- Invalid commands are rejected and error logged

### State Consistency
- Sensors read-only; no writes possible (immutable telemetry)
- Actuator commands validated by state machine before execution
- AlertManager prevents alert storms through deduplication

### Audit Trail
- LoggingManager records all state transitions
- All alerts timestamped with user/system origin
- Log files retained for compliance review

---

## Summary

Carebed is a sophisticated but modular system that separates concerns cleanly:

- **Sensors** continuously gather raw data
- **Actuators** execute controlled commands
- **Managers** orchestrate domain logic (alerting, logging, command routing)
- **Event Bus** enables loose coupling and asynchronous communication
- **UI** provides user control and real-time visibility
- **Messages** serve as the universal data contract

This architecture prioritizes **extensibility, reliability, testability, and maintainability** while supporting the real-time responsiveness required in a medical monitoring environment. New features, sensors, actuators, or managers can be added with minimal impact on existing code, and failures are isolated to prevent cascade effects.
