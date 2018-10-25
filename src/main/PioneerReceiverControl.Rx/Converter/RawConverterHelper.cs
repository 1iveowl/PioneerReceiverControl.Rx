using System.Diagnostics;
using System.Linq;
using IPioneerReceiverControl.Rx.CustomException;
using IPioneerReceiverControl.Rx.Model;
using IPioneerReceiverControl.Rx.Model.Command;
using IPioneerReceiverControl.Rx.Model.Enum;
using PioneerReceiverControl.Rx.ExtensionMethod;
using PioneerReceiverControl.Rx.Model;

namespace PioneerReceiverControl.Rx.Converter
{
    internal static class RawConverterHelper
    {
        internal static ReceiverResponse ConvertToResponse(
            IReceiverCommandDefinition commandDefinition, 
            IRawResponseData data,
            string parameter)
        {
            var response = new ReceiverResponse
            {
                ResponseTime = data.TimeStamp,
                WaitingForResponseTimedOut = false,
                ResponseToCommand = commandDefinition.CommandName.ToString()
            };

            if (commandDefinition.ResponseParameterType == typeof(OnOff))
            {
                response.ResponseValue = parameter == "0";
            }

            if (commandDefinition.ResponseParameterType == typeof(IVolume))
            {
                response.ResponseValue = ResponseConverterHelper.Convert(commandDefinition.CommandName, parameter);
            }

            if (commandDefinition.ResponseParameterType == typeof(InputType))
            {
                if (int.TryParse(parameter, out var inputTypeNumber))
                {
                    response.ResponseValue = (InputType)inputTypeNumber;
                }
                else
                {
                    response.ResponseValue = InputType.Unknown;
                }
            }

            if (commandDefinition.ResponseParameterType == typeof(ListeningMode))
            {
                if (int.TryParse(parameter, out var listeningModeNumber))
                {
                    response.ResponseValue = (ListeningMode)listeningModeNumber;
                }
                else
                {
                    response.ResponseValue = ListeningMode.Unknown;
                }
            }

            return response;
        }

        internal static string ConvertToRawCommand(IReceiverCommand command, IReceiverCommandDefinition commandDefinition)
        {
            if (commandDefinition?.CommandTemplate is null)
            {
                throw new PioneerReceiverException($"Template is undefined for: {command.KeyValue.Key}");
            }

            if (command.KeyValue.Value is null || string.IsNullOrEmpty(command.KeyValue.Value?.ToString()))
            {
                return commandDefinition.CommandTemplate;
            }

            if (commandDefinition is null)
            {
                throw new PioneerReceiverException($"Unknown command: {command.KeyValue.Key}");
            }

            if (commandDefinition.CommandParameterType.IsInterface)
            {
                if (commandDefinition.CommandParameterType != command.KeyValue.Value.GetType().GetInterfaces().FirstOrDefault())
                {
                    throw new PioneerReceiverException($"Wrong command interface type: '{command.KeyValue.Value.GetType().GetInterfaces().FirstOrDefault()}'. " +
                                                       $"Expected: '{commandDefinition.CommandParameterType}'");
                }
            }
            else
            {
                if (commandDefinition.CommandParameterType != command.KeyValue.Value.GetType())
                {
                    throw new PioneerReceiverException($"Wrong command type: '{command.KeyValue.Value.GetType()}'. " +
                                                       $"Expected: '{commandDefinition.CommandParameterType}'");
                }
            }
            
            string parameter = null;

            if (command.KeyValue.Value is UpDown direction)
            {
                parameter = direction == UpDown.Down ? "D" : "U";
            }

            if (command.KeyValue.Value is OnOff button)
            {
                parameter = button == OnOff.On ? "O" : "F";
            }

            if (command.KeyValue.Value is InputType inputType)
            {
                parameter = ((int)inputType).ToString("00");
            }

            if (command.KeyValue.Value is ListeningMode listeningMode)
            {
                parameter = ((int)listeningMode).ToString("0000");
            }

            if (command.KeyValue.Value is IVolume volume)
            {
                parameter = volume.NummericValue?.ToString("000");
            }

            if (command.KeyValue.Value is IZoneVolume zoneVolume)
            {
                parameter = zoneVolume.NummericValue?.ToString("00");
            }

            return commandDefinition.CommandTemplate.WildcardReplace('*', parameter);
        }
    }
}
