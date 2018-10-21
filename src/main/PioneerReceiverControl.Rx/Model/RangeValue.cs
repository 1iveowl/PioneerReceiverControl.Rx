using System;
using System.Collections.Generic;
using System.Text;
using IPioneerReceiverControl.Rx.Model;

namespace PioneerReceiverControl.Rx.Model
{
    public class RangeValue : IRangeValue
    {
        public int Max { get; internal set; }
        public int Min { get; internal set; }
        public int StepInterval { get; internal set; }
        public double? NummericValue { get; internal set; }
        public string StringValue { get; internal set; }
        public string StringJsonValue => NummericValue.ToString();
    }
}
