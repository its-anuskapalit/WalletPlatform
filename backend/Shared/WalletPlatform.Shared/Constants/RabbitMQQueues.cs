//static class cannot be instantiated.
//const is evaluated at compilte time and zero runtime overhead
// readonly is at runtime
// for queue names that never chnage const is better choice
// enum is not used as Rabbitmq needs a actual string.using enum would have required .ToString()
// dot notation : ser.registered reads as "the registered event from the user domain. immediately clear what domain the event belongs to without reading documentation.

namespace WalletPlatform.Shared.Constants;
public static class RabbitMQQueues
{
    public const string UserRegistered        = "user.registered";
    public const string KYCApproved           = "kyc.approved";
    public const string WalletFunded          = "wallet.funded";
    public const string PaymentInitiated      = "payment.initiated";
    public const string TransactionCompleted  = "transaction.completed";
    public const string TransactionFailed     = "transaction.failed";
    public const string PointsAwarded         = "points.awarded";
    public const string RedemptionRequested   = "redemption.requested";
}