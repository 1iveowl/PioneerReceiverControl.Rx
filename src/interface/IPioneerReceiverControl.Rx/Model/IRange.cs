using System;
using System.Collections.Generic;
using System.Text;

namespace IPioneerReceiverControl.Rx.Model
{
    public interface IRange
    {
        double Max { get; }
        double Min { get; }
        double StepInterval { get; }
    }
}
