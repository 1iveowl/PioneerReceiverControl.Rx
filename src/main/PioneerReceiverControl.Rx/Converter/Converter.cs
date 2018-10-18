using System;
using System.Collections.Generic;
using System.Text;
using IPioneerReceiverControl.Rx.CustomException;
using IPioneerReceiverControl.Rx.Model;
using IPioneerReceiverControl.Rx.Model.Enum;
using PioneerReceiverControl.Rx.Model;

namespace PioneerReceiverControl.Rx.Converter
{
    internal static class ResponseConverter
    {
        internal static IRangeValue Convert(CommandName commandName, string parameter)
        {
            switch (commandName)
            {
                case CommandName.VolumeControl:
                case CommandName.VolumeStatus:
                case CommandName.VolumeSet:

                    RangeValue rangeValue = null;

                    if (int.TryParse(parameter, out var i))
                    {
                        rangeValue = new RangeValue
                        {
                            Max = 185,
                            Min = 0,
                            StepInterval = 2,
                            NummericValue = i != 0 ? (double?)i / 2 - 80 : null,
                        };

                        rangeValue.StringValue = rangeValue.NummericValue != null
                            ? $"{rangeValue.NummericValue}db"
                            : "---.-db";
                    }

                    return rangeValue;
                case CommandName.TrebleControl:
                    return null;
                    break;
                default:
                    throw new PioneerReceiverException($"No known converter for {commandName}");
            }

        }


    }
}
