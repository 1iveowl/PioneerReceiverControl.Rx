﻿using System;
using System.Collections.Generic;
using System.Text;

namespace IPioneerReceiverControl.Rx.Model.Command
{
    public interface IReceiverResponse
    {
        string ResponseToCommand { get; }
        object ResponseValue { get; }
        DateTime ResponseTime { get; }
        bool WaitingForResponseTimedOut { get; }

        string GetValueString();

        string GetValueJson();

    }
}
