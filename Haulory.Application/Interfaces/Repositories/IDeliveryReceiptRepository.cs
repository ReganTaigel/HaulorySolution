using Haulory.Domain.Entities;

namespace Haulory.Application.Interfaces.Repositories;

#region Interface: Delivery Receipt Repository

public interface IDeliveryReceiptRepository
{
    #region Create

    // Persists a new delivery receipt
    Task AddAsync(DeliveryReceipt receipt);

    #endregion

    #region Queries

    // Retrieves all delivery receipts
    // Typically used for admin, reporting, or auditing
    Task<IReadOnlyList<DeliveryReceipt>> GetAllAsync();

    // Retrieves delivery receipts linked to a specific Job
    // Used when viewing job history or generating invoices
    Task<IReadOnlyList<DeliveryReceipt>> GetByJobIdAsync(Guid jobId);

    #endregion
}

#endregion
