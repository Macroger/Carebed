using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carebed.Infrastructure.Message.AlertMessages
{
    public class AlertCommandAckMessage : AlertBaseMessage<object?>
    {
        public bool commandAcknowledged { get; init; } = true;
        public AlertCommandAckMessage()
        {
            Payload = null;
        }

        
        public bool IsCommandAcknowledged() => commandAcknowledged;
    }
}
