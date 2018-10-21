using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using IPioneerReceiverControl.Rx.Model;
using PioneerReceiverControl.Rx.Model;

namespace PioneerReceiverControl.Rx.ExtensionMethod
{
    public static class ByteStreamEx
    {
        private const byte CR = 0x0D; //Carriage Return
        private const byte LF = 0x0A; //Line Feed
    
        public static IObservable<IRawResponseData> ToResponseObservable(this IObservable<byte> observableByteStream, bool ignoreMissingLineFeed = true)
        {
            return Observable.Create<IRawResponseData>(obs =>
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
                                //ignore additional CR's after first CR is received
                                break;
                            case LF when crReceived:
                                Update(false);
                                break;
                            default:
                            {
                                if (b != LF && crReceived && ignoreMissingLineFeed)
                                {
                                    Update(true);
                                }

                                break;
                            }
                        }

                        void Update(bool hasErrors)
                        {
                            var str = Encoding.UTF8.GetString(buffer.ToArray());
                            obs.OnNext(new RawResponseData(str, hasErrors));
                            buffer.Clear();
                            crReceived = false;
                        }
                    });

                return disposableByteStream;

            });
        }
    }
}
