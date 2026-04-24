using HaulitCore.Domain.Entities;

namespace HaulitCore.Application.Interfaces.Repositories;

public interface IDeliveryReceiptRepository
{
    Task AddAsync(DeliveryReceipt receipt);
    Task UpdateAsync(DeliveryReceipt receipt);
    Task<IReadOnlyList<DeliveryReceipt>> GetByOwnerAsync(Guid ownerUserId);
    Task<IReadOnlyList<DeliveryReceipt>> GetByJobIdAsync(Guid ownerUserId, Guid jobId);
    Task<DeliveryReceipt?> GetByIdAsync(Guid ownerUserId, Guid receiptId);
    Task<IReadOnlyList<DeliveryReceipt>> GetByOwnerDeliveredBetweenUtcAsync(Guid ownerUserId, DateTime fromUtc, DateTime toUtc);
}