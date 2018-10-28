using System.Runtime.InteropServices;
using IPioneerReceiverControl.Rx.CustomException;
using IPioneerReceiverControl.Rx.Model;

namespace PioneerReceiverControl.Rx.Model.Base
{
    public class RangeValueBase : IRangeValue
    {
        public virtual double? PioneerInternalValue { get; set; }
        public virtual double? PioneerValue { get; set; }
        public string PresentationStringValue => PioneerValue != null ? $"{PioneerValue}db" : "---db(MIN)";

        internal void SetPioneerValueFromString(string strVal)
        {
            if (double.TryParse(strVal, out var x))
            {
                PioneerValue = x;
            }
            else
            {
                throw new PioneerReceiverException($"Unable to set Pioneer Value with {strVal}");
            }
        }

        internal void SetPioneerInternalValueFromString(string strVal)
        {
            if (double.TryParse(strVal, out var x))
            {
                PioneerInternalValue = x;
            }
            else
            {
                throw new PioneerReceiverException($"Unable to set Pioneer Internal Value with {strVal}");
            }
        }

    }
}
