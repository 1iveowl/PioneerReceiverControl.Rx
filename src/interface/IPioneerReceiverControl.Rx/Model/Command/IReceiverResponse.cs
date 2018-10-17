using System;
using System.Collections.Generic;
using System.Text;

namespace IPioneerReceiverControl.Rx.Model.Command
{
    public interface IReceiverResponse
    {
        string Name { get; }
        string Description { get; }
        string Value { get; }
        string Parameter { get; }
        string ResponseTemplate { get; }
    }
}
