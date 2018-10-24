using IPioneerReceiverControl.Rx.CustomException;
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

                    RangeValue rangeValueVolume = null;

                    if (int.TryParse(parameter, out var vol))
                    {
                        rangeValueVolume = new RangeValue
                        {
                            Max = 185,
                            Min = 0,
                            StepInterval = 2,
                            NummericValue = vol != 0 ? (double?)vol / 2 - 80 : null,
                        };

                        rangeValueVolume.StringValue = rangeValueVolume.NummericValue != null
                            ? $"{rangeValueVolume.NummericValue}db"
                            : "---.-db";
                    }

                    return rangeValueVolume;

                case CommandName.Zone2VolumeControl:
                case CommandName.Zone2VolumeStatus:
                case CommandName.Zone2VolumeSet:

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

                case CommandName.TrebleControl:
                    return null;
                default:
                    throw new PioneerReceiverException($"No known converter for {commandName}");
            }

        }
    }
}
