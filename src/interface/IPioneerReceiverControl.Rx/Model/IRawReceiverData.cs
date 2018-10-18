using System;
using System.Collections.Generic;
using System.Text;

namespace IPioneerReceiverControl.Rx.Model
{
    public interface IRawReceiverData
    {
        string Data { get; }
        bool IsSuccessFul { get; }
    }
}
