namespace Transaction.API.Enums;

public enum TransactionStatus
{
    Pending    = 0,
    Processing = 1,
    Completed  = 2,
    Failed     = 3,
    Reversed   = 4   // Saga compensation applied
}