using System;
using System.Collections.Generic;
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

namespace PioneerReceiverControl.Rx.ExtensionMethod
{
    public static class TcpClientEx
    {
        private static CancellationTokenSource _cts;

        public static async Task SendCommandAsync(this TcpClient tcpClient, string command, CancellationToken ct = default)
        {
            // Ensure that the Receiver is on before sending command
            await SendCommand(tcpClient, "\r", ct);
            await Task.Delay(TimeSpan.FromMilliseconds(100), ct);
            await SendCommand(tcpClient, "\r", ct);

            await SendCommand(tcpClient, command, ct);
        }

        private static async Task SendCommand(TcpClient tcpClient, string command, CancellationToken ct)
        {
            var bArray = Encoding.UTF8.GetBytes($"{command}\r");

            var writeStream = tcpClient.GetStream();

            if (writeStream?.CanWrite ?? false)
            {
                await writeStream.WriteAsync(bArray, 0, bArray.Length, ct);
            }
        }

        public static IObservable<byte> ToByteStreamObservable(this TcpClient tcpClient, IPAddress ipAddress, int port)
        {
            _cts = new CancellationTokenSource();

            return Observable.Create<byte>(obs =>
            {
                var tcpClientConnectObservable = ConnectAndGetStreamAsync(tcpClient, ipAddress, port).ToObservable();
                
                return tcpClientConnectObservable
                    .SelectMany(s => Observable.While(
                        () => !_cts.Token.IsCancellationRequested,
                        Observable.FromAsync(() => ReadByteArrayAsync(s, _cts.Token))))
                    .SelectMany(a => a.ToList())
                    .Finally(() =>
                    {
                        _cts?.Cancel();
                        tcpClient?.Close();
                        _cts?.Dispose();
                    })
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


        private static async Task<byte[]> ReadByteArrayAsync(Stream stream, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
            {
                return Enumerable.Empty<byte>().ToArray();
            }

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
                    var bytesRead = await stream.ReadAsync(oneByteArray, 0, 1, ct).ConfigureAwait(false);

                    if (bytesRead < oneByteArray.Length)
                    {
                        throw new Exception("Stream connection aborted unexpectedly. Check connection and socket security version/TLS version).");
                    }
                }
                catch (IOException ex)
                {
                    if (_cts?.IsCancellationRequested ?? true)
                    {
                        Debug.WriteLine($"OK to ignore this exception when closing connection | '{ex.Message}'");

                        return Enumerable.Empty<byte>().ToArray();
                    }

                    throw ex;
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
