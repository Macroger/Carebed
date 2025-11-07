using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;
using Carebed.Domain.Sensors;
using Carebed.Infrastructure.EventBus;
using Carebed.Infrastructure.Message;
using Carebed.Infrastructure.MessageEnvelope;
using Carebed.Infrastructure.Enums;

namespace Carebed.Modules
{
    /// <summary>
    /// Top-level manager that polls configured sensors on a single timer and
    /// publishes their readings to the application's <see cref="IEventBus"/>.
    /// </summary>
    internal class SensorManager : IDisposable
    {
        private readonly IEventBus _eventBus;
        private readonly List<ISensor> _sensors;
        private readonly System.Timers.Timer _timer;
        private int _isPolling;

        public SensorManager(IEventBus eventBus, IEnumerable<ISensor> sensors, double intervalMilliseconds = 1000, ISynchronizeInvoke? synchronizingObject = null)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _sensors = sensors?.ToList() ?? throw new ArgumentNullException(nameof(sensors));

            _timer = new System.Timers.Timer(intervalMilliseconds) { AutoReset = true };
            if (synchronizingObject is not null)
                _timer.SynchronizingObject = synchronizingObject;

            _timer.Elapsed += (s, e) => _ = PollOnceAsync();
        }

        public void Start()
        {
            foreach (var s in _sensors) s.Start();
            _eventBus.Initialize();
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
            foreach (var s in _sensors) s.Stop();
            _eventBus.Shutdown();
        }

        private async Task PollOnceAsync()
        {
            // prevent overlapping polls
            if (Interlocked.Exchange(ref _isPolling, 1) == 1) return;

            try
            {
                var publishTasks = new List<Task>(_sensors.Count);
                foreach (var sensor in _sensors)
                {
                    try
                    {
                        var payload = sensor.ReadData();
                        var envelope = new MessageEnvelope<SensorData>(payload, MessageOriginEnum.SensorManager, MessageTypeEnum.SensorData);
                        publishTasks.Add(_eventBus.PublishAsync(envelope));
                    }
                    catch (Exception exSensor)
                    {
                        System.Diagnostics.Debug.WriteLine($"Sensor {sensor?.Source ?? "<unknown>"} read failed: {exSensor}");
                    }
                }

                if (publishTasks.Count > 0)
                {
                    try
                    {
                        await Task.WhenAll(publishTasks).ConfigureAwait(false);
                    }
                    catch (Exception exPub)
                    {
                        System.Diagnostics.Debug.WriteLine($"Publishing tasks failed: {exPub}");
                    }
                }
            }
            finally
            {
                Interlocked.Exchange(ref _isPolling, 0);
            }
        }

        public void Dispose()
        {
            Stop();
            _timer?.Dispose();
            foreach (var s in _sensors) s.Dispose();
        }
    }
}
