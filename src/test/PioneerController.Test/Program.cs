using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using IPioneerReceiverControl.Rx.Model;
using IPioneerReceiverControl.Rx.Model.Command;
using IPioneerReceiverControl.Rx.Model.Enum;
using PioneerReceiverControl.Rx;
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


            _commandDefinitions = new List<IReceiverCommandDefinition>
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

                //await receiverController.SendReceiverCommandAndForgetAsync(command1);

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
