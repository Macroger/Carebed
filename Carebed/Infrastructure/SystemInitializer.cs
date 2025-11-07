using Carebed.Infrastructure.EventBus;
using Carebed.Managers;
using Carebed.Modules;

namespace Carebed.Infrastructure
{
    internal static class SystemInitializer
    {
        public static (
            BasicEventBus eventBus,
            //placeholders for future managers
            SensorManager? sensorManager,
            ActuatorManager? actuatorManager)
            Initialize()
        {
            // create and intalize eventbus
            var eventBus = new BasicEventBus();
            eventBus.Initialize();

            // create managers placeholders
            var sensorManager = new SimulatedSensorManager(eventBus);
            //var actuatorManager = new ActuatorManager(eventBus);

            return (eventBus, sensorManager, null);
        }
    }



}



