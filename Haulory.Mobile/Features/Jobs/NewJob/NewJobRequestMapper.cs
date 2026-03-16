using Haulory.Contracts.Jobs;

namespace Haulory.Mobile.Features.Jobs.NewJob;

public sealed class NewJobRequestMapper
{
    private readonly NewJobValidator _validator;

    public NewJobRequestMapper(NewJobValidator validator)
    {
        _validator = validator;
    }

    public CreateJobRequest MapCreate(NewJobFormState state)
        => new()
        {
            ClientCompanyName = state.ClientCompanyName,
            ClientContactName = state.ClientContactName,
            ClientEmail = state.ClientEmail,
            ClientAddressLine1 = state.ClientAddressLine1,
            ClientCity = state.ClientCity,
            ClientCountry = state.ClientCountry,
            PickupCompany = state.PickupCompany,
            PickupAddress = state.PickupAddress,
            DeliveryCompany = state.DeliveryCompany,
            DeliveryAddress = state.DeliveryAddress,
            ReferenceNumber = state.ReferenceNumber,
            LoadDescription = state.LoadDescription,
            RateType = state.RateType,
            RateValue = state.RateValue,
            Quantity = state.Quantity,
            DriverId = state.SelectedDriverId,
            VehicleAssetId = state.SelectedVehicleId,
            AssignedToUserId = state.SelectedDriverUserId,
            TrailerAssetIds = _validator.GetTrailerIds(state)
        };

    public UpdateJobRequest MapUpdate(NewJobFormState state)
        => new()
        {
            ClientCompanyName = state.ClientCompanyName,
            ClientContactName = state.ClientContactName,
            ClientEmail = state.ClientEmail,
            ClientAddressLine1 = state.ClientAddressLine1,
            ClientCity = state.ClientCity,
            ClientCountry = state.ClientCountry,
            PickupCompany = state.PickupCompany,
            PickupAddress = state.PickupAddress,
            DeliveryCompany = state.DeliveryCompany,
            DeliveryAddress = state.DeliveryAddress,
            ReferenceNumber = state.ReferenceNumber,
            LoadDescription = state.LoadDescription,
            RateType = state.RateType,
            RateValue = state.RateValue,
            Quantity = state.Quantity,
            DriverId = state.SelectedDriverId,
            VehicleAssetId = state.SelectedVehicleId,
            AssignedToUserId = state.SelectedDriverUserId,
            TrailerAssetIds = _validator.GetTrailerIds(state),
            WaitTimeMinutes = _validator.ParseWaitTimeMinutes(state.WaitTimeMinutesText).value,
            DamageNotes = state.DamageNotes
        };

    public UpdatePickupDetailsRequest MapPickupDetails(NewJobFormState state)
        => new()
        {
            WaitTimeMinutes = _validator.ParseWaitTimeMinutes(state.WaitTimeMinutesText).value,
            DamageNotes = state.DamageNotes
        };

    public ReviewJobRequest MapReview(NewJobFormState state)
        => new()
        {
            WaitTimeMinutes = _validator.ParseWaitTimeMinutes(state.WaitTimeMinutesText).value,
            DamageNotes = state.DamageNotes
        };
}
