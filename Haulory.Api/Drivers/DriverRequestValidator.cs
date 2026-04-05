using Haulory.Contracts.Drivers;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Haulory.Api.Drivers;

// Handles validation logic for driver-related requests.
// Uses ModelState to collect validation errors instead of throwing exceptions.
public sealed class DriverRequestValidator
{
    // Validates a CreateDriverRequest.
    public void ValidateCreate(CreateDriverRequest request, ModelStateDictionary modelState)
    {
        // Validate common fields shared between create and update.
        ValidateShared(request.FirstName, request.LastName, request.Email, modelState);

        // Validate emergency contact details.
        ValidateEmergencyContact(request.EmergencyContact, modelState);

        // If a login account is requested, a password must be provided.
        if (request.CreateLoginAccount && string.IsNullOrWhiteSpace(request.Password))
            modelState.AddModelError(nameof(request.Password), "Password is required when creating a login account.");
    }

    // Validates an UpdateDriverRequest.
    public void ValidateUpdate(UpdateDriverRequest request, ModelStateDictionary modelState)
    {
        // Only shared fields are validated for updates.
        ValidateShared(request.FirstName, request.LastName, request.Email, modelState);
    }

    // Validates fields common to both create and update operations.
    private static void ValidateShared(string? firstName, string? lastName, string? email, ModelStateDictionary modelState)
    {
        // Ensure first name is provided.
        if (string.IsNullOrWhiteSpace(firstName))
            modelState.AddModelError(nameof(CreateDriverRequest.FirstName), "First name is required.");

        // Ensure last name is provided.
        if (string.IsNullOrWhiteSpace(lastName))
            modelState.AddModelError(nameof(CreateDriverRequest.LastName), "Last name is required.");

        // Ensure email is provided.
        if (string.IsNullOrWhiteSpace(email))
            modelState.AddModelError(nameof(CreateDriverRequest.Email), "Email is required.");
    }

    // Validates emergency contact details for driver creation.
    private static void ValidateEmergencyContact(EmergencyContactRequest? emergencyContact, ModelStateDictionary modelState)
    {
        // Ensure emergency contact object exists.
        if (emergencyContact is null)
        {
            modelState.AddModelError(nameof(CreateDriverRequest.EmergencyContact), "Emergency contact is required.");
            return;
        }

        // Validate required emergency contact fields.
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