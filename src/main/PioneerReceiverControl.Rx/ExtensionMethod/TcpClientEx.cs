using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IPioneerReceiverControl.Rx.CustomException;

namespace PioneerReceiverControl.Rx.ExtensionMethod
{
    public static class TcpClientEx
    {
        public static async Task SendCommandAsync(this TcpClient tcpClient, string command, IPAddress ipAddress, int port, CancellationToken ct = default)
        {
            // Ensure that the Receiver is on before sending command
            await SendCommand(tcpClient, "\r", ipAddress, port, ct);
            await Task.Delay(TimeSpan.FromMilliseconds(100), ct);
            await SendCommand(tcpClient, "\r", ipAddress, port, ct);

            await SendCommand(tcpClient, command, ipAddress, port, ct);
        }

        private static async Task SendCommand(TcpClient tcpClient, string command, IPAddress ipAddress, int port, CancellationToken ct)
        {
            var bArray = Encoding.UTF8.GetBytes(command != "\r" ? $"{command}\r" : $"\r");

            if (!tcpClient.Connected)
            {

                // TODO Test if the client is connected to the same hos as the IPAddress and port supplied


                await tcpClient.ConnectAsync(ipAddress, port);

                if (!tcpClient.Client.Connected)
                {
                    throw new PioneerReceiverException($"Unable to connect to host: {ipAddress}:{port}");
                }
            }
            
            var writeStream = tcpClient.GetStream();

            if (writeStream?.CanWrite ?? false)
            {
                await writeStream.WriteAsync(bArray, 0, bArray.Length, ct);
            }
            else
            {
                throw new PioneerReceiverException($"Unable to get write stream from host: {ipAddress}:{port}");
            }

        }

        public static IObservable<byte> ToByteStreamObservable(this TcpClient tcpClient, IPAddress ipAddress, int port)
        {
            return Observable.Create<byte>(obs =>
                {
                    var tcpClientConnectObservable = tcpClient.Connected 
                        ? Observable.Return(tcpClient.GetStream() as Stream) 
                        : ConnectAndGetStreamAsync(tcpClient, ipAddress, port).ToObservable();

                    return tcpClientConnectObservable
                        .SelectMany(s => Observable.While(
                            () => tcpClient.Connected,
                            Observable.FromAsync(() => ReadByteArrayAsync(s))))
                        .SelectMany(a => a.ToList())
                        .Subscribe(
                            b =>
                            {
                                obs.OnNext(b);
                            },
                            ex =>
                            {
                                obs.OnError(ex);
                            },
                            () =>
                            {
                                obs.OnCompleted();
                            });
                })
            .Publish().RefCount();
        }

        private static async Task<Stream> ConnectAndGetStreamAsync(TcpClient tcpClient, IPAddress ipAddress, int port)
        {
            await tcpClient.ConnectAsync(ipAddress, port);

            return tcpClient.GetStream();
        }


        private static async Task<byte[]> ReadByteArrayAsync(Stream stream)
        {
            var oneByteArray = new byte[1];

            try
            {
                if (stream == null)
                {
                    throw new Exception("Read stream cannot be null.");
                }

                if (!stream.CanRead)
                {
                    throw new Exception("Stream connection have been closed.");
                }

                try
                {
                    var bytesRead = await stream.ReadAsync(oneByteArray, 0, 1).ConfigureAwait(false);

                    if (bytesRead < oneByteArray.Length)
                    {
                        throw new Exception("Stream connection aborted unexpectedly. Check connection and socket security version/TLS version).");
                    }
                }
                catch (IOException)
                {
                    // OK to ignore
                }
            }
            catch (ObjectDisposedException)
            {
                Debug.WriteLine("Ignoring Object Disposed Exception - This is an expected exception.");
                return Enumerable.Empty<byte>().ToArray();
            }
            catch (IOException)
            {
                return Enumerable.Empty<byte>().ToArray();
            }

            return oneByteArray;
        }
    }
}
