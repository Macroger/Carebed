using Carebed.Infrastructure.Enums;
using Carebed.Infrastructure.EventBus;
using Carebed.Infrastructure.Logging;
using Carebed.Infrastructure.Message.LoggerMessages;
using Carebed.Infrastructure.MessageEnvelope;
using System.Threading.Tasks;

namespace Carebed.Managers
{
    public class LoggingManager : IManager, IDisposable
    {
        #region Properties and Fields

        // Log directory and file path
        private string _logDir = "";
        private string _filePath = "";

        private bool _isLoggerStarted = false;

        // Event bus for message handling
        private readonly IEventBus _eventBus;

        // Logging service for handling log messages
        private readonly IFileLoggingService _loggingService;   

        // Handler for global messages
        private readonly Action<IMessageEnvelope> _logMessageHandler;

        // Handler for log commands - uses Func to support async operations
        private readonly Func<MessageEnvelope<LoggerCommandMessage>, Task> _logCommandHandler;
        
        #endregion

        #region Constructor(s)
        public LoggingManager(string logDir, string filePath, IFileLoggingService loggingService, IEventBus eventBus)
        {
            try
            {                
                _loggingService = loggingService;
                _eventBus = eventBus;
                UpdateLogLocation(logDir, filePath);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("ERROR! Encountered following exception: ", ex);
            }

            // Register the log message handler
            _logMessageHandler = HandleLogMessage;

            // Register the log command handler
            _logCommandHandler = HandleLogCommand;
        }
        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles incoming log command messages. Processes commands such as Start, Stop, and AdjustFilePath.
        /// Is an async method to accommodate potential asynchronous operations in command handling.
        /// </summary>
        /// <param name="envelope"></param>
        /// <returns></returns>
        private async Task HandleLogCommand(MessageEnvelope<LoggerCommandMessage> envelope)
        {
            var command = envelope.Payload.Command;
            bool successfullyExecutedCommand = false;

            switch (command)
            {
                case LoggerCommands.Start:
                    if (!_isLoggerStarted)
                    {
                        await _loggingService.Start();
                        _isLoggerStarted = true;
                        successfullyExecutedCommand = true;
                    }
                    break;
                case LoggerCommands.Stop:
                    if (_isLoggerStarted)
                    {
                        await _loggingService.Stop();
                        _isLoggerStarted = false;
                        successfullyExecutedCommand = true;
                    }
                    break;
                case LoggerCommands.AdjustFilePath:
                    var metadata = envelope.Payload.Metadata ?? new Dictionary<string, string>();
                    successfullyExecutedCommand = UpdateLogLocation(
                        logDir: metadata.GetValueOrDefault("LogDirectory", _logDir),
                        filePath: metadata.GetValueOrDefault("FilePath", _filePath));
                    break;
                default:
                    // Handle unknown or unsupported command
                    break;
            }

            // Generate a response message indicating the result of the command execution
            var commandResponseMessage = new LoggerCommandAckMessage(commandType: command, isAcknowledged: successfullyExecutedCommand);

            // Create a message envelope for the response
            var responseEnvelope = new MessageEnvelope<LoggerCommandAckMessage>(
                commandResponseMessage,
                MessageOrigins.LoggingManager,
                MessageTypes.LoggerCommandResponse);

            // Publish the response message to the event bus
            await _eventBus.PublishAsync(responseEnvelope);
        }

        /// <summary>
        /// Handles incoming log messages by passing them to the logging service.
        /// </summary>
        /// <param name="envelope"></param>
        public void HandleLogMessage(IMessageEnvelope envelope)
        {
            _loggingService.Log(envelope);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Checks if the provided file path is valid.
        /// </summary>
        /// <param name="path">The file path to validate.</param>
        /// <returns>True if the file path is valid; otherwise, false.</returns>
        public static bool IsValidFilePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            char[] invalidChars = Path.GetInvalidFileNameChars().Concat(Path.GetInvalidPathChars()).Distinct().ToArray();
            return path.IndexOfAny(invalidChars) == -1;
        }

        /// <summary>
        /// Updates the log directory and file path.
        /// </summary>
        /// <param name="logDir">The new log directory.</param>
        /// <param name="filePath">The new log file path.</param>
        public bool UpdateLogLocation(string logDir, string filePath)
        {
            bool result = false;
            // Check and create the log directory if it doesn't exist
            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }

            // Check if the file path is valid
            if (!IsValidFilePath(filePath))
            {
                throw new ArgumentException($"Invalid file path: {filePath}", nameof(filePath));
            }

            // Check if the logger is started
            if (_isLoggerStarted)
            {
                throw new InvalidOperationException("Cannot change log location while logger is started.");
            }

            _logDir = logDir;
            _filePath = filePath;

            result = _loggingService.ChangeFilePath(Path.Combine(_logDir, _filePath));

            return result;
        }

        /// <summary>
        /// Disposes the logging manager and its resources.
        /// </summary>
        public void Dispose()
        {
            if(_isLoggerStarted) 
            {
                Stop();
            }

            _isLoggerStarted = false;
            _loggingService.Dispose();            
        }

        /// <summary>
        /// Starts the logging service.
        /// </summary>
        public void Start()
        {
            if( _isLoggerStarted ) return;

            _loggingService.Start();
            _isLoggerStarted = true;
        }

        /// <summary>
        /// Stops the logging service.
        /// </summary>
        public void Stop()
        {
            if( !_isLoggerStarted ) return;

            _loggingService.Stop();
            _isLoggerStarted = false;
        }

        #endregion
    }
}