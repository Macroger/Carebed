using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carebed.Infrastructure.Message.UI
{
    public class AlertViewModel
    {
        public required string Text { get; set; }
        public required bool isCritical { get; set; }
    }

}
