using System;
using IPioneerReceiverControl.Rx.Model;

namespace PioneerReceiverControl.Rx.Model
{
    public class RawResponseData : IRawResponseData
    {
        public string Data { get; }
        public bool IsSuccessFul { get; }
        public DateTime TimeStamp { get; }

        public RawResponseData(string data, bool isSuccessFul)
        {
            Data = data;
            IsSuccessFul = isSuccessFul;
            TimeStamp = DateTime.Now;
        }
    }
}
