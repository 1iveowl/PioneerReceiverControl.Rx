using System;

namespace IPioneerReceiverControl.Rx.Model.Command
{
    public interface IReceiverResponse
    {
        bool IsSuccessful { get; }
        string ResponseToCommand { get; }
        object ResponseValue { get; }
        DateTime ResponseTime { get; }
        bool WaitingForResponseTimedOut { get; }

        string GetValueString();

        string GetValueJson();

    }
}
