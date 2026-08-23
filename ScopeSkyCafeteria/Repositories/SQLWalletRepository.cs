using Microsoft.EntityFrameworkCore;
using ScopeSkyCafeteria.Data;
using ScopeSkyCafeteria.Models.Domain;

namespace ScopeSkyCafeteria.Repositories
{
    public class SQLWalletRepository : IWalletRepository
    {
        private readonly SSCafeteriaDbContext dbContext;

        public SQLWalletRepository(SSCafeteriaDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        // =====================================================
        // Get Wallet By User Id
        // =====================================================

        public async Task<Wallet?> GetWalletByUserIdAsync(Guid userId)
        {
            return await dbContext.Wallets
                .Include(w => w.User)
                .FirstOrDefaultAsync(w => w.UserId == userId);
        }

        // =====================================================
        // Get Wallet By Wallet Id
        // =====================================================

        public async Task<Wallet?> GetWalletByIdAsync(Guid walletId)
        {
            return await dbContext.Wallets
                .Include(w => w.User)
                .FirstOrDefaultAsync(w => w.Id == walletId);
        }

        // =====================================================
        // Get All Wallets
        // =====================================================

        public async Task<List<Wallet>> GetAllWalletsAsync()
        {
            return await dbContext.Wallets
                .Include(w => w.User)
                .OrderBy(w => w.User.UserName)
                .ToListAsync();
        }

        // =====================================================
        // Create Wallet
        // =====================================================

        public async Task<Wallet> CreateWalletAsync(Guid userId)
        {
            var existingWallet = await dbContext.Wallets
                .FirstOrDefaultAsync(w => w.UserId == userId);

            if (existingWallet != null)
            {
                return existingWallet;
            }

            var wallet = new Wallet
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Balance = 0,
                Debt = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await dbContext.Wallets.AddAsync(wallet);
            await dbContext.SaveChangesAsync();

            return wallet;
        }

        // =====================================================
        // Deposit
        // إضافة مبلغ حقيقي إلى المحفظة
        // =====================================================

        public async Task<Wallet?> DepositAsync(Guid userId,decimal amount,Guid performedByUserId,string? note = null)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be greater than zero.");

            var wallet = await dbContext.Wallets
                .FirstOrDefaultAsync(w => w.UserId == userId);

            if (wallet == null)
                return null;

            wallet.Balance += amount;
            wallet.UpdatedAt = DateTime.UtcNow;

            var transaction = new WalletTransaction
            {
                Id = Guid.NewGuid(),
                WalletId = wallet.Id,
                Amount = amount,
                Type = WalletTransactionType.Deposit,
                PerformedByUserId = performedByUserId,
                Note = note,
                CreatedAt = DateTime.UtcNow
            };

            await dbContext.WalletTransactions.AddAsync(transaction);

            await dbContext.SaveChangesAsync();

            return wallet;
        }

        // =====================================================
        // Adjust Balance
        // تعديل الرصيد مباشرة
        // =====================================================

        public async Task<Wallet?> AdjustBalanceAsync(Guid userId, decimal amount, Guid performedByUserId, string? note = null)
        
        { var wallet = await dbContext.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
            if (wallet == null) return null; 
            var newBalance = wallet.Balance + amount;
            if (newBalance < 0) throw new InvalidOperationException("Wallet balance cannot be negative."); 
            wallet.Balance = newBalance; wallet.UpdatedAt = DateTime.UtcNow; var transaction = new WalletTransaction { Id = Guid.NewGuid(), WalletId = wallet.Id, Amount = amount, Type = WalletTransactionType.BalanceAdjustment, PerformedByUserId = performedByUserId, Note = note, CreatedAt = DateTime.UtcNow }; await dbContext.WalletTransactions.AddAsync(transaction); await dbContext.SaveChangesAsync(); return wallet; }

        // =====================================================
        // Adjust Debt
        // تعديل الدين مباشرة
        // =====================================================

        public async Task<Wallet?> AdjustDebtAsync(Guid userId,decimal amount,Guid performedByUserId,string? note = null)
        {
            var wallet = await dbContext.Wallets
                .FirstOrDefaultAsync(w => w.UserId == userId);

            if (wallet == null)
                return null;

            var newDebt = wallet.Debt + amount;

            if (newDebt < 0)
                throw new InvalidOperationException(
                    "Debt cannot be negative.");

            wallet.Debt = newDebt;
            wallet.UpdatedAt = DateTime.UtcNow;

            var transaction = new WalletTransaction
            {
                Id = Guid.NewGuid(),
                WalletId = wallet.Id,
                Amount = amount,
                Type = WalletTransactionType.DebtAdjustment,
                PerformedByUserId = performedByUserId,
                Note = note,
                CreatedAt = DateTime.UtcNow
            };

            await dbContext.WalletTransactions.AddAsync(transaction);

            await dbContext.SaveChangesAsync();

            return wallet;
        }

        // =====================================================
        // Pay Debt
        // تسديد الدين
        // =====================================================

        public async Task<Wallet?> PayDebtAsync(Guid userId,decimal amount,Guid performedByUserId,string? note = null)
        {
            if (amount <= 0)
                throw new ArgumentException(
                    "Payment amount must be greater than zero.");

            var wallet = await dbContext.Wallets
                .FirstOrDefaultAsync(w => w.UserId == userId);

            if (wallet == null)
                return null;

            if (amount > wallet.Debt)
                throw new InvalidOperationException(
                    "Payment amount cannot be greater than the user's debt.");

            wallet.Debt -= amount;
            wallet.UpdatedAt = DateTime.UtcNow;

            var transaction = new WalletTransaction
            {
                Id = Guid.NewGuid(),
                WalletId = wallet.Id,
                Amount = amount,
                Type = WalletTransactionType.DebtPayment,
                PerformedByUserId = performedByUserId,
                Note = note,
                CreatedAt = DateTime.UtcNow
            };

            await dbContext.WalletTransactions.AddAsync(transaction);

            await dbContext.SaveChangesAsync();

            return wallet;
        }

        public async Task<List<WalletTransaction>> GetTransactionsByUserIdAsync(Guid userId)
        {
            return await dbContext.WalletTransactions
                .Include(t => t.Wallet)
                .Include(t => t.PerformedByUser)
                .Where(t => t.Wallet.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }
    }
}


