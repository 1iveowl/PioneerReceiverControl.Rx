using System;
using System.Collections.Generic;
using System.Text;
using IPioneerReceiverControl.Rx.Model.Command;

namespace IPioneerReceiverControl.Rx.Model
{
    public interface IReceiverState : IReceiverResponse
    {
        object OldResponseValue { get; }
    }
}
