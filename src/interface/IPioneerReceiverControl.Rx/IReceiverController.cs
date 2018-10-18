using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using IPioneerReceiverControl.Rx.Model.Command;

namespace IPioneerReceiverControl.Rx
{
    public interface IReceiverController
    {
        IEnumerable<IReceiverCommandDefinition> Commands { get; }

        Task<IReceiverResponse> SendReceiverCommandAndTryWaitForResponseAsync(IReceiverCommand command, TimeSpan timeout);

        Task SendReceiverCommandAndForgetAsync(IReceiverCommand command);

    }
}
