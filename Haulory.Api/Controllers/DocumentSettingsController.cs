using Haulory.Api.Extensions;
using Haulory.Application.Interfaces.Repositories;
using Haulory.Contracts.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Haulory.Api.Controllers;

[ApiController]
[Route("api/document-settings")]
[Authorize]
public class DocumentSettingsController : ControllerBase
{
    private readonly IDocumentSettingsRepository _repository;

    public DocumentSettingsController(IDocumentSettingsRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<ActionResult<DocumentSettingsDto>> Get()
    {
        var ownerUserId = User.GetOwnerUserId();

        var settings = await _repository.GetOrCreateAsync(ownerUserId);

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

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateDocumentSettingsRequest request)
    {
        var ownerUserId = User.GetOwnerUserId();

        var settings = await _repository.GetOrCreateAsync(ownerUserId);

        settings.Update(
            request.GstEnabled,
            request.GstRatePercent,
            request.FuelSurchargeEnabled,
            request.FuelSurchargePercent,
            request.InvoicePrefix,
            request.PodPrefix,
            request.PaymentTermsDays,
            request.ShowDamageNotesOnPod,
            request.ShowWaitTimeOnPod);

        await _repository.SaveAsync(settings);

        return NoContent();
    }
}