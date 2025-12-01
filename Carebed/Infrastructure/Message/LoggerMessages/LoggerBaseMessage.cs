using Carebed.Infrastructure.Enums;
using Carebed.Infrastructure.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carebed.Infrastructure.Message.LoggerMessages
{
    /// <summary>
    /// Base class for logger messages.
    /// Used to define common properties for all logger messages.
    /// </summary>
    public abstract class LoggerBaseMessage : IEventMessage
    {
        /// <summary>
        /// Time when the message was created.
        /// </summary>
        public virtual DateTime CreatedAt { get; init; } = DateTime.UtcNow;

        /// <summary>
        /// A correlation ID to link related messages together.
        /// </summary>
        public virtual Guid CorrelationId { get; init; } = Guid.NewGuid();

        /// <summary>
        /// A dictionary for additional metadata. Should contain key-value pairs relevant to the log message.
        /// </summary>
        public virtual IReadOnlyDictionary<string, string>? Metadata { get; init; }

        /// <summary>
        /// A flag indicating whether the message is critical.
        /// </summary>
        public virtual bool IsCritical { get; init; } = false;
    }
}
