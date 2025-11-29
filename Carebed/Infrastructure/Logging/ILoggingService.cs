using Carebed.Infrastructure.Message;

namespace Carebed.Infrastructure.Logging
{
    public interface ILoggingService
    {
        public void Log(IEventMessage message);
        public Task StartAsync();
        public Task StopAsync();
    }
}