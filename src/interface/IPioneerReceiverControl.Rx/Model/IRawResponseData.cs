using System;
using System.Collections.Generic;
using System.Text;

namespace IPioneerReceiverControl.Rx.Model
{
    public interface IRawResponseData
    {
        string Data { get; }
        bool IsSuccessFul { get; }
        DateTime TimeStamp { get; }
    }
}
