using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Carebed.Infrastructure.EventBus;
using Carebed.Infrastructure.Message;
using Carebed.Infrastructure.MessageEnvelope;
using Carebed.Infrastructure.Enums;

namespace Carebed.Infrastructure.Sensors
{
    /// <summary>
    /// Simulates a set of sensors producing random measurements and publishes them on the provided <see cref="IEventBus"/>.
    /// Intended for local testing and UI demos.
    /// </summary>
    public class SimulatedSensorManager : IDisposable
    {
        private readonly IEventBus _eventBus;
        private readonly List<SimulatedSensor> _sensors = new();
        private readonly CancellationTokenSource _cts = new();
        private readonly Task _task;
        private readonly int _intervalMs;

        /// <summary>
        /// Create a new simulated sensor manager.
        /// </summary>
        /// <param name="eventBus">Event bus used to publish sensor readings.</param>
        /// <param name="sensorCount">Number of sensors to simulate.</param>
        /// <param name="intervalMs">Base publish interval in milliseconds (each tick will publish one reading per sensor).</param>
        public SimulatedSensorManager(IEventBus eventBus, int sensorCount = 5, int intervalMs = 1000)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _intervalMs = Math.Max(100, intervalMs);

            var rnd = new Random();
            for (int i = 1; i <= sensorCount; i++)
            {
                // choose a baseline and small variability per sensor
                var baseline = 20.0 + rnd.NextDouble() * 80.0; // 20..100
                _sensors.Add(new SimulatedSensor($"sensor-{i}", baseline));
            }

            _task = Task.Run(RunAsync, _cts.Token);
        }

        private async Task RunAsync()
        {
            try
            {
                while (!_cts.IsCancellationRequested)
                {
                    foreach (var s in _sensors)
                    {
                        var value = s.NextValue();
                        var payload = new SensorData(value, s.Name);
                        var envelope = new MessageEnvelope<SensorData>(payload, MessageOriginEnum.SensorManager, MessageTypeEnum.SensorData);

                        // publish, but don't await to avoid blocking the loop
                        _ = _eventBus.PublishAsync(envelope);
                    }

                    await Task.Delay(_intervalMs, _cts.Token).ConfigureAwait(false);
                }
            }
            catch (TaskCanceledException) { }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SimulatedSensorManager loop failed: {ex}");
            }
        }

        /// <summary>
        /// Dispose the manager and stop background publishing.
        /// </summary>
        public void Dispose()
        {
            try
            {
                _cts.Cancel();
                _task.Wait(500);
            }
            catch { }
            finally
            {
                _cts.Dispose();
            }
        }

        private class SimulatedSensor
        {
            private readonly Random _rnd = new();
            public string Name { get; }
            private double _current;

            public SimulatedSensor(string name, double baseline)
            {
                Name = name;
                _current = baseline;
            }

            public double NextValue()
            {
                // small random walk
                var delta = (_rnd.NextDouble() - 0.5) * 4.0; // +/-2.0
                _current = Math.Max(0.0, _current + delta);

                // occasional spike or drop
                if (_rnd.NextDouble() < 0.02)
                {
                    _current += (_rnd.NextDouble() * 40.0) - 20.0; // -20..+20
                    if (_current < 0) _current = 0;
                }

                return Math.Round(_current, 2);
            }
        }
    }
}
