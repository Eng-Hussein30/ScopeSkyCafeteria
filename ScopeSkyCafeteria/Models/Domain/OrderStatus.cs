namespace ScopeSkyCafeteria.Models.Domain
{
    public enum OrderStatus
    {
        Pending = 0,
        Accepted = 1,
        Preparing = 2,
        Ready = 3,
        Delivered = 4,
        Cancelled = 5,
        OnTheWay = 6
    }
}