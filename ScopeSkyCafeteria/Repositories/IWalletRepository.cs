using ScopeSkyCafeteria.Models.Domain;

namespace ScopeSkyCafeteria.Repositories
{
    public interface IWalletRepository
    {
        Task<Wallet?> GetWalletByUserIdAsync(Guid userId);

        Task<Wallet?> GetWalletByIdAsync(Guid walletId);

        Task<List<Wallet>> GetAllWalletsAsync();

        Task<Wallet> CreateWalletAsync(Guid userId);

        Task<Wallet?> DepositAsync(
            Guid userId,
            decimal amount,
            Guid performedByUserId,
            string? note = null);

        Task<Wallet?> AdjustBalanceAsync(
            Guid userId,
            decimal amount,
            Guid performedByUserId,
            string? note = null);

        Task<Wallet?> AdjustDebtAsync(
            Guid userId,
            decimal amount,
            Guid performedByUserId,
            string? note = null);

        Task<Wallet?> PayDebtAsync(
            Guid userId,
            decimal amount,
            Guid performedByUserId,
            string? note = null);

        Task<List<WalletTransaction>> GetTransactionsByUserIdAsync(Guid userId);
    }
}