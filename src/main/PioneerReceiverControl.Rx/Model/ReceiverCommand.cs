using System;
using System.Collections.Generic;
using System.Text;
using IPioneerReceiverControl.Rx.Model.Command;

namespace PioneerReceiverControl.Rx.Model
{
    public class ReceiverCommand : IReceiverCommand
    {
        public string Function { get; set; }
        public string Name { get; set; }
        public string CommandTemplate { get; set; }
        public string CommandParameter { get; set; }
        public string ResponseTemplate { get; set; }
    }
}
