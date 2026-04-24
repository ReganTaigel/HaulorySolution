namespace HaulitCore.Mobile.Features.Jobs.NewJob;

public sealed class NewJobValidator
{
    public IReadOnlyList<string> ValidateForSave(NewJobFormState state)
    {
        if (state.IsPickupOnly)
            return ValidateWaitTime(state);

        if (state.IsReviewOnly)
            return ValidateWaitTime(state);

        var errors = new List<string>();

        AddRequired(errors, state.ClientCompanyName, "Client company name is required.");
        AddRequired(errors, state.ClientAddressLine1, "Client address is required.");
        AddRequired(errors, state.ClientCity, "Client city is required.");
        AddRequired(errors, state.ClientCountry, "Client country is required.");
        AddRequired(errors, state.PickupCompany, "Pickup company is required.");
        AddRequired(errors, state.PickupAddress, "Pickup address is required.");
        AddRequired(errors, state.DeliveryCompany, "Delivery company is required.");
        AddRequired(errors, state.DeliveryAddress, "Delivery address is required.");

        if ((state.SelectedTrailer1Id.HasValue || state.SelectedTrailer2Id.HasValue) && !state.SelectedVehicleId.HasValue)
            errors.Add("Select a vehicle (power unit) before assigning trailers.");

        if (state.SelectedTrailer1Id.HasValue &&
            state.SelectedTrailer2Id.HasValue &&
            state.SelectedTrailer1Id == state.SelectedTrailer2Id)
        {
            errors.Add("Trailer 1 and Trailer 2 must be different trailers.");
        }

        if (GetTrailerIds(state).Count > 2)
            errors.Add("A maximum of 2 trailers can be assigned to a job.");

        if (ParseWaitTimeMinutes(state.WaitTimeMinutesText).isValid == false)
            errors.Add("Wait time must be a valid non-negative number.");

        return errors;
    }

    public List<Guid> GetTrailerIds(NewJobFormState state)
        => new[] { state.SelectedTrailer1Id, state.SelectedTrailer2Id }
            .Where(id => id.HasValue && id.Value != Guid.Empty)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

    public (bool isValid, int? value) ParseWaitTimeMinutes(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return (true, null);

        if (!int.TryParse(raw, out var parsed) || parsed < 0)
            return (false, null);

        return (true, parsed);
    }

    private static IReadOnlyList<string> ValidateWaitTime(NewJobFormState state)
    {
        var result = new List<string>();

        if (state.IsEditMode == false)
            result.Add(state.IsReviewOnly
                ? "Review can only be completed for an existing job."
                : "Pickup details can only be updated for an existing job.");

        if (!string.IsNullOrWhiteSpace(state.WaitTimeMinutesText))
        {
            if (!int.TryParse(state.WaitTimeMinutesText, out var parsedWaitTime) || parsedWaitTime < 0)
                result.Add("Wait time must be a valid non-negative number.");
        }

        return result;
    }

    private static void AddRequired(List<string> errors, string? value, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
            errors.Add(message);
    }
}
