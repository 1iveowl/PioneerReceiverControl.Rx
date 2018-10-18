using System;
using System.Collections.Generic;
using System.Text;
using IPioneerReceiverControl.Rx.Model.Enum;

namespace IPioneerReceiverControl.Rx.Model.Command
{
    public interface IReceiverCommand
    {
        KeyValuePair<CommandName, object> KeyValue { get; }

        DateTime CommandTime { get; }
    }
}
