using Carebed.Infrastructure.EventBus;
using Carebed.Managers;
using Carebed.UI;
using Carebed.Models.Actuators;
using Carebed.Models.Sensors;

namespace Carebed.Infrastructure
{
    public static class SystemInitializer
    {
        public static (BasicEventBus eventBus, List<IManager> managers, MainDashboard dashboard) Initialize()
        {
            var _eventBus = new BasicEventBus();
            
            // Create a list of simulated sensors
            var sensors = new List<AbstractSensor>
            {
                new TemperatureSensor("temp_sensor"),
                new BloodOxygenSensor("blood_o2_sensor"),
                new EegSensor("eeg_sensor"),
                new HeartRateSensor("heart_rate_sensor")
                // ... add more sensors as needed
            };

            var actuators = new List<IActuator>
            {
                new SimulatedBedLamp("SimulatedBedLampActuator1")
                // ... add more actuators as needed
            };

            var sensorManager = new SensorManager(_eventBus, sensors);
            var actuatorManager = new ActuatorManager(_eventBus, actuators);

            // Create AlertManager but do not start it until sensors are started by the UI
            var alertManager = new AlertManager(_eventBus);

            var managers = new List<IManager>
            {
                sensorManager,
                actuatorManager,
                alertManager
            };

            // Instantiate the MainDashboard, pass sensorManager and alertManager so UI controls their lifecycles
            var dashboard = new MainDashboard(_eventBus, sensorManager, alertManager);

            // Start non-sensor managers (defer starting sensors and alert manager to the UI)
            actuatorManager.Start();
            // alertManager.Start() deferred until sensors are started by the UI

            return (_eventBus, managers, dashboard);
        }
    }
}