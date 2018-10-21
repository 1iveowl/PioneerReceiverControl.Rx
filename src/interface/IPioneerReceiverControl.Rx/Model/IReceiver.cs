using System;
using System.Collections.Generic;
using System.Text;
using IPioneerReceiverControl.Rx.Model.Command;

namespace IPioneerReceiverControl.Rx.Model
{
    public interface IReceiver
    {
        IEnumerable<IReceiverCommandDefinition> Commands { get; }
    }
}
