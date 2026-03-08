namespace ERPBackend.Core.Enums
{
    public enum SampleType
    {
        DevelopmentSample = 1,
        FitSample = 2,
        SizeSetSample = 3,
        PreProductionSample = 4,
        ShipmentSample = 5
    }

    public enum OrderStatus
    {
        Draft = 0,
        Pending = 1,
        Confirmed = 2,
        InProgress = 3,
        Completed = 4,
        Shipped = 5,
        Cancelled = 6
    }

    public enum ShipmentMethod
    {
        Sea = 1,
        Air = 2,
        Road = 3,
        Rail = 4
    }

    public enum SampleStatus
    {
        Pending = 1,
        Sent = 2,
        Approved = 3,
        Rejected = 4,
        UnderReview = 5
    }

    public enum TransactionType
    {
        StockIn = 1,
        StockOut = 2,
        OpeningBalance = 3,
        Adjustment = 4
    }

    public enum BookingStatus
    {
        Pending = 1,
        Confirmed = 2,
        Issued = 3,
        Cancelled = 4
    }

    public enum PackType
    {
        PackA = 1,
        PackB = 2,
        PackAB = 3
    }
}
