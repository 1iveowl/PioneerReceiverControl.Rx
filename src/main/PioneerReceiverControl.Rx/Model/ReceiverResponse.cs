using System;
using System.Collections.Generic;
using System.Text;
using IPioneerReceiverControl.Rx.Model.Command;

namespace PioneerReceiverControl.Rx.Model
{
    public class ReceiverResponse : IReceiverResponse
    {
        public object ResponseValue { get; internal set; }
        public DateTime ResponseTime { get; internal set; }
        public bool WaitingForResponseTimedOut { get; internal set; }
    }
}
