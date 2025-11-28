namespace Carebed.Infrastructure.Enums
{
    /// <summary>
    /// Indicates which module originated the message.
    /// Extend this enum as new modules are added.
    /// </summary>
    public enum MessageOrigins
    {
        Unknown = 0,
        SensorManager,
        ActuatorManager,
        LoggingManager,
        SystemInitializer,
        DisplayManager,
        AlertManager,
        NetworkManager,
        EventBus
        // Add more as needed
    }

}
