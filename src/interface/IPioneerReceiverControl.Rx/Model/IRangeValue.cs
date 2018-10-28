namespace IPioneerReceiverControl.Rx.Model
{
    public interface IRangeValue
    {
        double? PioneerInternalValue { get; }
        double? PioneerValue { get; }
        string PresentationStringValue { get; }
    }
}
