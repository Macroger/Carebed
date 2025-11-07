csharp Carebed\Models\Sensors\HeartRateSensor.cs
using System;
using Carebed.Infrastructure.Message;

namespace Carebed.Domain.Sensors
{
    /// <summary>
    /// Simulated heart rate sensor (beats per minute).
    /// </summary>
    internal sealed class HeartRateSensor : AbstractSensor
    {
        private readonly int _min;
        private readonly int _max;
        private readonly int _lowCritical;
        private readonly int _highCritical;

        public HeartRateSensor(string source, int min = 40, int max = 130, int lowCritical = 40, int highCritical = 120)
            : base(source)
        {
            _min = min;
            _max = max;
            _lowCritical = lowCritical;
            _highCritical = highCritical;
        }

        public override SensorData ReadData()
        {
            var value = Random.Shared.Next(_min, _max + 1);
            var isCritical = value < _lowCritical || value > _highCritical;
            var meta = BuildMetadata(("Unit", "bpm"), ("Sensor", "HeartRate"));
            return new SensorData(value, Source, isCritical, meta);
        }
    }
}