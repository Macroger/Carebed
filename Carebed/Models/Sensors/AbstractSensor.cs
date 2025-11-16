using System;
using System.Collections.Generic;
using Carebed.Infrastructure.Message.SensorMessages;

namespace Carebed.Domain.Sensors
{   
    /// <summary>
    /// Base implementation for sensors. Provides Source property and basic lifecycle stubs.
    /// Concrete sensors implement <see cref="ReadData"/>.
    /// </summary>
    internal abstract class AbstractSensor : ISensor
    {
        protected AbstractSensor(string source)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
        }

        public string Source { get; }

        public virtual void Start() { /* default: no internal timer */ }
        public virtual void Stop() { /* default: no internal timer */ }

        /// <summary>
        /// Return a single snapshot of sensor data. Implementations should include unit metadata.
        /// </summary>
        public abstract SensorData ReadData();

        public virtual void Dispose()
        {
            // default: nothing to dispose
        }

        /// <summary>
        /// Helper for building metadata dictionaries.
        /// </summary>
        protected static IReadOnlyDictionary<string, string> BuildMetadata(params (string Key, string Value)[] pairs)
        {
            var d = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var (k, v) in pairs) d[k] = v;
            return d;
        }
    }
}