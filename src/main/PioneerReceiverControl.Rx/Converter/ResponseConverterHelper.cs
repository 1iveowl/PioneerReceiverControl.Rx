﻿using IPioneerReceiverControl.Rx.CustomException;
using IPioneerReceiverControl.Rx.Model;
using IPioneerReceiverControl.Rx.Model.Enum;
using PioneerReceiverControl.Rx.Model;

namespace PioneerReceiverControl.Rx.Converter
{
    internal static class ResponseConverterHelper
    {
        internal static IRangeValue Convert(CommandName commandName, string parameter)
        {
            switch (commandName)
            {
                case CommandName.VolumeControl:
                case CommandName.VolumeStatus:
                case CommandName.VolumeSet:
                case CommandName.Zone2VolumeControl:
                case CommandName.Zone2VolumeStatus:
                case CommandName.Zone2VolumeSet:
                    return GetVolumeRangeValue(parameter);
                case CommandName.TrebleControl:
                    return null;
                default:
                    throw new PioneerReceiverException($"No known converter for {commandName}");
            }
        }

        private static RangeValue GetVolumeRangeValue(string parameter)
        {
            RangeValue rangeValueZoneVolume = null;

            if (int.TryParse(parameter, out var z2vol))
            {
                rangeValueZoneVolume = new RangeValue
                {
                    Max = 185,
                    Min = 0,
                    StepInterval = 1,
                    NummericValue = z2vol != 0 ? (double?)z2vol - 80 : null,
                };

                rangeValueZoneVolume.StringValue = rangeValueZoneVolume.NummericValue != null
                    ? $"{rangeValueZoneVolume.NummericValue}db"
                    : "---.-db";
            }

            return rangeValueZoneVolume;
        }
    }
}
