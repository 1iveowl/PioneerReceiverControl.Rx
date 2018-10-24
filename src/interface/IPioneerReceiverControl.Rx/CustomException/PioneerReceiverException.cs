using System;

namespace IPioneerReceiverControl.Rx.CustomException
{
    public class PioneerReceiverException : Exception
    {
        public PioneerReceiverException() { }

        public PioneerReceiverException(string message) : base(message) { }

        public PioneerReceiverException(string message, Exception inner) : base(message, inner) { }
    }
}
