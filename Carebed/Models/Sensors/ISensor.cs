using System;

namespace Carebed.Domain.Sensors
{
    /// <summary>
    /// Minimal sensor contract used by the application composition root.
    /// Kept intentionally small so implementations can live in Modules or Domain
    /// without forcing heavy coupling.
    /// </summary>
    internal interface ISensor : IDisposable
    {
        /// <summary>
        /// Logical source/id for the sensor (e.g. "Room A").
        /// </summary>
        string Source { get; }

        /// <summary>
        /// Start periodic sampling/publishing.
        /// </summary>
        void Start();

        /// <summary>
        /// Stop sampling/publishing.
        /// </summary>
        void Stop();
    }
}
