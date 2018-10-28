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

                    var vol = new Volume();
                    vol.SetPioneerInternalValueFromString(parameter);
                    return vol;

                case CommandName.Zone2VolumeControl:
                case CommandName.Zone2VolumeStatus:
                case CommandName.Zone2VolumeSet:

                    var zonVol = new ZoneVolume();
                    zonVol.SetPioneerInternalValueFromString(parameter);
                    return zonVol;

                case CommandName.TrebleControl:

                    // TODO - not yet defined
                    return null;

                default:
                    throw new PioneerReceiverException($"No known converter for {commandName}");
            }
        }
    }
}
