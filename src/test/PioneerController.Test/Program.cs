using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using IPioneerReceiverControl.Rx.Model;
using IPioneerReceiverControl.Rx.Model.Command;
using IPioneerReceiverControl.Rx.Model.Enum;
using PioneerReceiverControl.Rx;
using PioneerReceiverControl.Rx.Data;
using PioneerReceiverControl.Rx.ExtensionMethod;
using PioneerReceiverControl.Rx.Model;

namespace PioneerController.Test
{
    public class Program
    {
        private static IDisposable _disposableResponse;

        private static IEnumerable<IReceiverCommandDefinition> _commandDefinitions;

        private static IPAddress _ipAddress;
        private static int _port;

        private static async Task Main(string[] args)
        {
            _ipAddress = IPAddress.Parse("192.168.0.24");
            _port = 23;

            _commandDefinitions = new DefaultReceiverCommandDefinition().GetDefaultDefinitions;

            await TcpStartListenerAsync();
            Console.ReadLine();
            _disposableResponse?.Dispose();

            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
        }

        private static async Task TcpSentCommandAndDisconnectAsync()
        {
            using (var tcpClient = new TcpClient())
            using (var receiverController = new ReceiverController(_commandDefinitions, tcpClient, _ipAddress, _port))
            {
                await Task.Delay(TimeSpan.FromSeconds(5));

                var command1 = new ReceiverCommand
                {
                    KeyValue = new KeyValuePair<CommandName, object>(CommandName.VolumeControl, UpDown.Up)
                };

                var result1 = await receiverController.SendReceiverCommandAndTryWaitForResponseAsync(command1, TimeSpan.FromSeconds(2));

                Console.WriteLine($"Value: {((IRangeValue)result1.ResponseValue).StringValue}, " +
                                  $"Timed Out: {result1.WaitingForResponseTimedOut}, " +
                                  $"Time: {result1.ResponseTime}");
            }
        }

        private static async Task TcpStartListenerAsync()
        {
            using (var tcpClient = new TcpClient())
            using (var receiverController = new ReceiverController(_commandDefinitions, tcpClient, _ipAddress, _port))
            {
               
                var disposableReceiverController = receiverController.ListenerObservable
                    .Subscribe(
                        res =>
                        {
                            Console.WriteLine(res.Data);
                        },
                        ex =>
                        {
                            Console.WriteLine(ex);
                        },
                        () =>
                        {
                            Console.WriteLine("Completed.");
                        });

                var command1 = new ReceiverCommand
                {
                    KeyValue = new KeyValuePair<CommandName, object>(CommandName.VolumeControl, UpDown.Up)
                };

                await Task.Delay(TimeSpan.FromSeconds(5));

                var result1 = await receiverController.SendReceiverCommandAndTryWaitForResponseAsync(command1, TimeSpan.FromSeconds(2));
                Console.WriteLine($"Value: {((IRangeValue)result1?.ResponseValue)?.StringValue}, " +
                                  $"Timed Out: {result1?.WaitingForResponseTimedOut}, " +
                                  $"Time: {result1?.ResponseTime}");

                var command2 = new ReceiverCommand
                {
                    KeyValue = new KeyValuePair<CommandName, object>(CommandName.VolumeStatus, null)
                };

                await Task.Delay(TimeSpan.FromSeconds(2));

                var result2 = await receiverController.SendReceiverCommandAndTryWaitForResponseAsync(command2, TimeSpan.FromSeconds(2));
                Console.WriteLine($"Value: {((IRangeValue)result2.ResponseValue).StringValue}, " +
                                  $"Timed Out: {result2.WaitingForResponseTimedOut}, " +
                                  $"Time: {result2.ResponseTime}");

                await Task.Delay(TimeSpan.FromSeconds(60));

                disposableReceiverController?.Dispose();
            }
        }

        private static async Task TcpStartRawAsync()
        {
            var tcpClient = new TcpClient();

            _disposableResponse = tcpClient
                .ToByteStreamObservable(_ipAddress, _port)
                .ToResponseObservable()
                .Subscribe(
                    res =>
                    {
                        Console.WriteLine(res.Data);
                    },
                    ex =>
                    {
                        Console.WriteLine(ex);
                    },
                    () =>
                    {
                        Console.WriteLine("Completed.");
                    });

            await Task.Delay(TimeSpan.FromSeconds(5));

            Console.WriteLine("Is Zone 2 on?");
            await tcpClient.SendCommandAsync("?AP");
            Console.WriteLine("Zone 2 Volume?");
            await tcpClient.SendCommandAsync("?ZV");

            await Task.Delay(TimeSpan.FromSeconds(1));

            await Task.CompletedTask;
        }

        private static async Task SerialStartAsync()
        {
            var serialPort = new SerialPort
            {
                PortName = "COM1",
                Parity = Parity.None,
                BaudRate = 9600,
                DataBits = 8,
                StopBits = StopBits.One
            };

            serialPort.SendCommand("");

        }
    }
}
