using System;
using System.Collections.Generic;
using System.Text;

namespace IPioneerReceiverControl.Rx.Model.Command
{
    public interface IReceiverCommand
    {
        string Function { get; }
        string Name { get; }
        string CommandTemplate { get; }
        string CommandParameter { get; }

        string ResponseTemplate { get; }
    }
}
