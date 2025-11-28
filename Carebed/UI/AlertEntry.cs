using Carebed.Infrastructure.Message;

namespace Carebed.UI
{
    public class AlertEntry
    {
        public string Source { get; init; } = string.Empty;
        public string AlertText { get; init; } = string.Empty;
        public IEventMessage? Payload { get; init; }
        public bool IsCritical { get; init; }
    }
}
