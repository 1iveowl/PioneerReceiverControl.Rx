using System;
using System.Collections.Generic;
using System.Text;
using IPioneerReceiverControl.Rx.Model;
using IPioneerReceiverControl.Rx.Model.Command;
using IPioneerReceiverControl.Rx.Model.Enum;
using PioneerReceiverControl.Rx.Model;

namespace PioneerReceiverControl.Rx.Data
{
    public class DefaultReceiverCommandDefinition
    {
        public IEnumerable<IReceiverCommandDefinition> GetDefaultDefinitions =>
            new List<IReceiverCommandDefinition>
            {
                new ReceiverCommandDefinition
                {
                    Function = "Power On/Off",
                    CommandName = CommandName.PowerSwitch,
                    CommandTemplate = "P*",
                    CommandParameterType = typeof(OnOff),
                    ResponseTemplate = "PWR*",
                    ResponseParameterType = typeof(OnOff),
                },
                new ReceiverCommandDefinition
                {
                    Function = "Power Status",
                    CommandName = CommandName.PowerStatus,
                    CommandTemplate = "?*",
                    CommandParameterType = null,
                    ResponseTemplate = "PWR*",
                    ResponseParameterType = typeof(OnOff),
                },
                new ReceiverCommandDefinition
                {
                    Function = "Volume Control",
                    CommandName = CommandName.VolumeControl,
                    CommandTemplate = "V*",
                    CommandParameterType = typeof(UpDown),
                    ResponseTemplate = "VOL***",
                    ResponseParameterType = typeof(IRangeValue)
                },
                new ReceiverCommandDefinition
                {
                    Function = "Volume Set",
                    CommandName = CommandName.VolumeSet,
                    CommandTemplate = "***VL",
                    CommandParameterType = typeof(UpDown),
                    ResponseTemplate = "VOL***",
                    ResponseParameterType = typeof(IRangeValue)
                },
                new ReceiverCommandDefinition
                {
                    Function = "Volume Status",
                    CommandName = CommandName.VolumeStatus,
                    CommandTemplate = "?V",
                    CommandParameterType = null,
                    ResponseTemplate = "VOL***",
                    ResponseParameterType = typeof(IRangeValue)
                },
                new ReceiverCommandDefinition
                {
                    Function = "Mute",
                    CommandName = CommandName.MuteSwitch,
                    CommandTemplate = "M*",
                    CommandParameterType = typeof(OnOff),
                    ResponseTemplate = "MUT*",
                    ResponseParameterType = typeof(OnOff),
                },
                new ReceiverCommandDefinition
                {
                    Function = "Mute Status",
                    CommandName = CommandName.MuteStatus,
                    CommandTemplate = "?M",
                    CommandParameterType = null,
                    ResponseTemplate = "MUT*",
                    ResponseParameterType = typeof(OnOff),
                },
                new ReceiverCommandDefinition
                {
                    Function = "Input Set",
                    CommandName = CommandName.InputSet,
                    CommandTemplate = "**FN",
                    CommandParameterType = typeof(InputType),
                    ResponseTemplate = "FN**",
                    ResponseParameterType = typeof(InputType)
                },
                new ReceiverCommandDefinition
                {
                    Function = "Input Status",
                    CommandName = CommandName.InputSet,
                    CommandTemplate = "?F",
                    CommandParameterType = null,
                    ResponseTemplate = "FN**",
                    ResponseParameterType = typeof(InputType)
                },
                new ReceiverCommandDefinition
                {
                    Function = "Zone 2 Power On/Off",
                    CommandName = CommandName.Zone2PowerSwitch,
                    CommandTemplate = "AP*",
                    CommandParameterType = typeof(OnOff),
                    ResponseTemplate = "APR*",
                    ResponseParameterType = typeof(OnOff),
                },
                new ReceiverCommandDefinition
                {
                    Function = "Zone 2 Volume Status",
                    CommandName = CommandName.Zone2VolumeStatus,
                    CommandTemplate = "?ZV",
                    CommandParameterType = null,
                    ResponseTemplate = "ZV**",
                    ResponseParameterType = typeof(IRangeValue)
                },
                new ReceiverCommandDefinition
                {
                    Function = "Zone 2 Volume Control",
                    CommandName = CommandName.Zone2VolumeControl,
                    CommandTemplate = "Z*",
                    CommandParameterType = typeof(UpDown),
                    ResponseTemplate = "ZV**",
                    ResponseParameterType = typeof(IRangeValue)
                },
                new ReceiverCommandDefinition
                {
                    Function = "Zone 2 Input Set",
                    CommandName = CommandName.Zone2InputSet,
                    CommandTemplate = "**ZS",
                    CommandParameterType = typeof(InputType),
                    ResponseTemplate = "Z2F**",
                    ResponseParameterType = typeof(InputType)
                },
                new ReceiverCommandDefinition
                {
                    Function = "Zone 2 Input Status",
                    CommandName = CommandName.Zone2InputStatus,
                    CommandTemplate = "?ZS",
                    CommandParameterType = null,
                    ResponseTemplate = "Z2F**",
                    ResponseParameterType = typeof(InputType)
                },
                new ReceiverCommandDefinition
                {
                    Function = "Listening Mode Set",
                    CommandName = CommandName.ListeningModeSet,
                    CommandTemplate = "****SR",
                    CommandParameterType = typeof(ListeningMode),
                    ResponseTemplate = "SR****",
                    ResponseParameterType = typeof(ListeningMode)
                },
                new ReceiverCommandDefinition
                {
                    Function = "Listening Mode Status",
                    CommandName = CommandName.ListeningModeStatus,
                    CommandTemplate = "?S",
                    CommandParameterType = null,
                    ResponseTemplate = "SR****",
                    ResponseParameterType = typeof(ListeningMode)
                },
            };
    }
}
