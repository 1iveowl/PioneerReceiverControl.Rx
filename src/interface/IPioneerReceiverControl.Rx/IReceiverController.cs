using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using IPioneerReceiverControl.Rx.Model;
using IPioneerReceiverControl.Rx.Model.Command;
using IPioneerReceiverControl.Rx.Model.Enum;

namespace IPioneerReceiverControl.Rx
{
    public interface IReceiverController : IDisposable
    {
        IEnumerable<CommandName> KnownCommands { get; }

        Task<IReceiverResponse> SendReceiverCommandAndTryWaitForResponseAsync(IReceiverCommand command, TimeSpan timeout);

        Task SendReceiverCommandAndForgetAsync(IReceiverCommand command);

        IObservable<IRawResponseData> ListenerObservable { get; }

    }
}
