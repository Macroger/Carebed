
using Carebed.Infrastructure.Enums;
using Carebed.Infrastructure.EventBus;
using Carebed.Infrastructure.Logging;
using Carebed.Infrastructure.Message;
using Carebed.Infrastructure.Message.ActuatorMessages;
using Carebed.Infrastructure.Message.SensorMessages;
using Carebed.Infrastructure.MessageEnvelope;

namespace Carebed.Managers
{
    public class LoggingManager : IManager, IDisposable
    {
        #region Properties and Fields

        private string _logDir = "";
        private string _filePath = "";

        private readonly IEventBus _eventBus;
        private readonly ILoggingService _loggingService;   

        // Handler for global messages
        private readonly Action<IMessageEnvelope> _logMessageHandler;
      
        #endregion

        #region Constructor(s)
        public LoggingManager(string logDir, string filePath, ILoggingService loggingService, IEventBus eventBus)
        {
            UpdateLogLocation(logDir, filePath);
            try
            {
                _loggingService = loggingService;
                _eventBus = eventBus;

                // Register the log message handler
                _logMessageHandler = HandleLogCommand;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to initialize LoggingManager with the provided logging service.", ex);
            }
        }

        #region Log Methods
        public void LogSensorData<T>(T payload, string message = "SensorData", LogLevelEnum level = LogLevelEnum.Info)
        {
            var lm = new LogMessage
            {
                Timestamp = DateTimeOffset.UtcNow,
                Level = level,
                Origin = MessageOrigins.SensorManager,
                Type = MessageTypes.SensorData,
                Message = message,
                PayloadJson = SerializePayload(payload)
            };
            //_logger.Log(lm);
        }

        
        public void Log(IEventMessage message)
        {             
            //var lm = new LogMessage
            //{
            //    Timestamp = DateTimeOffset.UtcNow,
            //    Level = LogLevelEnum.Info,
            //    Origin = message.Origin,
            //    Type = message.Type,
            //    Message = message.GetType().Name,
            //    PayloadJson = SerializePayload(message)
            //};
            //_logger.Log(lm);
        }

        //public void Log(MessageOrigins origin, MessageTypes type, string message, object? payload = null, LogLevelEnum level = LogLevelEnum.Info)
        //{
        //    var lm = new LogMessage
        //    {
        //        Timestamp = DateTimeOffset.UtcNow,
        //        Level = level,
        //        Origin = origin,
        //        Type = type,
        //        Message = message,
        //        PayloadJson = SerializePayload(payload)
        //    };
        //    //_logger.Log(lm);
        //}

        private static string SerializePayload(object? payload)
        {
            if (payload == null) return string.Empty;
            try
            {
                return System.Text.Json.JsonSerializer.Serialize(payload, new System.Text.Json.JsonSerializerOptions { WriteIndented = false });
            }
            catch
            {
                return payload.ToString() ?? string.Empty;
            }
        }
        #endregion

        #region Methods

        public void HandleLogCommand(IMessageEnvelope envelope)
        {
            _loggingService.Log(envelope.Payload);
        }

        /// <summary>
        /// Checks if the provided file path is valid.
        /// </summary>
        /// <param name="path">The file path to validate.</param>
        /// <returns>True if the file path is valid; otherwise, false.</returns>
        public static bool IsValidFilePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            char[] invalidChars = Path.GetInvalidPathChars();
            return path.IndexOfAny(invalidChars) == -1;
        }

        /// <summary>
        /// Updates the log directory and file path.
        /// </summary>
        /// <param name="logDir">The new log directory.</param>
        /// <param name="filePath">The new log file path.</param>
        public void UpdateLogLocation(string logDir, string filePath)
        {
            try
            {
                if (!Directory.Exists(logDir))
                {
                    Directory.CreateDirectory(logDir);
                }

                if (!IsValidFilePath(filePath))
                {
                    throw new ArgumentException($"Invalid file path: {filePath}", nameof(filePath));
                }

                _logDir = logDir;
                _filePath = filePath;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to update log directory and file path.", ex);
            }
        }

        public Task ShutdownAsync()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
        #endregion






    }
}