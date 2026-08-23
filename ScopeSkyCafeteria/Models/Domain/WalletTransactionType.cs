namespace ScopeSkyCafeteria.Models.Domain
{
    public enum WalletTransactionType
    {
        Deposit = 0,
        Purchase = 1,
        Debt = 2,
        DebtPayment = 3,
        BalanceAdjustment = 4,
        DebtAdjustment = 5
    }
}