using System.Collections.Generic;
using IPioneerReceiverControl.Rx.Model.Command;

namespace IPioneerReceiverControl.Rx.Model
{
    public interface IReceiver
    {
        IEnumerable<IReceiverCommandDefinition> Commands { get; }
    }
}
