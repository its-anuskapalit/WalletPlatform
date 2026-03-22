using Wallet.API.Entities;

namespace Wallet.API.Repositories.Interfaces;

public interface IWalletRepository
{
    Task<Wallet.API.Entities.Wallet?>  GetByUserIdAsync(Guid userId);
    Task<Wallet.API.Entities.Wallet?>  GetByIdAsync(Guid walletId);
    Task<Wallet.API.Entities.Wallet?>  GetByWalletNumberAsync(string walletNumber);
    Task<Wallet.API.Entities.Wallet>   CreateAsync(Wallet.API.Entities.Wallet wallet);
    Task<Wallet.API.Entities.Wallet>   UpdateAsync(Wallet.API.Entities.Wallet wallet);
    Task<PaymentMethod?>               GetPaymentMethodAsync(Guid paymentMethodId);
    Task<PaymentMethod>                AddPaymentMethodAsync(PaymentMethod method);
    Task                               AddFreezeLogAsync(WalletFreezeLog log);
}