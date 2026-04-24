using HaulitCore.Api.Extensions;
using HaulitCore.Application.Interfaces.Repositories;
using HaulitCore.Contracts.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HaulitCore.Api.Controllers;

// Marks this class as an API controller with automatic model binding and validation.
[ApiController]

// Base route: api/document-settings
[Route("api/document-settings")]

// Requires authentication for all endpoints.
[Authorize]
public class DocumentSettingsController : ControllerBase
{
    // Repository responsible for accessing and persisting document settings.
    private readonly IDocumentSettingsRepository _repository;

    // Constructor injection of the repository.
    public DocumentSettingsController(IDocumentSettingsRepository repository)
    {
        _repository = repository;
    }

    // Retrieves the document settings for the current owner.
    // If no settings exist yet, a default record is created automatically.
    [HttpGet]
    public async Task<ActionResult<DocumentSettingsDto>> Get()
    {
        // Extract the owner ID from the authenticated user's claims.
        var ownerUserId = User.GetOwnerUserId();

        // Load existing settings or create defaults if none exist.
        var settings = await _repository.GetOrCreateAsync(ownerUserId);

        // Map the domain entity to a DTO for API response.
        return Ok(new DocumentSettingsDto
        {
            GstEnabled = settings.GstEnabled,
            GstRatePercent = settings.GstRatePercent,
            FuelSurchargeEnabled = settings.FuelSurchargeEnabled,
            FuelSurchargePercent = settings.FuelSurchargePercent,
            InvoicePrefix = settings.InvoicePrefix,
            PodPrefix = settings.PodPrefix,
            PaymentTermsDays = settings.PaymentTermsDays,
            ShowDamageNotesOnPod = settings.ShowDamageNotesOnPod,
            ShowWaitTimeOnPod = settings.ShowWaitTimeOnPod,
        });
    }

    // Updates the document settings for the current owner.
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateDocumentSettingsRequest request)
    {
        // Extract the owner ID from the authenticated user's claims.
        var ownerUserId = User.GetOwnerUserId();

        // Load existing settings or create defaults if none exist.
        var settings = await _repository.GetOrCreateAsync(ownerUserId);

        // Apply updates using the domain method to enforce business rules.
        settings.Update(
            gstEnabled: request.GstEnabled,
            gstRatePercent: request.GstRatePercent,
            fuelSurchargeEnabled: request.FuelSurchargeEnabled,
            fuelSurchargePercent: request.FuelSurchargePercent,
            waitTimeCharge: request.WaitTimeCharge,
            handUnloadCharge: request.HandUnloadCharge,
            waitTimeChargeEnabled: request.WaitTimeChargeEnabled,
            handUnloadChargeEnabled: request.HandUnloadChargeEnabled,
            invoicePrefix: request.InvoicePrefix,
            podPrefix: request.PodPrefix,
            paymentTermsDays: request.PaymentTermsDays,
            showDamageNotesOnPod: request.ShowDamageNotesOnPod,
            showWaitTimeOnPod: request.ShowWaitTimeOnPod);

        // Persist the updated settings.
        await _repository.SaveAsync(settings);

        // Return 204 No Content to indicate a successful update with no response body.
        return NoContent();
    }
}