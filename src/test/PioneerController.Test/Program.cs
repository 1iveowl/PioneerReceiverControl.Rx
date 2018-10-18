using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Threading;
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
                    Function = "Zone 2 Power On/Off",
                    Command = CommandName.ZonePowerSwitch,
                    CommandTemplate = "AP*",
                    CommandParameterType = typeof(OnOff),
                    ResponseTemplate = "APR*",
                    ResponseParameterType = typeof(OnOff),
                },
                new ReceiverCommandDefinition
                {
                    Function = "Volume Control",
                    Command = CommandName.VolumeControl,
                    CommandTemplate = "V*",
                    CommandParameterType = typeof(UpDown),
                    ResponseTemplate = "VOL***",
                    ResponseParameterType = typeof(double)
                }
            };

            await TcpStartAsync();
            Console.ReadLine();
            _disposableResponse?.Dispose();

            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
        }

        private static async Task TcpStartAsync()
        {
            var tcpClient = new TcpClient();

            var rawDataResponseObservable = tcpClient
                .ToByteStreamObservable(_ipAddress, _port)
                .ToResponseObservable();

            var receiverController = new ReceiverController(_commandDefinitions, rawDataResponseObservable, tcpClient);

            var command = new ReceiverCommand
            {
                KeyValue = new KeyValuePair<CommandName, object>(CommandName.VolumeControl, UpDown.Down)
            };

            await receiverController.SendReceiverCommandAndForgetAsync(command);

        }

        private static async Task TcpStartRawAsync()
        {
            var tcpClient = new TcpClient();

            _disposableResponse = tcpClient
                .ToByteStreamObservable(_ipAddress, _port)
                .ToResponseObservable()
                .Subscribe(
                    m =>
                    {
                        Console.WriteLine(m.Data);
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
