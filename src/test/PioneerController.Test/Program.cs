using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Threading;
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
        // Needed for alternative to Console.ReadLine();
        private static readonly AutoResetEvent WaitHandle = new AutoResetEvent(false);

        private static IDisposable _disposableResponse;
        private static IDisposable _disposableReceiverController;
        private static ReceiverController _receiverController;

        private static IEnumerable<IReceiverCommandDefinition> _commandDefinitions;

        private static TcpClient _tcpClient;
        private static IPAddress _ipAddress;
        private static int _port;
  

        private static async Task Main(string[] args)
        {
            _ipAddress = IPAddress.Parse("192.168.0.24");
            _port = 8102;

            _commandDefinitions = new DefaultReceiverCommandDefinition().GetDefaultDefinitions;

            // Run this when the user presses the ctrl-C key - alternative to Console.ReadLine();
            Console.CancelKeyPress += (o, e) =>
            {
                // Clean up...
                _disposableResponse?.Dispose();
                _receiverController?.Dispose();
                _tcpClient?.Dispose();
                Console.WriteLine("Exit");
                WaitHandle.Set();
            };

            // Start the TCP Listener.
            //StartTcpListener();

            _tcpClient = new TcpClient();

            _receiverController = new ReceiverController(_commandDefinitions, _tcpClient, _ipAddress, _port);

            //Wait for connection
            //await Task.Delay(TimeSpan.FromSeconds(5));

            //// Let's send some commands

            //// Create a command:
            var command1 = new ReceiverCommand
            {
                KeyValue = new KeyValuePair<CommandName, object>(CommandName.PowerSwitch, OnOff.On)
            };

            // Let's send the command and forget about it.
            await _receiverController.SendReceiverCommandAndForgetAsync(command1);

            await Task.Delay(TimeSpan.FromSeconds(3));

            // Create another command:
            var command2 = new ReceiverCommand
            {
                KeyValue = new KeyValuePair<CommandName, object>(CommandName.VolumeStatus, string.Empty)
            };

            // Send a command and listen for the receiver to respond. 
            var result2 = await _receiverController.SendReceiverCommandAndTryWaitForResponseAsync(command2, TimeSpan.FromSeconds(2));
            Console.WriteLine(FormateNiceStringFromResponse(result2));

            await Task.Delay(TimeSpan.FromSeconds(1));

            //_receiverController?.Dispose();
            //_tcpClient?.Dispose();

            await Task.Delay(TimeSpan.FromSeconds(3));

            //StartTcpListener();

            // Create another command:


            var command3 = new ReceiverCommand
            {
                KeyValue = new KeyValuePair<CommandName, object>(CommandName.VolumeSet, new Volume{NummericValue = 101} as IVolume)
            };

            // Send a command and listen for the receiver to respond. 
            var result3 = await _receiverController.SendReceiverCommandAndTryWaitForResponseAsync(command3, TimeSpan.FromSeconds(2));
            Console.WriteLine(FormateNiceStringFromResponse(result3));


            await Task.Delay(TimeSpan.FromSeconds(15));

            Console.WriteLine("Wait one");

            // Wait here until the user presses the ctrl-C key - alternative to Console.ReadLine();
            WaitHandle.WaitOne();

            _receiverController?.Dispose();
            _tcpClient?.Close();

            Console.WriteLine("...End...");
            Console.ReadLine();

        }

        private static async Task TcpSendOneCommandAndDisconnectAsync()
        {
            using (var tcpClient = new TcpClient())
            using (var receiverController = new ReceiverController(_commandDefinitions, _tcpClient, _ipAddress, _port))
            {
                await Task.Delay(TimeSpan.FromSeconds(5));

                var command1 = new ReceiverCommand
                {
                    KeyValue = new KeyValuePair<CommandName, object>(CommandName.Zone2InputStatus, null)
                };

                var result1 = await _receiverController.SendReceiverCommandAndTryWaitForResponseAsync(command1, TimeSpan.FromSeconds(2));

                Console.WriteLine(FormateNiceStringFromResponse(result1));
            }
        }

        private static void StartTcpListener()
        {
            _tcpClient = new TcpClient();

            _receiverController = new ReceiverController(_commandDefinitions, _tcpClient, _ipAddress, _port);

            Debug.WriteLine($"Thread - Start: {Thread.CurrentThread.ManagedThreadId}");

            //_disposableReceiverController = _receiverController.ListenerObservable
            //    .Do(_ => Debug.WriteLine($"Thread - Response Main Observer: {Thread.CurrentThread.ManagedThreadId}"))
            //    .Subscribe(
            //        res =>
            //        {
            //            Debug.WriteLine($"Thread - Response Main Subscription: {Thread.CurrentThread.ManagedThreadId}");
            //            Console.WriteLine(FormateNiceStringFromResponse(res));
            //        },
            //        ex =>
            //        {
            //            Console.WriteLine(ex);
            //        },
            //        () =>
            //        {
            //            Console.WriteLine("Completed.");
            //        });

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
            await tcpClient.SendCommandAsync("?AP", _ipAddress, _port);
            Console.WriteLine("Zone 2 Volume?");
            await tcpClient.SendCommandAsync("?ZV", _ipAddress, _port);

            await Task.Delay(TimeSpan.FromSeconds(1));

            await Task.CompletedTask;
        }

        private static string FormateNiceStringFromResponse(IReceiverResponse response)
        {
            return $"Command: {response.ResponseToCommand}, " +
                    $"Value: {response.GetValueString()}, " +
                    $"Timed Out: {response.WaitingForResponseTimedOut}, " +
                    $"Time: {response.ResponseTime} \r\n" +
                    $"JSON: {response.GetValueJson()} \r\n";
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
