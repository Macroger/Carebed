using Carebed.Infrastructure.Enums;

namespace Carebed.Infrastructure.Message.LoggerMessages
{
    public class LoggerCommandAckMessage: LoggerBaseMessage
    {

        public LoggerCommands CommandType { get; init; }
        public bool IsAcknowledged { get; init; }
        public string? Reason { get; init; } = null;
        public override Guid CorrelationId { get; init; }

        public LoggerCommandAckMessage(
            LoggerCommands commandType,
            bool? isAcknowledged = null,
            string? reason = null,
            Guid? correlationId = null)
        {
            CommandType = commandType;
            IsAcknowledged = isAcknowledged ?? true;
            Reason = reason ?? string.Empty;
            CorrelationId = correlationId ?? Guid.NewGuid();
        }
    }
}
