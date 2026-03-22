namespace Wallet.API.Enums;
public enum WalletStatus
{
    Pending  = 0,   // Created but KYC not done yet
    Active   = 1,   // KYC approved, fully operational
    Frozen   = 2,   // Manually frozen by admin
    Closed   = 3    // Permanently closed
}