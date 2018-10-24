﻿namespace IPioneerReceiverControl.Rx.Model
{
    public interface IRangeValue
    {
        int Max { get; }
        int Min { get; }
        int StepInterval { get; }
        double? NummericValue { get; }
        string StringValue { get; }
        string StringJsonValue { get; }

    }
}
