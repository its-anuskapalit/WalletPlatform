namespace Transaction.API.Enums;

public enum TransactionType
{
    WalletFund     = 0,  // Top-up from payment method
    WalletWithdraw = 1,  // Withdraw to bank
    PeerTransfer   = 2,  // User to User
    MerchantPay    = 3,  // Pay a merchant
    Refund         = 4,  // Reversal/refund
    Reward         = 5   // Points cashback credit
}