using System;
using System.Collections.Generic;
using System.Text;
using IPioneerReceiverControl.Rx.Model;

namespace PioneerReceiverControl.Rx.Model
{
    public class RawReceiverData : IRawReceiverData
    {
        public string Data { get; }
        public bool IsSuccessFul { get; }

        public RawReceiverData(string data, bool isSuccessFul)
        {
            Data = data;
            IsSuccessFul = isSuccessFul;
        }
    }
}
