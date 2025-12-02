namespace Carebed.Infrastructure.Message.AlertMessages
{
    /// <summary>
    /// This class represents an alert action message with a generic payload. 
    /// It is used to send alerts to the UI layer for user notification.
    /// </summary>
    /// <typeparam name="TPayload"></typeparam>
    public class AlertActionMessage<TPayload>: AlertBaseMessage<TPayload> where TPayload : IEventMessage
    {
        /// <summary>
        /// Used to track how many alerts are in the system and which one this is.
        /// </summary>
        public int alertNumber { get; set; }
    }
}
