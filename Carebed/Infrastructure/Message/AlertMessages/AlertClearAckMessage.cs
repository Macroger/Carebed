namespace Carebed.Infrastructure.Message.AlertMessages
{
    /// <summary>
    /// This class represents an alert clear acknowledgment message.
    /// It is used to confirm that an alert has been cleared.
    /// </summary>
    public class AlertClearAckMessage : AlertBaseMessage<object?>
    {
        public AlertClearAckMessage()
        {
            Payload = null;
        }

        public bool alertCleared = true;
    }
}
