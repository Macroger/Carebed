using Carebed.Infrastructure.Enums;
using Carebed.Infrastructure.Message.SensorMessages;
namespace Carebed.Models.Sensors
{
    /// <summary>
    /// Simulated temperature sensor.
    /// </summary>
    public class TemperatureSensor : AbstractSensor
    {
        public TemperatureSensor(string sensorID, SensorTypes sensorType = SensorTypes.Temperature, double min = 35.0, double max = 40.0, double criticalThreshold = 45.0)
            : base(sensorID, sensorType, min, max, criticalThreshold)
        {
        }

        public override SensorData ReadDataActual()
        {
            var raw = Random.Shared.NextDouble() * (_max - _min) + _min;
            var value = Math.Round(raw, 2);
            var meta = BuildMetadata(("Unit", "°C"), ("Sensor", "Temperature"));           
            System.Guid correlationId = Guid.NewGuid();
            return new SensorData
            {
                Value = value,
                Source = SensorID,
                SensorType = this.SensorType,
                // temperature is critical when it exceeds the high threshold
                IsCritical = (value > _criticalThreshold),
                CreatedAt = DateTime.UtcNow,
                CorrelationId = correlationId,
                Metadata = meta
            };
        }
    }
}