using DeliveryService.Domain;
using DeliveryService.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DeliveryService.Application;

public class DeliveryOrderService : IDeliveryOrderService
{
    private readonly DeliveryContext _context;
    private readonly ILogger<DeliveryOrderService> _logger;

    public DeliveryOrderService(DeliveryContext context, ILogger<DeliveryOrderService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<DeliveryOrder> CreateFromMonolithAsync(DeliveryOrder order, string idempotencyToken)
    {
        _logger.LogInformation("Creating order {OrderId}-{CustomerId} with idempotency token {Token}",
            order.OrderId, order.CustomerId, idempotencyToken);

        // Check for idempotency
        var existing = await _context.DeliveryOrders
            .FirstOrDefaultAsync(o => o.IdempotencyToken == idempotencyToken);

        if (existing != null)
        {
            _logger.LogInformation("Order already exists with idempotency token {Token}", idempotencyToken);
            return existing;
        }

        order.IdempotencyToken = idempotencyToken;
        order.CreatedAt = DateTime.UtcNow;
        order.Status = "Available";

        _context.DeliveryOrders.Add(order);
        await _context.SaveChangesAsync();

        return order;
    }

    public async Task<IEnumerable<DeliveryOrder>> GetAvailableOrdersByCategoryAsync(string? category = null)
    {
        _logger.LogInformation("Getting available orders by category: {Category}", category ?? "all");

        var query = _context.DeliveryOrders
            .Where(o => o.Status == "Available");

        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(o => o.Category == category);
        }

        return await query.OrderBy(o => o.CreatedAt).ToListAsync();
    }

    public async Task<DeliveryOrder?> AssignDeliveryPersonAsync(int orderId, int customerId, int deliveryPersonId, string idempotencyToken)
    {
        _logger.LogInformation("Assigning delivery person {PersonId} to order {OrderId}-{CustomerId}",
            deliveryPersonId, orderId, customerId);

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var order = await _context.DeliveryOrders
                .FirstOrDefaultAsync(o => o.OrderId == orderId && o.CustomerId == customerId);

            if (order == null)
            {
                _logger.LogWarning("Order {OrderId}-{CustomerId} not found", orderId, customerId);
                return null;
            }

            // Check if already assigned
            if (order.DeliveryPersonId != null)
            {
                _logger.LogWarning("Order {OrderId}-{CustomerId} already assigned to delivery person {PersonId}",
                    orderId, customerId, order.DeliveryPersonId);
                return order;
            }

            // Verify delivery person exists and is active
            var deliveryPerson = await _context.DeliveryPersons
                .FirstOrDefaultAsync(dp => dp.Id == deliveryPersonId && dp.IsActive);

            if (deliveryPerson == null)
            {
                _logger.LogWarning("Delivery person {PersonId} not found or inactive", deliveryPersonId);
                return null;
            }

            order.DeliveryPersonId = deliveryPersonId;
            order.Status = "Assigned";

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return order;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error assigning delivery person");
            throw;
        }
    }

    public async Task<IEnumerable<DeliveryOrder>> GetOrdersByDeliveryPersonAsync(int deliveryPersonId)
    {
        _logger.LogInformation("Getting orders for delivery person {PersonId}", deliveryPersonId);

        return await _context.DeliveryOrders
            .Where(o => o.DeliveryPersonId == deliveryPersonId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<DeliveryOrder?> GetOrderAsync(int orderId, int customerId)
    {
        _logger.LogInformation("Getting order {OrderId}-{CustomerId}", orderId, customerId);

        return await _context.DeliveryOrders
            .Include(o => o.DeliveryPerson)
            .FirstOrDefaultAsync(o => o.OrderId == orderId && o.CustomerId == customerId);
    }

    public async Task<DeliveryOrder?> UpdateOrderStatusAsync(int orderId, int customerId, string status)
    {
        _logger.LogInformation("Updating order {OrderId}-{CustomerId} status to {Status}",
            orderId, customerId, status);

        var order = await _context.DeliveryOrders
            .FirstOrDefaultAsync(o => o.OrderId == orderId && o.CustomerId == customerId);

        if (order == null)
        {
            _logger.LogWarning("Order {OrderId}-{CustomerId} not found", orderId, customerId);
            return null;
        }

        order.Status = status;
        await _context.SaveChangesAsync();

        return order;
    }
}