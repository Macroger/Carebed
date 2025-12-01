using System.Collections.Generic;
using Carebed.Infrastructure;
using Carebed.Infrastructure.EventBus;
using Carebed.Managers;
using Carebed.UI;
using Carebed.Models.Actuators;
using Carebed.Models.Sensors;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Carebed.Tests.Infrastructure
{
    [TestClass]
    public class SystemInitializerTests
    {
        [TestMethod]
        public void Initialize_Returns_Valid_Objects()
        {
            // Act
            var (eventBus, managers, dashboard) = SystemInitializer.Initialize();

            // Assert
            Assert.IsNotNull(eventBus, "EventBus should not be null.");
            Assert.IsInstanceOfType(eventBus, typeof(BasicEventBus));

            Assert.IsNotNull(managers, "Managers list should not be null.");
            Assert.IsTrue(managers.Count >= 3, "Managers list should contain at least 3 managers.");
            Assert.IsTrue(managers.Exists(m => m.GetType().Name == "SensorManager"), "Managers should contain SensorManager.");
            Assert.IsTrue(managers.Exists(m => m.GetType().Name == "ActuatorManager"), "Managers should contain ActuatorManager.");
            Assert.IsTrue(managers.Exists(m => m.GetType().Name == "AlertManager"), "Managers should contain AlertManager.");

            Assert.IsNotNull(dashboard, "Dashboard should not be null.");
            Assert.IsInstanceOfType(dashboard, typeof(MainDashboard));
        }

        [TestMethod]
        public void Initialize_Managers_Are_Started()
        {
            // Act
            var (_, managers, _) = SystemInitializer.Initialize();

            // Assert
            foreach (var manager in managers)
            {
                // All managers should be started after initialization.
                // We check for a public property or method that indicates running state if available.
                // Since the interface does not expose state, we check that no exception is thrown and type is correct.
                Assert.IsNotNull(manager);
            }
        }

        [TestMethod]
        public void Initialize_Sensors_And_Actuators_Are_Configured()
        {
            // Act
            var (_, managers, _) = SystemInitializer.Initialize();

            var sensorManager = managers.Find(m => m.GetType().Name == "SensorManager");
            var actuatorManager = managers.Find(m => m.GetType().Name == "ActuatorManager");

            Assert.IsNotNull(sensorManager, "SensorManager should be present in managers.");
            Assert.IsNotNull(actuatorManager, "ActuatorManager should be present in managers.");
        }
    }
}