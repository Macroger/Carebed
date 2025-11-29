using Carebed.Infrastructure.Message;
using System.Collections.Concurrent;
using System.Text.Json;

namespace Carebed.Infrastructure.Logging
{
    /// <summary>
    /// Provides a simple file-based logging service that writes log messages to a specified file.
    /// </summary>
    /// <remarks>This logger supports asynchronous logging by enqueuing messages and processing them in the
    /// background. It ensures thread safety and optimizes I/O performance by batching writes. The logger must be
    /// started using <see cref="StartAsync"/> before logging messages and should be stopped using <see
    /// cref="StopAsync"/> or disposed via <see cref="Dispose"/> to release resources properly.</remarks>
    public class SimpleFileLogger : ILoggingService, IDisposable
    {
        #region Fields and Properties

        private string _filePath;
        private StreamWriter? _writer;
        private readonly object _sync = new();
        private bool _started;

        // Queue for log messages
        private readonly ConcurrentQueue<IEventMessage> _queue = new();

        // Cancellation token for the background worker - used to stop processing
        private CancellationTokenSource? _cts;

        // Background worker task - enables asynchronous log processing
        private Task? _worker;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor for SimpleFileLogger.
        /// </summary>
        /// <param name="filePath"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public SimpleFileLogger(string filePath)
        {           
            _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Changes the file path used by the logger.
        /// </summary>
        /// <param name="newFilePath">The new file path to be used. Cannot be <see langword="null"/>.</param>
        /// <exception cref="InvalidOperationException">Thrown if the logger has already been started.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="newFilePath"/> is <see langword="null"/>.</exception>
        public void ChangeFilePath(string newFilePath)
        {
            lock (_sync)
            {
                if (_started)
                {
                    throw new InvalidOperationException("Cannot change file path while logger is started.");
                }
                _filePath = newFilePath ?? throw new ArgumentNullException(nameof(newFilePath));
            }
        }

        /// <summary>
        /// Starts the logger by initializing the file stream and background worker.
        /// </summary>
        /// <returns></returns>
        public Task StartAsync()
        {
            // Synchron - ensure only one start operation at a time
            lock (_sync)
            {
                // Check if already started - if so, do nothing
                if (_started) return Task.CompletedTask;

                // Ensure directory exists
                var dir = Path.GetDirectoryName(_filePath) ?? AppDomain.CurrentDomain.BaseDirectory;
                Directory.CreateDirectory(dir);

                // Open file stream for appending log messages
                var fs = new FileStream(_filePath, FileMode.Append, FileAccess.Write, FileShare.Read);
                _writer = new StreamWriter(fs) { AutoFlush = false };

                // Mark as started
                _started = true;

                // Set up cancellation token 
                _cts = new CancellationTokenSource();

                // Start background worker to process log queue
                _worker = Task.Run(() => ProcessQueue(_cts.Token));
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Stops the logger by cancelling the background worker and flushing remaining messages.
        /// </summary>
        /// <returns></returns>
        public Task StopAsync()
        {
            lock (_sync)
            {
                if (!_started) return Task.CompletedTask;
                _cts?.Cancel();
                try
                {
                    _worker?.Wait(1000);
                }
                catch { }
                try
                {
                    _writer?.Flush();
                    _writer?.Dispose();
                }
                catch { }
                finally
                {
                    _writer = null;
                    _started = false;
                    _cts?.Dispose();
                    _cts = null;
                    _worker = null;
                }
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Disposes the logger by stopping it synchronously.
        /// </summary>
        public void Dispose()
        {
            StopAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Logs an event message by enqueueing it for background processing.
        /// Places the message into a concurrent queue to be processed by the background worker.
        /// </summary>
        /// <param name="message"></param>
        public void Log(IEventMessage message)
        {
            // Check for null message
            if (message == null) return;

            // Ensure logger is started by checking _started flag
            if (!_started)
            {
                // Attempt to start the logger synchronously
                try
                {
                    StartAsync().GetAwaiter().GetResult();
                }
                catch
                {
                    // If starting fails, silently return without logging
                    // This prevents exceptions from propagating in logging
                    return;
                }
            }
            _queue.Enqueue(message);
        }


        #endregion

        #region Private Methods

        /// <summary>
        /// Processes the log message queue in the background. This process runs continuously until cancelled,
        /// writing log messages to the file in batches to optimize I/O performance.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task ProcessQueue(CancellationToken token)
        {
            // Flush interval - how often to flush to disk
            // This will be used to balance performance by batching writes into
            // clusters to reduce I/O overhead
            var flushInterval = TimeSpan.FromMilliseconds(200);
            var lastFlush = DateTime.UtcNow;

            // Main processing loop
            while (!token.IsCancellationRequested)
            {
                // Setup flag to track if any messages were written
                bool wrote = false;

                // Dequeue and write all available messages
                // Dequeue just means to remove from the queue
                while (_queue.TryDequeue(out var msg))
                {
                    WriteLogLine(msg);
                    wrote = true;
                }

                // Flush to disk if needed based on time interval
                if (wrote && (DateTime.UtcNow - lastFlush) > flushInterval)
                {
                    lock (_sync)
                    {
                        // Flush the writer to ensure all messages are written to disk
                        // This is the actual I/O operation
                        _writer?.Flush();
                    }
                    lastFlush = DateTime.UtcNow;
                }

                // Brief delay to avoid tight loop when idle
                await Task.Delay(50, token).ConfigureAwait(false);
            }

            // Final flush on exit
            lock (_sync)
            {
                _writer?.Flush();
            }
        }

        /// <summary>
        /// Writes a single log line to the file.
        /// </summary>
        /// <param name="message"></param>
        private void WriteLogLine(IEventMessage message)
        {
            string line;
            try
            {
                var logObject = new
                {
                    Type = message.GetType().Name,
                    Message = message
                };

                var opts = new JsonSerializerOptions { WriteIndented = false };
                line = JsonSerializer.Serialize(logObject, opts);
            }
            catch
            {
                line = $"{{\"Timestamp\":\"{message.CreatedAt:O}\",\"CorrelationId\":\"{message.CorrelationId}\"}}";
            }

            lock (_sync)
            {
                try
                {
                    _writer?.WriteLine(line);
                }
                catch
                {
                    // swallow for MVP
                }
            }
        }

        #endregion
    }
}