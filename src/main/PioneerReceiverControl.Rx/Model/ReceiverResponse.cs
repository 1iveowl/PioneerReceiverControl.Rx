using System;
using IPioneerReceiverControl.Rx.Model;
using IPioneerReceiverControl.Rx.Model.Command;

namespace PioneerReceiverControl.Rx.Model
{
    public class ReceiverResponse : IReceiverResponse
    {
        public string ResponseToCommand { get; internal set; }
        public object ResponseValue { get; internal set; }
        public DateTime ResponseTime { get; internal set; }
        public bool WaitingForResponseTimedOut { get; internal set; }
        public string GetValueString()
        {
            if (ResponseValue is IRangeValue rv)
            {
                return rv.StringValue;
            }
            else
            {
                return ResponseValue?.ToString();
            }
        }
    }
}
