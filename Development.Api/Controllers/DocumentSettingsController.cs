using HaulitCore.Api.Extensions;
using HaulitCore.Application.Interfaces.Repositories;
using HaulitCore.Contracts.Settings;
using HaulitCore.Infrastructure.Persistence.Repositories;
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
    private readonly IDocumentSettingsRepository _documentSettingsRepository;
    private readonly IUserAccountRepository _userAccountRepository;

    // Constructor injection of the repository.
    public DocumentSettingsController(
        IDocumentSettingsRepository documentSettingsRepository,
        IUserAccountRepository userAccountRepository)
    {
        _documentSettingsRepository = documentSettingsRepository;
        _userAccountRepository = userAccountRepository;
    }

    // Retrieves the document settings for the current owner.
    // If no settings exist yet, a default record is created automatically.
    [HttpGet]
    public async Task<ActionResult<DocumentSettingsDto>> Get()
    {
        // Extract the owner ID from the authenticated user's claims.
        var ownerUserId = User.GetOwnerUserId();

        // Load existing settings or create defaults if none exist.
        var settings = await _documentSettingsRepository.GetOrCreateAsync(ownerUserId);
        var user = await _userAccountRepository.GetByIdAsync(ownerUserId);

        if (user is null)
            return NotFound();

        // Map the domain entity to a DTO for API response.
        return Ok(new DocumentSettingsDto
        {
            GstEnabled = settings.GstEnabled,
            GstRatePercent = settings.GstRatePercent,

            FuelSurchargeEnabled = settings.FuelSurchargeEnabled,
            FuelSurchargePercent = settings.FuelSurchargePercent,

            WaitTimeCharge = settings.WaitTimeCharge,
            HandUnloadCharge = settings.HandUnloadCharge,
            WaitTimeChargeEnabled = settings.WaitTimeChargeEnabled,
            HandUnloadChargeEnabled = settings.HandUnloadChargeEnabled,
            InvoicePrefix = settings.InvoicePrefix,
            PodPrefix = settings.PodPrefix,
            PaymentTermsDays = settings.PaymentTermsDays,
            ShowDamageNotesOnPod = settings.ShowDamageNotesOnPod,
            ShowWaitTimeOnPod = settings.ShowWaitTimeOnPod,

            BusinessAddress1 = user.BusinessAddress1,
            BusinessSuburb = user.BusinessSuburb,
            BusinessCity = user.BusinessCity,
            BusinessRegion = user.BusinessRegion,
            BusinessPostcode = user.BusinessPostcode,
            BusinessCountry = user.BusinessCountry,
            SupplierGstNumber = user.SupplierGstNumber,
            SupplierNzbn = user.SupplierNzbn,
            BankAccountNumber = user.BankAccountNumber
        });
    }

    // Updates the document settings for the current owner.
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateDocumentSettingsRequest request)
    {
        // Extract the owner ID from the authenticated user's claims.
        var ownerUserId = User.GetOwnerUserId();

        // Load existing settings or create defaults if none exist.
        var settings = await _documentSettingsRepository.GetOrCreateAsync(ownerUserId);
        var user = await _userAccountRepository.GetByIdAsync(ownerUserId);

        if (user is null)
            return NotFound();

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

        user.UpdateBusinessAddress(
            line1: request.BusinessAddress1,
            suburb: request.BusinessSuburb,
            city: request.BusinessCity,
            region: request.BusinessRegion,
            postcode: request.BusinessPostcode,
            country: request.BusinessCountry);

        user.UpdateBusinessIdentity(
            businessName: user.BusinessName,
            businessEmail: user.BusinessEmail,
            gstNumber: request.SupplierGstNumber,
            nzbn: request.SupplierNzbn,
            bankAccountNumber: request.BankAccountNumber);
        System.Diagnostics.Debug.WriteLine(
    $"[API Received Settings] GST={request.GstEnabled}, GST Rate={request.GstRatePercent}, Fuel={request.FuelSurchargeEnabled}, Fuel Rate={request.FuelSurchargePercent}");
        // Persist the updated settings.
        await _documentSettingsRepository.SaveAsync(settings);
        await _userAccountRepository.SaveChangesAsync();

        // Return 204 No Content to indicate a successful update with no response body.
        return NoContent();
    }
}