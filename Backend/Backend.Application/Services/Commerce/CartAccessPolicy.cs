using Backend.Domain.Entities.Carts;

namespace Backend.Application.Services;

internal static class CartAccessPolicy
{
    public static bool CanAccess(ShoppingCart cart, Guid? userId, Guid? sessionId)
    {
        if (cart.UserId.HasValue)
        {
            return userId.HasValue && cart.UserId.Value == userId.Value;
        }

        if (cart.SessionId.HasValue)
        {
            return sessionId.HasValue && cart.SessionId.Value == sessionId.Value;
        }

        return false;
    }
}
