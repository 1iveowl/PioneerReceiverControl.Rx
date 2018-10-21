using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PioneerReceiverControl.Rx.ExtensionMethod
{
    public static class SerialEx
    {
        public static void SendCommand(this SerialPort serialPort, string command)
        {
            serialPort.WriteLine(command);
        }

        public static IObservable<byte> ToByteStreamObservable(this SerialPort serialPort, int bufferSize)
        {
            return Observable.Create<byte>(obs =>
            {
                var disposableSerialPort = Observable
                    .FromEventPattern<SerialDataReceivedEventHandler, SerialDataReceivedEventArgs>(
                        handler => serialPort.DataReceived += handler,
                        handler => serialPort.DataReceived -= handler)
                    .SelectMany(e =>
                    {
                        var buffer = new byte[bufferSize];
                        var result = new List<byte>();

                        var isDoneReadingBytes = false;

                        while (!isDoneReadingBytes)
                        {
                            var bytesRead = serialPort.Read(buffer, 0, buffer.Length);
                            result.AddRange(buffer.Take(bytesRead));

                            isDoneReadingBytes = bytesRead <= buffer.Length;
                        }

                        return result;

                    })
                    .Finally(serialPort.Close)
                    .Subscribe(
                        obs.OnNext,
                        obs.OnError,
                        obs.OnCompleted);

                serialPort.Open();

                return disposableSerialPort;
            })
            .Publish().RefCount();
        }
    }
}
