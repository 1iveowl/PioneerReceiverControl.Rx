using System;
using IPioneerReceiverControl.Rx.Model;
using PioneerReceiverControl.Rx.Model.Base;

namespace PioneerReceiverControl.Rx.Model
{
    public class Volume : RangeValueBase, IVolume
    {
        private double? _pioneerInternalValue;

        public override double? PioneerInternalValue
        {
            get => _pioneerInternalValue;
            set
            {
                if (value is null)
                {
                    _pioneerInternalValue = null;
                    return;
                }

                var val = value.Value;

                if (val >= 0 && val <= 185)
                {
                    _pioneerInternalValue = val;
                }
                else
                {
                    throw new ArgumentOutOfRangeException($"'{val}' is not a valid number. Pioneer internal numeric volume representation must be a number between 0 and 185.");
                }
            }
        }

        public override double? PioneerValue
        {
            get => _pioneerInternalValue - 1 / 2 - 80;
            set
            {
                if (value is null)
                {
                    _pioneerInternalValue = null;
                    return;
                }

                var val = value.Value;

                if (val >= -80 && val <= 12)
                {
                    _pioneerInternalValue = (val + 80) * 2 + 1;
                }
                else
                {
                    throw new ArgumentOutOfRangeException($"'{val}' is not a valid number. Pioneer db volume must be a number between -80db and 12db");
                }
            }
        }

        public Volume() { }

        public Volume(string strVal)
        {
            base.SetPioneerValueFromString(strVal);
        }
    }
}
