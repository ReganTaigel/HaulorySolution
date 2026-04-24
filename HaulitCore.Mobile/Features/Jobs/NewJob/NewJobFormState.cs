using HaulitCore.Domain.Enums;

namespace HaulitCore.Mobile.Features.Jobs.NewJob;

public sealed class NewJobFormState
{
    public Guid? SelectedCustomerId { get; set; }
    public Guid? EditingJobId { get; set; }
    public bool IsLoadingExistingJob { get; set; }
    public bool IsPickupOnly { get; set; }
    public bool IsReviewOnly { get; set; }

    public Guid? SelectedDriverId { get; set; }
    public Guid? SelectedDriverUserId { get; set; }
    public Guid? SelectedVehicleId { get; set; }
    public Guid? SelectedTrailer1Id { get; set; }
    public Guid? SelectedTrailer2Id { get; set; }

    public RateType RateType { get; set; } = RateType.PerLoad;
    public decimal Quantity { get; set; } = 1m;
    public decimal RateValue { get; set; }

    public string PickupCompany { get; set; } = string.Empty;
    public string PickupAddress { get; set; } = string.Empty;
    public string DeliveryCompany { get; set; } = string.Empty;
    public string DeliveryAddress { get; set; } = string.Empty;
    public string ReferenceNumber { get; set; } = string.Empty;
    public string LoadDescription { get; set; } = string.Empty;

    public string ClientCompanyName { get; set; } = string.Empty;
    public string? ClientContactName { get; set; }
    public string? ClientEmail { get; set; }
    public string ClientAddressLine1 { get; set; } = string.Empty;
    public string ClientCity { get; set; } = string.Empty;
    public string ClientCountry { get; set; } = "New Zealand";
    public string WaitTimeMinutesText { get; set; } = string.Empty;
    public string? DamageNotes { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;

    public bool IsEditMode => EditingJobId.HasValue;

    public void ClearSelections()
    {
        SelectedDriverId = null;
        SelectedDriverUserId = null;
        SelectedVehicleId = null;
        SelectedTrailer1Id = null;
        SelectedTrailer2Id = null;
    }
}
