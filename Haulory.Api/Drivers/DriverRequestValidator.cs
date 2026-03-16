using Haulory.Contracts.Drivers;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Haulory.Api.Drivers;

public sealed class DriverRequestValidator
{
    public void ValidateCreate(CreateDriverRequest request, ModelStateDictionary modelState)
    {
        ValidateShared(request.FirstName, request.LastName, request.Email, modelState);
        ValidateEmergencyContact(request.EmergencyContact, modelState);

        if (request.CreateLoginAccount && string.IsNullOrWhiteSpace(request.Password))
            modelState.AddModelError(nameof(request.Password), "Password is required when creating a login account.");
    }

    public void ValidateUpdate(UpdateDriverRequest request, ModelStateDictionary modelState)
    {
        ValidateShared(request.FirstName, request.LastName, request.Email, modelState);
    }

    private static void ValidateShared(string? firstName, string? lastName, string? email, ModelStateDictionary modelState)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            modelState.AddModelError(nameof(CreateDriverRequest.FirstName), "First name is required.");

        if (string.IsNullOrWhiteSpace(lastName))
            modelState.AddModelError(nameof(CreateDriverRequest.LastName), "Last name is required.");

        if (string.IsNullOrWhiteSpace(email))
            modelState.AddModelError(nameof(CreateDriverRequest.Email), "Email is required.");
    }

    private static void ValidateEmergencyContact(EmergencyContactRequest? emergencyContact, ModelStateDictionary modelState)
    {
        if (emergencyContact is null)
        {
            modelState.AddModelError(nameof(CreateDriverRequest.EmergencyContact), "Emergency contact is required.");
            return;
        }

        if (string.IsNullOrWhiteSpace(emergencyContact.FirstName))
            modelState.AddModelError(nameof(emergencyContact.FirstName), "Emergency contact first name is required.");

        if (string.IsNullOrWhiteSpace(emergencyContact.LastName))
            modelState.AddModelError(nameof(emergencyContact.LastName), "Emergency contact last name is required.");

        if (string.IsNullOrWhiteSpace(emergencyContact.Relationship))
            modelState.AddModelError(nameof(emergencyContact.Relationship), "Emergency contact relationship is required.");

        if (string.IsNullOrWhiteSpace(emergencyContact.Email))
            modelState.AddModelError(nameof(emergencyContact.Email), "Emergency contact email is required.");

        if (string.IsNullOrWhiteSpace(emergencyContact.PhoneNumber))
            modelState.AddModelError(nameof(emergencyContact.PhoneNumber), "Emergency contact phone number is required.");
    }
}
