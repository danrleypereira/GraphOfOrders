using DeliveryService.Domain;

namespace DeliveryService.Application;

public interface IDeliveryOrderService
{
    Task<DeliveryOrder> CreateFromMonolithAsync(DeliveryOrder order, string idempotencyToken);
    Task<IEnumerable<DeliveryOrder>> GetAvailableOrdersByCategoryAsync(string? category = null);
    Task<DeliveryOrder?> AssignDeliveryPersonAsync(int orderId, int customerId, int deliveryPersonId, string idempotencyToken);
    Task<IEnumerable<DeliveryOrder>> GetOrdersByDeliveryPersonAsync(int deliveryPersonId);
    Task<DeliveryOrder?> GetOrderAsync(int orderId, int customerId);
    Task<DeliveryOrder?> UpdateOrderStatusAsync(int orderId, int customerId, string status);
}