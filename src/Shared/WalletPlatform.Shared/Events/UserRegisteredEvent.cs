//Auth.API registers a new user 3 other Services reacts (Wallet creates wallet, Rewards creates loyalty acc, Notifications sends welcome)
//insert of Auth calling 3 it publishes one event and others decide what to do with them
//GUId(not int) for better security
// without = string.Empty NullReferenceException will throw
namespace WalletPlatform.Shared.Events;
public class UserRegisteredEvent
{
    public Guid UserId        { get; set; }
    public string Email       { get; set; } = string.Empty; //need by notification service
    public string PhoneNumber { get; set; } = string.Empty;
    public string FullName    { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow; // returns local timezone
}