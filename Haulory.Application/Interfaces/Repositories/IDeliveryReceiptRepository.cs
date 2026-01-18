using Haulory.Domain.Entities;

namespace Haulory.Application.Interfaces.Repositories;

public interface IDeliveryReceiptRepository
{
    Task AddAsync(DeliveryReceipt receipt);
    Task<IReadOnlyList<DeliveryReceipt>> GetAllAsync();
    Task<IReadOnlyList<DeliveryReceipt>> GetByJobIdAsync(Guid jobId);
}
