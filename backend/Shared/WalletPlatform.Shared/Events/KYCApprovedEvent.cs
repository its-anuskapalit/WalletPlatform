//Wallet service only needs to know whose KYC was approved so it can activate that wallet
//minimum necessary information
namespace WalletPlatform.Shared.Events;
public class KYCApprovedEvent
{
    public Guid     UserId     { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}