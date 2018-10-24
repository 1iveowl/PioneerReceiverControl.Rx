using System;
using IPioneerReceiverControl.Rx.Model.Command;
using IPioneerReceiverControl.Rx.Model.Enum;

namespace PioneerReceiverControl.Rx.Model
{
    public class ReceiverCommandDefinition : IReceiverCommandDefinition
    {
        public string Function { get; set; }
        public CommandName CommandName { get; set; }
        public string CommandTemplate { get; set; }
        public string ResponseTemplate { get; set; }
        public Type CommandParameterType { get; set; }
        public Type ResponseParameterType { get; set; }
    }
}
