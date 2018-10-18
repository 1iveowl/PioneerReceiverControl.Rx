using System;
using IPioneerReceiverControl.Rx.Model.Enum;

namespace IPioneerReceiverControl.Rx.Model.Command
{
    public interface IReceiverCommandDefinition
    {
        string Function { get; }
        CommandName Command { get; }
        string CommandTemplate { get; }
        string ResponseTemplate { get; }
        Type CommandParameterType { get; }
        Type ResponseParameterType { get; }
    }
}
