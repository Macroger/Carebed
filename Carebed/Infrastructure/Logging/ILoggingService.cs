using Carebed.Infrastructure.Message;
using Carebed.Infrastructure.MessageEnvelope;

namespace Carebed.Infrastructure.Logging
{
    public interface ILoggingService
    {
        public void Log(IMessageEnvelope envelope);

        public Task Start();
        public Task Stop();

        public void Dispose();
        public Task HandleLogAsync(IMessageEnvelope envelope);
    }
}