using HaulitCore.Application.Interfaces.Services;
using HaulitCore.Mobile.Features;

namespace HaulitCore.Mobile.Features.Drivers.NewDriver;

public sealed class NewDriverValidator
{
    public bool CanSave(
        NewDriverFormState state,
        ISessionService sessionService,
        IFeatureAccessService? featureAccessService,
        bool isMainUser)
    {
        if (state.IsSaving)
            return false;

        if (!(featureAccessService?.IsEnabled(AppFeature.AddDriver) ?? true))
            return false;

        if (!isMainUser)
            return false;

        if (!sessionService.IsAuthenticated)
            return false;

        var ownerId = sessionService.CurrentOwnerId ?? Guid.Empty;
        if (ownerId == Guid.Empty)
            return false;

        return Validate(state).Count == 0;
    }

    public IReadOnlyList<string> Validate(NewDriverFormState state)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(state.FirstName))
            errors.Add("First name is required.");

        if (string.IsNullOrWhiteSpace(state.LastName))
            errors.Add("Last name is required.");

        var email = state.Email?.Trim();
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            errors.Add("A valid email is required.");

        if (string.IsNullOrWhiteSpace(state.EmergencyFirstName))
            errors.Add("Emergency contact first name is required.");

        if (string.IsNullOrWhiteSpace(state.EmergencyLastName))
            errors.Add("Emergency contact last name is required.");

        if (string.IsNullOrWhiteSpace(state.EmergencyRelationship))
            errors.Add("Emergency contact relationship is required.");

        var emergencyEmail = state.EmergencyEmail?.Trim();
        if (string.IsNullOrWhiteSpace(emergencyEmail) || !emergencyEmail.Contains('@'))
            errors.Add("A valid emergency contact email is required.");

        if (string.IsNullOrWhiteSpace(state.EmergencyPhoneNumber))
            errors.Add("Emergency contact phone number is required.");

        if (state.CreateLoginAccount && string.IsNullOrWhiteSpace(state.Password))
            errors.Add("Password is required when creating a login account.");

        return errors;
    }
}
