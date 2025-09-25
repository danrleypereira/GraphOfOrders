using DeliveryService.Domain;

namespace DeliveryService.Application;

public interface IDeliveryPersonService
{
    Task<IEnumerable<DeliveryPerson>> GetAllAsync();
    Task<DeliveryPerson?> GetByIdAsync(int id);
    Task<DeliveryPerson> CreateAsync(DeliveryPerson deliveryPerson);
    Task<DeliveryPerson?> UpdateAsync(int id, DeliveryPerson deliveryPerson);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<DeliveryPerson>> GetActiveAsync();
}