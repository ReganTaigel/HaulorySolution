using Haulory.Contracts.Jobs;
using Haulory.Domain.Enums;
using Haulory.Mobile.Services;
using Haulory.Mobile.ViewModels;

namespace Haulory.Mobile.Features.Jobs.NewJob;

public sealed class NewJobEditorService
{
    private readonly JobsApiService _jobsApiService;
    private readonly NewJobRequestMapper _mapper;

    public NewJobEditorService(
        JobsApiService jobsApiService,
        NewJobRequestMapper mapper)
    {
        _jobsApiService = jobsApiService;
        _mapper = mapper;
    }

    public async Task LoadIntoStateAsync(NewJobFormState state, Guid jobId)
    {
        var job = await _jobsApiService.GetJobByIdAsync(jobId);
        if (job == null)
            throw new InvalidOperationException("The selected job could not be loaded.");
        state.SelectedCustomerId = job.CustomerId;
        state.EditingJobId = jobId;
        state.ClientCompanyName = job.ClientCompanyName ?? string.Empty;
        state.ClientContactName = job.ClientContactName;
        state.ClientEmail = job.ClientEmail;
        state.ClientAddressLine1 = job.ClientAddressLine1 ?? string.Empty;
        state.ClientCity = job.ClientCity ?? string.Empty;
        state.ClientCountry = job.ClientCountry ?? "New Zealand";
        state.PickupCompany = job.PickupCompany ?? string.Empty;
        state.PickupAddress = job.PickupAddress ?? string.Empty;
        state.DeliveryCompany = job.DeliveryCompany ?? string.Empty;
        state.DeliveryAddress = job.DeliveryAddress ?? string.Empty;
        state.ReferenceNumber = job.ReferenceNumber ?? string.Empty;
        state.LoadDescription = job.LoadDescription ?? string.Empty;
        state.InvoiceNumber = job.InvoiceNumber ?? string.Empty;
        state.WaitTimeMinutesText = job.WaitTimeMinutes?.ToString() ?? string.Empty;
        state.DamageNotes = job.DamageNotes;
        state.RateType = ParseRateType(job.RateType);
        state.RateValue = job.RateValue;
        state.Quantity = job.Quantity;
        state.SelectedDriverId = job.DriverId;
        state.SelectedVehicleId = job.VehicleAssetId;
        state.SelectedTrailer1Id = job.TrailerAssetIds != null && job.TrailerAssetIds.Count > 0 ? job.TrailerAssetIds[0] : null;
        state.SelectedTrailer2Id = job.TrailerAssetIds != null && job.TrailerAssetIds.Count > 1 ? job.TrailerAssetIds[1] : null;
    }

    public async Task SaveAsync(NewJobFormState state)
    {
        if (state.IsPickupOnly)
        {
            await _jobsApiService.UpdatePickupDetailsAsync(
                state.EditingJobId!.Value,
                _mapper.MapPickupDetails(state));
            return;
        }

        if (state.IsReviewOnly)
        {
            await _jobsApiService.ReviewJobAsync(
                state.EditingJobId!.Value,
                _mapper.MapReview(state));
            return;
        }

        if (state.IsEditMode)
        {
            await _jobsApiService.UpdateJobAsync(
                state.EditingJobId!.Value,
                _mapper.MapUpdate(state));
            return;
        }

        await _jobsApiService.CreateJobAsync(_mapper.MapCreate(state));
    }

    public void SyncSelections(
        NewJobFormState state,
        DriverPickerItem? selectedDriver,
        VehiclePickerItem? selectedVehicle,
        VehiclePickerItem? selectedTrailer1,
        VehiclePickerItem? selectedTrailer2)
    {
        state.SelectedDriverId = selectedDriver?.Id;
        state.SelectedDriverUserId = selectedDriver?.UserId;
        state.SelectedVehicleId = selectedVehicle?.Id;
        state.SelectedTrailer1Id = selectedTrailer1?.Id;
        state.SelectedTrailer2Id = selectedTrailer2?.Id;
    }

    private static RateType ParseRateType(string? raw)
        => !string.IsNullOrWhiteSpace(raw) &&
           Enum.TryParse<RateType>(raw, true, out var parsedRateType)
            ? parsedRateType
            : RateType.PerLoad;
}
