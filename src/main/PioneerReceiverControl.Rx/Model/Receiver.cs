using System;
using System.Collections.Generic;
using System.Text;
using IPioneerReceiverControl.Rx.Model;
using IPioneerReceiverControl.Rx.Model.Command;

namespace PioneerReceiverControl.Rx.Model
{
    public class Receiver : IReceiver
    {
        public IEnumerable<IReceiverCommandDefinition> Commands { get; set; }
    }
}
