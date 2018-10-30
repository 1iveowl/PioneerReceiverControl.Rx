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

            var isTesting = true;

            Console.CancelKeyPress += (o, e) =>
            {
                // Clean up...
                isTesting = false;
                _disposableResponse?.Dispose();
                _receiverController?.Dispose();
                _tcpClient?.Dispose();
                Console.WriteLine("Exit");
                WaitHandle.Set();
            };

            _tcpClient = new TcpClient();
            var lingerOption = new LingerOption(true, 0);
            
            _tcpClient.LingerState = lingerOption;
            _tcpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

            _receiverController = new ReceiverController(_commandDefinitions, _tcpClient, _ipAddress, _port);
            
            while (isTesting)
            {
                await Task.Delay(TimeSpan.FromSeconds(15));

                var command1 = new ReceiverCommand
                {
                    KeyValue = new KeyValuePair<CommandName, object>(CommandName.PowerStatus, null)
                };

                try
                {
                    var result1 = await _receiverController.SendReceiverCommandAndTryWaitForResponseAsync(command1, TimeSpan.FromSeconds(10));

                    if (result1 is null)
                    {
                        Console.WriteLine($"No result");
                    }
                    else
                    {
                        if (result1.IsSuccessful)
                        {
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine(FormateNiceStringFromResponse(result1));
                            Console.WriteLine($"Thread: {Thread.CurrentThread.ManagedThreadId}");
                            Console.WriteLine($"-----------------------------");
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"Timed out: {result1.WaitingForResponseTimedOut}");
                            Console.WriteLine($"Value: {(result1.ResponseValue is null ? "null" : result1.GetValueJson())}");
                            Console.WriteLine($"Thread: {Thread.CurrentThread.ManagedThreadId}");
                            Console.WriteLine($"-----------------------------");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Exception: {ex}, inner: {ex.InnerException}");
                    Console.WriteLine($"Thread: {Thread.CurrentThread.ManagedThreadId}");
                    Console.WriteLine($"-----------------------------");
                }

                var rnd = new Random();
                var randomWait = rnd.Next(5, 8);
                await Task.Delay(TimeSpan.FromMinutes(randomWait));
            }

            ////Wait for connection
            ////await Task.Delay(TimeSpan.FromSeconds(5));

            ////// Let's send some commands

            ////// Create a command:

            
            ////_tcpClient.Client.Shutdown(SocketShutdown.Both);
            ////_tcpClient.Client.Disconnect(true);
            
            //_tcpClient.GetStream().Close();
            //_tcpClient.Dispose();

            ////await _tcpClient.Client.DisconnectAsync(true);
            ////_tcpClient.GetStream().Close();
            ////_tcpClient.Close();
            //_receiverController.Dispose();

            //await Task.Delay(TimeSpan.FromSeconds(3));

            //_tcpClient = new TcpClient();

            //_receiverController = new ReceiverController(_commandDefinitions, _tcpClient, _ipAddress, _port);

            //// Create another command:
            //var command2 = new ReceiverCommand
            //{
            //    KeyValue = new KeyValuePair<CommandName, object>(CommandName.PowerStatus, null)
            //};

            //// Send a command and listen for the receiver to respond. 
            //var result2 = await _receiverController.SendReceiverCommandAndTryWaitForResponseAsync(command2, TimeSpan.FromSeconds(10));
            //Console.WriteLine(FormateNiceStringFromResponse(result2));

            //await Task.Delay(TimeSpan.FromSeconds(1));

            ////_receiverController?.Dispose();
            ////_tcpClient?.Dispose();

            //await Task.Delay(TimeSpan.FromSeconds(3));

            ////StartTcpListener();

            //// Create another command:


            //var command3 = new ReceiverCommand
            //{
            //    KeyValue = new KeyValuePair<CommandName, object>(CommandName.VolumeSet, new Volume{PioneerInternalValue = 101})
            //};

            //// Send a command and listen for the receiver to respond. 
            //var result3 = await _receiverController.SendReceiverCommandAndTryWaitForResponseAsync(command3, TimeSpan.FromSeconds(10));
            //Console.WriteLine(FormateNiceStringFromResponse(result3));


            //await Task.Delay(TimeSpan.FromSeconds(15));

            //Console.WriteLine("Wait one");

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
