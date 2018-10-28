using System;
using IPioneerReceiverControl.Rx.Model;
using PioneerReceiverControl.Rx.Model.Base;

namespace PioneerReceiverControl.Rx.Model
{
    public class ZoneVolume : RangeValueBase, IZoneVolume
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

                if (val >= 0 && val <= 81)
                {
                    _pioneerInternalValue = val;
                }
                else
                {
                    throw new ArgumentOutOfRangeException($"'{val}' is not a valid number. Pioneer internal numeric volume representation must be a number between 0 and 81.");
                }
            }
        }

        public override double? PioneerValue
        {
            get => _pioneerInternalValue - 81;
            set
            {
                if (value is null)
                {
                    _pioneerInternalValue = null;
                    return;
                }

                var val = value.Value;

                if (val >= -80 && val <= 0)
                {
                    _pioneerInternalValue = val + 81;
                }
                else
                {
                    throw new ArgumentOutOfRangeException($"'{val}' is not a valid number. Pioneer db volume must be a number between -80db and 0db");
                }
            }
        }

        public ZoneVolume() { }

        public ZoneVolume(string strVal)
        {
            base.SetPioneerValueFromString(strVal);
        }
    }
}
