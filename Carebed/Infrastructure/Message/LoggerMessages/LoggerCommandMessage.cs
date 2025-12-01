using Carebed.Infrastructure.Enums;

namespace Carebed.Infrastructure.Message.LoggerMessages
{
    public class LoggerCommandMessage : LoggerBaseMessage
    {
        /// <summary>
        /// The logging command to be executed.
        /// </summary>
        public LoggerCommands Command { get; init; }

        public LoggerCommandMessage(LoggerCommands command, IReadOnlyDictionary<string, string> metadata)
        {
            Command = command;
            Metadata = metadata;
        }

        public LoggerCommandMessage(LoggerCommands command)
        {
            Command = command;
            Metadata = new Dictionary<string, string>();
        }
    }
}
