using System;
using System.Collections.Generic;
using System.Text;

namespace PioneerReceiverControl.Rx.Model
{
    public class ReceiveResponseObject
    {
        public string Command { get; set; }
        public string Parameter { get; set; }
        public DateTime Timestamp { get; set; }

        public string RawResponse { get; set; }
    }
}
