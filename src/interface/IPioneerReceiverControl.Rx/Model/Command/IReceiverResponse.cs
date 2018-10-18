using System;
using System.Collections.Generic;
using System.Text;

namespace IPioneerReceiverControl.Rx.Model.Command
{
    public interface IReceiverResponse : IReceiverCommandDefinition
    {
        object ResponseValue { get; }
        DateTime ResponseTime { get; }
    }
}
