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
using PioneerReceiverControl.Rx.ExtensionMethod;
using PioneerReceiverControl.Rx.Model;

namespace PioneerController.Test
{
    public class Program
    {
        private static IDisposable _disposableResponse;

        private static IReceiver _receiver;

        private static async Task Main(string[] args)
        {

            _receiver = new Receiver
            {
                Commands = new List<IReceiverCommandDefinition>
                {
                    new ReceiverCommandDefinition
                    {
                        Function = "Zone 2 Power On/Off",
                        Name = "zone2PowerSwitch",
                        CommandTemplate = "AP*",
                        CommandParameterType = typeof(OnOff),
                        ResponseTemplate = "APR*",
                        ResponseParameterType = typeof(OnOff),
                    },
                },
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

            var ipaddress = IPAddress.Parse("192.168.0.24");

            _disposableResponse = tcpClient
                .ToByteStreamObservable(ipaddress, 23)
                .ToResponseObservable()
                .Subscribe(
                    m =>
                    {
                        Console.WriteLine(m);
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
