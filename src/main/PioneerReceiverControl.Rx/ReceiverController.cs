using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
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

        private readonly IPAddress _ipAddress;
        private readonly int _port;
        private readonly IDisposable _disposableKeepalive;

        private readonly IEnumerable<IReceiverCommandDefinition> _commands;

        private readonly IObservable<IRawResponseData> _listenerObservable;
     

        public IObservable<IReceiverResponse> ListenerObservable
        {
            get
            {
                return Observable.Create<IReceiverResponse>(obs =>
                    {
                        return _listenerObservable
                            .Do(_ => Debug.WriteLine($"Thread - Listener Observe: {Thread.CurrentThread.ManagedThreadId}"))
                            .Subscribe(
                            rawResponse =>
                            {
                                Debug.WriteLine($"Thread - Listen for Subscription: {Thread.CurrentThread.ManagedThreadId}");
                                var commandDefinition = _commands.FirstOrDefault(c =>
                                    ResponseParserHelper.MatchResponse(c, rawResponse) != null);

                                if (commandDefinition is null)
                                {
                                    obs.OnNext(new ReceiverResponse
                                    {
                                        ResponseValue = $"Unknown: {rawResponse.Data}",
                                        ResponseTime = DateTime.Now,
                                        ResponseToCommand = "Unknown",
                                        WaitingForResponseTimedOut = false,
                                        RawResponse = rawResponse.Data
                                    });

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
                    .Publish().RefCount();
            }
        }

        public IEnumerable<CommandName> KnownCommands => _commands.Select(c => c.CommandName);

        public ReceiverController(
            IEnumerable<IReceiverCommandDefinition> commands, 
            TcpClient tcpClient, 
            IPAddress ipAddress, 
            int port,
            TimeSpan keepAlive = default)
        {
            _ipAddress = ipAddress;
            _port = port;

            _listenerObservable = tcpClient
                .ToByteStreamObservable(ipAddress, port)
                .ToResponseObservable()
                .Where(r => r.Data != "R");

            _commands = commands;

            _transportLayerType = TransportLayerType.Tcp;
            _tcpClient = tcpClient;

            if (keepAlive == default)
            {
                keepAlive = TimeSpan.FromSeconds(10);
            }

            _disposableKeepalive = Observable.Interval(keepAlive)
                .Select(_ => Observable.FromAsync(ct => _tcpClient.SendPingAsync(_ipAddress, port, ct)))
                .Concat()
                .Subscribe(_ =>
                    {
                        Debug.WriteLine($"Ping send");
                    },
                    ex =>
                    {
                        Debug.WriteLine($"Ping failed: {ex}");
                    },
                    () =>
                    {
                        Debug.WriteLine($"Ping ended");
                    });
        }

        public ReceiverController(
            IEnumerable<IReceiverCommandDefinition> commands, 
            SerialPort serialPort, 
            int bufferSize)
        {
            _listenerObservable = serialPort
                .ToByteStreamObservable(bufferSize)
                .ToResponseObservable()
                .Where(r => r.Data != "R");

            _commands = commands;

            _transportLayerType = TransportLayerType.SerialPort;
            _serialPort = serialPort;
        }

        public ReceiverController(
            IEnumerable<IReceiverCommandDefinition> commands,
            IObservable<IRawResponseData> rawDataObservable,
            TcpClient tcpClient) : this(commands, rawDataObservable)
        {
            _ipAddress = ((IPEndPoint) tcpClient.Client.RemoteEndPoint).Address;
            _port = ((IPEndPoint) tcpClient.Client.RemoteEndPoint).Port;

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
            _listenerObservable = rawDataObservable.Where(r => r.Data != "R");
            _commands = commands;
        }


        public async Task<IReceiverResponse> SendReceiverCommandAndTryWaitForResponseAsync(
            IReceiverCommand command,
            TimeSpan timeout)
        {
            var commandDefinition = _commands.FirstOrDefault(c => c.CommandName == command.KeyValue.Key);

            var observableSendResponse = Observable.Create<ReceiverResponse>(obs =>
            {
                var receiverResponse = new ReceiverResponse();

                var disposableListener = _listenerObservable
                    .Do(_ => Debug.WriteLine($"A Thread - Listen for Response Observe: {Thread.CurrentThread.ManagedThreadId}"))
                    .Timeout(timeout)
                    //.ObserveOn(Scheduler.CurrentThread)
                    .Where(r => !(r?.Data is null))
                    .Subscribe(
                        rawResponse =>
                        {
                            Debug.WriteLine($"Response: {rawResponse?.Data}");
                            Debug.WriteLine($"Thread - Listen for Response Subscription: {Thread.CurrentThread.ManagedThreadId}");
                            var matchedResponse = ResponseParserHelper.MatchResponse(commandDefinition, rawResponse);

                            if (!(matchedResponse is null))
                            {
                                try
                                {
                                    receiverResponse = ConvertToResponse(commandDefinition, rawResponse, matchedResponse);
                                }
                                catch (Exception) { }
                                finally
                                {
                                    obs.OnNext(receiverResponse);
                                    obs.OnCompleted();
                                }
                            }
                        },
                        ex =>
                        {
                            if (ex is TimeoutException)
                            {
                                receiverResponse.WaitingForResponseTimedOut = true;
                                obs.OnNext(null);
                                obs.OnCompleted();
                            }
                            else
                            {
                                obs.OnNext(null);
                                obs.OnCompleted();
                            }
                        },
                        () =>
                        {
                            obs.OnNext(null);
                            obs.OnCompleted();
                        });

                var disposableSend = SendAsync(CreateRawCommand(command))
                    .ToObservable()
                    .Subscribe(
                        _ =>
                        {
                            //obs.OnNext(null);
                        },
                        ex =>
                        {
                            obs.OnError(ex);
                        },
                        () =>
                        {
                            //obs.OnNext(0);
                        });

                return new CompositeDisposable(disposableListener, disposableSend);
            });

            try
            {
                var response = await observableSendResponse; //.ObserveOn(Scheduler.CurrentThread);
                return response;
            }
            catch (Exception)
            {
                return null;
            }
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
                    await _tcpClient.SendCommandAsync(rawCommand, _ipAddress, _port);
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
            _disposableKeepalive?.Dispose();
            //_tcpClient?.Dispose();
            //_serialPort?.Dispose();
        }
    }
}