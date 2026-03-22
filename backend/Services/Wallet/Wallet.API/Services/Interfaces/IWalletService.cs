using Wallet.API.DTOs.Request;
using Wallet.API.DTOs.Response;

namespace Wallet.API.Services.Interfaces;

public interface IWalletService
{
    Task<WalletResponseDto>  GetWalletAsync(Guid userId);
    Task<WalletResponseDto>  FundWalletAsync(Guid userId, FundWalletRequestDto dto);
    Task<WalletResponseDto>  WithdrawAsync(Guid userId, WithdrawRequestDto dto);
    Task<WalletResponseDto>  FreezeWalletAsync(Guid walletId, Guid adminId, FreezeWalletRequestDto dto);
    Task<WalletResponseDto>  UnfreezeWalletAsync(Guid walletId, Guid adminId, FreezeWalletRequestDto dto);
    Task<WalletResponseDto>  CreateWalletAsync(Guid userId);
    Task                     ActivateWalletAsync(Guid userId);      // called after KYC approval
    Task                     DebitAsync(Guid userId, decimal amount);  // called by Transaction service event
    Task                     CreditAsync(Guid userId, decimal amount); // called by Transaction service event
}