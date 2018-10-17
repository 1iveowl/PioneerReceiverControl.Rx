using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;

namespace PioneerReceiverControl.Rx.ExtensionMethod
{
    public static class ByteStreamEx
    {
        private const byte CR = 0x0D; //Carriage Return
        private const byte LF = 0x0A; //Line Feed
    
        public static IObservable<string> ToResponseObservable(this IObservable<byte> observableByteStream, bool ignoreMissingLineFeed = true)
        {
            return Observable.Create<string>(obs =>
            {
                var buffer = new List<byte>();

                var crReceived = false;

                var disposableByteStream = observableByteStream
                    .Subscribe(b =>
                    {
                        if (b != CR && b != LF && !crReceived)
                        {
                            buffer.Add(b);
                        }
                        else switch (b)
                        {
                            case CR when !crReceived:
                                crReceived = true;
                                break;
                            case CR when crReceived:
                                //ignore addition CR after first CR is received
                                break;
                            case LF when crReceived:
                                Update();
                                break;
                            default:
                            {
                                if (b != LF && crReceived && ignoreMissingLineFeed)
                                {
                                    Update();
                                }

                                break;
                            }
                        }

                        void Update()
                        {
                            obs.OnNext(Encoding.UTF8.GetString(buffer.ToArray()));
                            buffer.Clear();
                            crReceived = false;
                        }
                    });

                return disposableByteStream;

            });
        }
    }
}
