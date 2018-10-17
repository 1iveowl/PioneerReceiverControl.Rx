using System;
using System.Collections.Generic;
using System.Text;

namespace IPioneerReceiverControl.Rx.Model.Command
{
    public interface IReceiverCommandDefinition
    {
        string Function { get; }
        string Name { get; }
        string CommandTemplate { get; }
        string ResponseTemplate { get; }
        Type CommandParameterType { get; }
        Type ResponseParameterType { get; }
    }
}
