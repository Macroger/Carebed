using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carebed.Infrastructure.Enums
{
    public enum SensorErrorCodes
    {
        None = 0,
        SensorDisconnected = 1,
        SensorMalfunction = 2,
        DataOutOfRange = 3,
        CalibrationError = 4,
        LowBattery = 5,
        CommunicationError = 6
    }
}
