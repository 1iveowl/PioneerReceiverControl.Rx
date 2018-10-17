using System;
using System.Collections.Generic;
using System.Text;
using IPioneerReceiverControl.Rx.Model.Command;

namespace PioneerReceiverControl.Rx.Model
{
    public class ReceiverCommandDefinition : IReceiverCommandDefinition
    {
        public string Function { get; set; }
        public string Name { get; set; }
        public string CommandTemplate { get; set; }
        public string ResponseTemplate { get; set; }
        public Type CommandParameterType { get; set; }
        public Type ResponseParameterType { get; set; }
    }
}
