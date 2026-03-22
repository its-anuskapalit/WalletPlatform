namespace Rewards.API.Enums;

public enum PointTransactionType
{
    Earned   = 0,  // Points credited from a payment
    Redeemed = 1,  // Points spent on catalog item
    Expired  = 2,  // Points expired (future use)
    Adjusted = 3   // Manual admin adjustment
}