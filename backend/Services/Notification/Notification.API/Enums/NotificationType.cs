namespace Notification.API.Enums;

public enum NotificationType
{
    Welcome             = 0,
    LoginAlert          = 1,
    KYCApproved         = 2,
    KYCRejected         = 3,
    TransactionSuccess  = 4,
    TransactionFailed   = 5,
    WalletFunded        = 6,
    PointsAwarded       = 7,
    TierUpgrade         = 8,
    RedemptionSuccess   = 9,
    PasswordChanged     = 10,
    WalletFrozen        = 11
}