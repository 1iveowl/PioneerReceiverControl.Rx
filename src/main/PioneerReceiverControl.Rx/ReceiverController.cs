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
using static PioneerReceiverControl.Rx.Converter.RawConverterHelper;

namespace PioneerReceiverControl.Rx
{
    public class ReceiverController : IReceiverController
    {
        private readonly TransportLayerType _transportLayerType;
        private readonly TcpClient _tcpClient;
        private readonly SerialPort _serialPort;

        private readonly IEnumerable<IReceiverCommandDefinition> _commands;

        private readonly IObservable<IRawResponseData> _listenerObservable;

        private bool _isListening;
        private bool _isConnectionClosed;

        public IObservable<IReceiverResponse> ListenerObservable
        {
            get
            {
                return Observable.Create<IReceiverResponse>(obs =>
                    {
                        _isListening = true;
                        _isConnectionClosed = false;

                        return _listenerObservable.Subscribe(
                            rawResponse =>
                            {
                                
                                var commandDefinition = _commands.FirstOrDefault(c =>
                                    ResponseParserHelper.MatchResponse(c, rawResponse) != null);

                                if (commandDefinition is null)
                                {
                                    return;
                                }

                                var matchedResponse = ResponseParserHelper.MatchResponse(commandDefinition, rawResponse);

                                if (!(matchedResponse is null))
                                {
                                    obs.OnNext(ConvertToResponse(commandDefinition, rawResponse, matchedResponse));
                                }
                            },
                            obs.OnError,
                            obs.OnCompleted);
                    })
                    .Finally(() => _isListening = false)
                    .Publish().RefCount();
            }
        }

        public IEnumerable<CommandName> KnownCommands => _commands.Select(c => c.CommandName);

        public ReceiverController(
            IEnumerable<IReceiverCommandDefinition> commands, 
            TcpClient tcpClient, 
            IPAddress ipAddress, 
            int port)
        {
            _listenerObservable = tcpClient
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
            _listenerObservable = serialPort
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
            _listenerObservable = rawDataObservable;
            _commands = commands;
        }


        public async Task<IReceiverResponse> SendReceiverCommandAndTryWaitForResponseAsync(IReceiverCommand command,
            TimeSpan timeout)
        {
            if (!_isListening && _isConnectionClosed)
            {
                throw new PioneerReceiverException("When no listener is defined, a new Receiver object have to be created for every Send Command ");
            }

            var commandDefinition = _commands.FirstOrDefault(c => c.CommandName == command.KeyValue.Key);

            var receiverResponse = new ReceiverResponse();
            
            await Observable.Create<int>(async obs =>
            {
                var disposable = _listenerObservable
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
            if (!_isListening && _isConnectionClosed)
            {
                throw new PioneerReceiverException("When no listener is defined, a new Receiver object have to be created for every Send Command ");
            }

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

            if (!_isListening)
            {
                _isConnectionClosed = true;
            }
            
        }

        public void Dispose()
        {
            _tcpClient?.Dispose();
            _serialPort?.Dispose();
        }
    }
}