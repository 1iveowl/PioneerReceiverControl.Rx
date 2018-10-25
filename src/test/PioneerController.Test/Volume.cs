using IPioneerReceiverControl.Rx.Model;

namespace PioneerController.Test
{
    public class Volume : IVolume
    {
        public int Max { get; }
        public int Min { get; }
        public int StepInterval { get; }
        public double? NummericValue { get; internal set; }
        public string StringValue { get; }
        public string StringJsonValue { get; }
    }
}
