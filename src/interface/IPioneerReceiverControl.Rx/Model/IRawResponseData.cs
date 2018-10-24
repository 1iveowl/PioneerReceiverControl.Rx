using System;

namespace IPioneerReceiverControl.Rx.Model
{
    public interface IRawResponseData
    {
        string Data { get; }
        bool IsSuccessFul { get; }
        DateTime TimeStamp { get; }
    }
}
