csharp Carebed\Models\Sensors\BloodOxygenSensor.cs
using System;
using Carebed.Infrastructure.Message;

namespace Carebed.Domain.Sensors
{
    /// <summary>
    /// Simulated blood oxygen (SpO2) sensor.
    /// </summary>
    internal sealed class BloodOxygenSensor : AbstractSensor
    {
        private readonly double _min;
        private readonly double _max;
        private readonly double _criticalThreshold;

        public BloodOxygenSensor(string source, double min = 85.0, double max = 100.0, double criticalThreshold = 90.0)
            : base(source)
        {
            _min = min;
            _max = max;
            _criticalThreshold = criticalThreshold;
        }

        public override SensorData ReadData()
        {
            var value = Random.Shared.NextDouble() * (_max - _min) + _min;
            var meta = BuildMetadata(("Unit", "%"), ("Sensor", "SpO2"));
            return new SensorData(value, Source, value < _criticalThreshold, meta);
        }
    }
}