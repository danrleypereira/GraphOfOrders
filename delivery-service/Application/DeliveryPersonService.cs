using DeliveryService.Domain;
using DeliveryService.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DeliveryService.Application;

public class DeliveryPersonService : IDeliveryPersonService
{
    private readonly DeliveryContext _context;
    private readonly ILogger<DeliveryPersonService> _logger;

    public DeliveryPersonService(DeliveryContext context, ILogger<DeliveryPersonService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<DeliveryPerson>> GetAllAsync()
    {
        _logger.LogInformation("Getting all delivery persons");
        return await _context.DeliveryPersons.ToListAsync();
    }

    public async Task<DeliveryPerson?> GetByIdAsync(int id)
    {
        _logger.LogInformation("Getting delivery person with id {Id}", id);
        return await _context.DeliveryPersons
            .Include(dp => dp.DeliveryOrders)
            .FirstOrDefaultAsync(dp => dp.Id == id);
    }

    public async Task<DeliveryPerson> CreateAsync(DeliveryPerson deliveryPerson)
    {
        _logger.LogInformation("Creating new delivery person: {Name}", deliveryPerson.Name);
        deliveryPerson.CreatedAt = DateTime.UtcNow;
        deliveryPerson.UpdatedAt = DateTime.UtcNow;

        _context.DeliveryPersons.Add(deliveryPerson);
        await _context.SaveChangesAsync();

        return deliveryPerson;
    }

    public async Task<DeliveryPerson?> UpdateAsync(int id, DeliveryPerson deliveryPerson)
    {
        _logger.LogInformation("Updating delivery person with id {Id}", id);

        var existing = await _context.DeliveryPersons.FindAsync(id);
        if (existing == null)
        {
            _logger.LogWarning("Delivery person with id {Id} not found", id);
            return null;
        }

        existing.Name = deliveryPerson.Name;
        existing.Phone = deliveryPerson.Phone;
        existing.Email = deliveryPerson.Email;
        existing.IsActive = deliveryPerson.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        _logger.LogInformation("Deleting delivery person with id {Id}", id);

        var deliveryPerson = await _context.DeliveryPersons.FindAsync(id);
        if (deliveryPerson == null)
        {
            _logger.LogWarning("Delivery person with id {Id} not found", id);
            return false;
        }

        _context.DeliveryPersons.Remove(deliveryPerson);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<DeliveryPerson>> GetActiveAsync()
    {
        _logger.LogInformation("Getting active delivery persons");
        return await _context.DeliveryPersons
            .Where(dp => dp.IsActive)
            .ToListAsync();
    }
}