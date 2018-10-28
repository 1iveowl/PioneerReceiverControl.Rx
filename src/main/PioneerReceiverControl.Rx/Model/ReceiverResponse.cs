using System;
using IPioneerReceiverControl.Rx.Model;
using IPioneerReceiverControl.Rx.Model.Command;
using Newtonsoft.Json;

namespace PioneerReceiverControl.Rx.Model
{
    public class ReceiverResponse : IReceiverResponse
    {
        public bool IsSuccessful { get; internal set; }
        public string ResponseToCommand { get; internal set; }
        public object ResponseValue { get; internal set; }
        public DateTime ResponseTime { get; internal set; }
        public bool WaitingForResponseTimedOut { get; internal set; }
        public string RawResponse { get; set; }

        public string GetValueString()
        {
            if (ResponseValue is IRangeValue rv)
            {
                return rv.PresentationStringValue;
            }
            else
            {
                return ResponseValue?.ToString();
            }
        }

        public string GetValueJson()
        {
            string parameterValue;

            if (ResponseValue is IRangeValue rv)
            {
                parameterValue = rv.PioneerValue.ToString();
            }
            else
            {
                parameterValue= ResponseValue?.ToString();
            }

            var responseObj = new ReceiveResponseObject
            {
                Command = ResponseToCommand,
                Parameter = parameterValue,
                Timestamp = ResponseTime,
                RawResponse = RawResponse
            };

            return JsonConvert.SerializeObject(responseObj);
        }
    }
}
