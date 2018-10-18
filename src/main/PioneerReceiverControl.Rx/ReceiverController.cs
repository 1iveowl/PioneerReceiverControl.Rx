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
using IPioneerReceiverControl.Rx.Model;
using IPioneerReceiverControl.Rx.Model.Command;
using IPioneerReceiverControl.Rx.Model.Enum;
using PioneerReceiverControl.Rx.Converter;
using PioneerReceiverControl.Rx.ExtensionMethod;
using PioneerReceiverControl.Rx.Model;
using static PioneerReceiverControl.Rx.Converter.RawConverterHelper;

namespace PioneerReceiverControl.Rx
{
    public class ReceiverController : IReceiverController
    {


        private readonly TransportLayerType _transportLayerType;
        private readonly TcpClient _tcpClient;
        private readonly SerialPort _serialPort;

        private readonly IEnumerable<IReceiverCommandDefinition> _commands;

        public IObservable<IRawResponseData> ListenerObservable { get; }

        public IEnumerable<CommandName> KnownCommands => _commands.Select(c => c.CommandName);

        public ReceiverController(
            IEnumerable<IReceiverCommandDefinition> commands, 
            TcpClient tcpClient, 
            IPAddress ipAddress, 
            int port)
        {
            ListenerObservable = tcpClient
                .ToByteStreamObservable(ipAddress, port)
                .ToResponseObservable();

            _commands = commands;

            _transportLayerType = TransportLayerType.Tcp;
            _tcpClient = tcpClient;
        }

        public ReceiverController(
            IEnumerable<IReceiverCommandDefinition> commands, 
            SerialPort serialPort, 
            int bufferSize)
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
                            var matchedResponse = ResponseParserHelper.MatchResponse(commandDefinition, rawResponse);

                            if (!(matchedResponse is null))
                            {
                                receiverResponse = ConvertToResponse(commandDefinition, rawResponse, matchedResponse);
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

        private string CreateRawCommand(IReceiverCommand command)
        {
            var commandDefinition = _commands.FirstOrDefault(d => d.CommandName == command?.KeyValue.Key);

            return ConvertToRawCommand(command, commandDefinition);
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