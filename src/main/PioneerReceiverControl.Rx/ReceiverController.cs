using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Threading.Tasks;
using IPioneerReceiverControl.Rx;
using IPioneerReceiverControl.Rx.CustomException;
using IPioneerReceiverControl.Rx.Model;
using IPioneerReceiverControl.Rx.Model.Command;
using IPioneerReceiverControl.Rx.Model.Enum;
using PioneerReceiverControl.Rx.Converter;
using PioneerReceiverControl.Rx.ExtensionMethod;
using PioneerReceiverControl.Rx.Model;

namespace PioneerReceiverControl.Rx
{
    public class ReceiverController : IReceiverController
    {
        private const string Wildcard = "*";

        private readonly TransportLayerType _transportLayerType;
        private readonly TcpClient _tcpClient;
        private readonly SerialPort _serialPort;

        private readonly IEnumerable<IReceiverCommandDefinition> _commands;

        public IObservable<IRawResponseData> ListenerObservable { get; }

        public IEnumerable<CommandName> KnownCommands => _commands.Select(c => c.CommandName);

        public ReceiverController(IEnumerable<IReceiverCommandDefinition> commands, TcpClient tcpClient, IPAddress ipAddress, int port)
        {
            ListenerObservable = tcpClient
                .ToByteStreamObservable(ipAddress, port)
                .ToResponseObservable();

            _commands = commands;

            _transportLayerType = TransportLayerType.Tcp;
            _tcpClient = tcpClient;
        }

        public ReceiverController(IEnumerable<IReceiverCommandDefinition> commands, SerialPort serialPort, int bufferSize)
        {
            ListenerObservable = serialPort
                .ToByteStreamObservable(bufferSize)
                .ToResponseObservable();

            _commands = commands;

            _transportLayerType = TransportLayerType.SerialPort;
            _serialPort = serialPort;
        }

        public ReceiverController(
            IEnumerable<IReceiverCommandDefinition> commands,
            IObservable<IRawResponseData> rawDataObservable,
            TcpClient tcpClient) : this(commands, rawDataObservable)
        {
            _transportLayerType = TransportLayerType.Tcp;
            _tcpClient = tcpClient;
        }

        public ReceiverController(
            IEnumerable<IReceiverCommandDefinition> commands,
            IObservable<IRawResponseData> rawDataObservable,
            SerialPort serialPort) : this(commands, rawDataObservable)
        {
            _transportLayerType = TransportLayerType.SerialPort;
            _serialPort = serialPort;
        }

        

        private ReceiverController(IEnumerable<IReceiverCommandDefinition> commands,
            IObservable<IRawResponseData> rawDataObservable)
        {
            ListenerObservable = rawDataObservable;
            _commands = commands;
        }


        public async Task<IReceiverResponse> SendReceiverCommandAndTryWaitForResponseAsync(IReceiverCommand command,
            TimeSpan timeout)
        {
            var commandDefinition = _commands.FirstOrDefault(c => c.CommandName == command.KeyValue.Key);

            var receiverResponse = new ReceiverResponse();
            
            await Observable.Create<int>(async obs =>
            {
                var disposable = ListenerObservable
                    .Timeout(timeout)
                    .Where(r => !(r?.Data is null))
                    .Subscribe(
                        rawResponse =>
                        {
                            Debug.WriteLine($"Response: {rawResponse?.Data}");
                            var matchedResponse = MatchResponse(commandDefinition, rawResponse);

                            if (!(matchedResponse is null))
                            {
                                receiverResponse = CreateResponse(commandDefinition, rawResponse, matchedResponse);
                                obs.OnNext(1);
                                obs.OnCompleted();
                            }
                        },
                        ex =>
                        {
                            if (ex is TimeoutException)
                            {
                                receiverResponse.WaitingForResponseTimedOut = true;
                                obs.OnNext(0);
                                obs.OnCompleted();
                            }
                            else
                            {
                                obs.OnNext(0);
                                obs.OnError(ex);
                            }
                        },
                        () =>
                        {
                            obs.OnNext(0);
                            obs.OnCompleted();
                        });

                await SendAsync(CreateRawCommand(command));

                return disposable;
            });
                

            return receiverResponse;
        }


        public async Task SendReceiverCommandAndForgetAsync(IReceiverCommand command)
        {
            var rawCommand = CreateRawCommand(command);

            await SendAsync(rawCommand);
        }

        private ReceiverResponse CreateResponse(IReceiverCommandDefinition commandDefinition, IRawResponseData data,
            string parameter)
        {
            var response = new ReceiverResponse
            {
                ResponseTime = data.TimeStamp,
                WaitingForResponseTimedOut = false,
            };

            if (commandDefinition.ResponseParameterType == typeof(OnOff))
            {
                response.ResponseValue = parameter == "0";
            }

            if (commandDefinition.ResponseParameterType == typeof(IRangeValue))
            {
                response.ResponseValue = ResponseConverter.Convert(commandDefinition.CommandName, parameter);
            }

            if (commandDefinition.ResponseParameterType == typeof(InputType))
            {
                if (int.TryParse(parameter, out var inputTypeNumber))
                {
                    response.ResponseValue = (InputType) inputTypeNumber;
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

        private string MatchResponse(IReceiverCommandDefinition commandDefinition, IRawResponseData response)
        {
            var responseWithOutWildcard = SplitNameFromParameter(commandDefinition.ResponseTemplate, response.Data);

            return responseWithOutWildcard.response == responseWithOutWildcard.template
                ? responseWithOutWildcard.parameter
                : null;
        }

        private (string template, string response, string parameter) SplitNameFromParameter(string template,
            string response)
        {
            if (response.Length != template.Length)
            {
                return (null, null, null);
            }

            var templateWithoutWildcards = template.Replace(Wildcard, string.Empty);

            var lenTemplateWithoutWildcards = templateWithoutWildcards.Length;

            var responseName = string.Join("", response.Take(lenTemplateWithoutWildcards));

            var parameter = response.Replace(templateWithoutWildcards, string.Empty);

            return (templateWithoutWildcards, responseName, parameter);
        }

        private string CreateRawCommand(IReceiverCommand command)
        {
            var commandDefinition = _commands.FirstOrDefault(d => d.CommandName == command?.KeyValue.Key);

            if (commandDefinition?.CommandTemplate is null)
            {
                throw new PioneerReceiverException($"Template is undefined for: {command.KeyValue.Key}");
            }

            if (command.KeyValue.Value is null)
            {
                return commandDefinition.CommandTemplate;
            }

            if (commandDefinition is null)
            {
                throw new PioneerReceiverException($"Unknown command: {command.KeyValue.Key}");
            }

            if (commandDefinition.CommandParameterType != command.KeyValue.Value.GetType())
            {
                throw new PioneerReceiverException($"Wrong command type: '{command.KeyValue.Value.GetType()}'. " +
                                                   $"Expected: '{commandDefinition.ResponseParameterType}'");
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

            return commandDefinition.CommandTemplate.WildcardReplace('*', parameter);
        }

        private async Task SendAsync(string rawCommand)
        {
            switch (_transportLayerType)
            {
                case TransportLayerType.Tcp:
                    await _tcpClient.SendCommandAsync(rawCommand);
                    break;
                case TransportLayerType.SerialPort:
                    _serialPort.SendCommand(rawCommand);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Dispose()
        {
            _tcpClient?.Dispose();
            _serialPort?.Dispose();
        }
    }
}