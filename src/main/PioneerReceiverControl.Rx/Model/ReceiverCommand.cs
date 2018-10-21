using System;
using System.Collections.Generic;
using System.Text;
using IPioneerReceiverControl.Rx.Model.Command;
using IPioneerReceiverControl.Rx.Model.Enum;

namespace PioneerReceiverControl.Rx.Model
{
    public class ReceiverCommand : IReceiverCommand
    {
        public KeyValuePair<CommandName, object> KeyValue { get; set; }
        public DateTime CommandTime { get; }
    }
}
