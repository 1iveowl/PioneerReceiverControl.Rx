using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IPioneerReceiverControl.Rx.Model.Command;
using IPioneerReceiverControl.Rx.Model.Enum;

namespace IPioneerReceiverControl.Rx
{
    public interface IReceiverController : IDisposable
    {
        IEnumerable<CommandName> KnownCommands { get; }

        Task<IReceiverResponse> SendReceiverCommandAndTryWaitForResponseAsync(IReceiverCommand command, TimeSpan timeout);

        Task SendReceiverCommandAndForgetAsync(IReceiverCommand command);

        IObservable<IReceiverResponse> ListenerObservable { get; }

    }
}
