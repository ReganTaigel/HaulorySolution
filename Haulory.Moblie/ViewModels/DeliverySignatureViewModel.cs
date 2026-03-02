using Haulory.Application.Interfaces.Repositories;
using Haulory.Application.Interfaces.Services;
using Haulory.Domain.Entities;
using Haulory.Mobile.Views;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Haulory.Mobile.ViewModels;

[QueryProperty(nameof(JobId), "jobId")]
public class DeliverySignatureViewModel : BaseViewModel
{
    #region Dependencies

    private readonly IJobRepository _jobRepository;
    private readonly IDeliveryReceiptRepository _deliveryReceiptRepository;
    private readonly ISessionService _session;
    private readonly IUnitOfWork _uow;

    private readonly IAppEventBus? _events;

    #endregion

    #region State

    private Job? _job;
    private bool _isSaving;

    private string _receiverName = string.Empty;
    private string _statusMessage = string.Empty;

    private Guid _jobId;

    #endregion

    #region Bindable Properties - Job Details

    public string ReferenceNumber => _job?.ReferenceNumber ?? string.Empty;
    public string PickupCompany => _job?.PickupCompany ?? string.Empty;
    public string DeliveryCompany => _job?.DeliveryCompany ?? string.Empty;
    public string DeliveryAddress => _job?.DeliveryAddress ?? string.Empty;
    public string LoadDescription => _job?.LoadDescription ?? string.Empty;

    public bool IsDelivered => _job?.IsDelivered ?? false;

    #endregion

    #region Bindable Properties - Receiver + Status

    public string ReceiverName
    {
        get => _receiverName;
        set
        {
            if (_receiverName == value) return;
            _receiverName = value;
            OnPropertyChanged();
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            if (_statusMessage == value) return;
            _statusMessage = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Signature Data

    private readonly List<List<PointF>> _strokes = new();
    private List<PointF>? _currentStroke;

    public SignatureDrawable SignatureDrawable { get; }

    #endregion

    #region Commands

    public ICommand ClearSignatureCommand { get; }
    public ICommand SaveCommand { get; }

    #endregion

    #region Query Params

    public string JobId
    {
        get => _jobId.ToString();
        set
        {
            if (!Guid.TryParse(value, out var id))
                return;

            _jobId = id;
            _ = LoadJobSafeAsync();
        }
    }

    #endregion

    #region Constructor

    public DeliverySignatureViewModel(
        IJobRepository jobRepository,
        IDeliveryReceiptRepository deliveryReceiptRepository,
        ISessionService session,
        IUnitOfWork uow,
        IAppEventBus? events = null)
    {
        _jobRepository = jobRepository;
        _deliveryReceiptRepository = deliveryReceiptRepository;
        _session = session;
        _uow = uow;
        _events = events;

        SignatureDrawable = new SignatureDrawable(_strokes);

        ClearSignatureCommand = new Command(() =>
        {
            _strokes.Clear();
            _currentStroke = null;
            SignatureDrawable.InvalidateRequested?.Invoke();
            StatusMessage = "Signature cleared.";
        });

        SaveCommand = new Command(async () => await SaveAsync(), () => !_isSaving);
    }

    #endregion

    #region Load

    private async Task LoadJobSafeAsync()
    {
        try
        {
            await LoadJobAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Load failed: {ex.Message}";
        }
    }

    private async Task LoadJobAsync()
    {
        StatusMessage = "Loading job...";

        // IMPORTANT: tracked entity so we can safely work with it in a transaction if needed
        _job = await _jobRepository.GetByIdForUpdateAsync(_jobId);

        if (_job == null)
        {
            StatusMessage = "Job not found.";
            RaiseJobPropertiesChanged();
            return;
        }

        RaiseJobPropertiesChanged();

        StatusMessage = _job.IsDelivered
            ? "This job has already been delivered."
            : "Sign and save delivery.";
    }

    private void RaiseJobPropertiesChanged()
    {
        OnPropertyChanged(nameof(ReferenceNumber));
        OnPropertyChanged(nameof(PickupCompany));
        OnPropertyChanged(nameof(DeliveryCompany));
        OnPropertyChanged(nameof(DeliveryAddress));
        OnPropertyChanged(nameof(LoadDescription));
        OnPropertyChanged(nameof(IsDelivered));
    }

    #endregion

    #region Touch Methods

    public void StartStroke(PointF point)
    {
        _currentStroke = new List<PointF> { point };
        _strokes.Add(_currentStroke);
        SignatureDrawable.InvalidateRequested?.Invoke();
    }

    public void DragStroke(PointF point)
    {
        _currentStroke?.Add(point);
        SignatureDrawable.InvalidateRequested?.Invoke();
    }

    public void EndStroke()
    {
        _currentStroke = null;
        SignatureDrawable.InvalidateRequested?.Invoke();
    }

    #endregion

    #region Save Flow

    private async Task SaveAsync()
    {
        if (_isSaving) return;

        _isSaving = true;
        ((Command)SaveCommand).ChangeCanExecute();

        try
        {
            if (_job == null)
            {
                StatusMessage = "Job not loaded.";
                return;
            }

            if (_job.IsDelivered)
            {
                await Shell.Current.DisplayAlertAsync(
                    "Already Delivered",
                    "This job has already been signed off.",
                    "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(ReceiverName))
            {
                await Shell.Current.DisplayAlertAsync(
                    "Missing Info",
                    "Please enter receiver name.",
                    "OK");
                return;
            }

            if (!HasSignature())
            {
                await Shell.Current.DisplayAlertAsync(
                    "Missing Signature",
                    "Please capture a signature before saving.",
                    "OK");
                return;
            }

            var ownerUserId = _session.CurrentAccountId ?? Guid.Empty;
            if (ownerUserId == Guid.Empty)
            {
                await Shell.Current.DisplayAlertAsync("Not logged in", "Please log in again.", "OK");
                return;
            }

            var signatureJson = BuildSignatureJson();
            var deliveredAtUtc = DateTime.UtcNow;

            // Mark delivered (optional if you still delete job, but OK)
            _job.MarkDelivered(ReceiverName.Trim(), signatureJson);

            var receipt = new DeliveryReceipt(
                ownerUserId: ownerUserId,
                jobId: _job.Id,

                // Client snapshot from Job (entered at job creation)
                clientCompanyName: _job.ClientCompanyName,
                clientContactName: _job.ClientContactName,
                clientEmail: _job.ClientEmail,
                clientAddressLine1: _job.ClientAddressLine1,
                clientCity: _job.ClientCity,
                clientCountry: _job.ClientCountry,

                // existing receipt snapshot fields
                referenceNumber: _job.ReferenceNumber,
                invoiceNumber: _job.InvoiceNumber,
                pickupCompany: _job.PickupCompany,
                pickupAddress: _job.PickupAddress,
                deliveryCompany: _job.DeliveryCompany,
                deliveryAddress: _job.DeliveryAddress,
                loadDescription: _job.LoadDescription,
                rateType: _job.RateType,
                rateValue: _job.RateValue,
                quantity: _job.Quantity,
                total: _job.Total,
                receiverName: ReceiverName.Trim(),
                deliveredAtUtc: deliveredAtUtc,
                signatureJson: signatureJson
            );

            await _uow.ExecuteInTransactionAsync(async () =>
            {
                await _deliveryReceiptRepository.AddAsync(receipt);
                await _jobRepository.DeleteAsync(_job.Id);
            });

            if (_events != null)
                await _events.PublishAsync(new JobCompletedEvent(_job.Id));

            await Shell.Current.DisplayAlertAsync(
                "Saved",
                "Delivery signed and moved to Reports.",
                "OK");

            await Shell.Current.GoToAsync(nameof(DashboardPage));
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Save failed", ex.Message, "OK");
        }
        finally
        {
            _isSaving = false;
            ((Command)SaveCommand).ChangeCanExecute();
        }
    }
    private bool HasSignature() => _strokes.Any(s => s.Count > 2);

    private string BuildSignatureJson()
    {
        var data = new SignatureData(
            _strokes.Select(stroke =>
                new SignatureStroke(
                    stroke.Select(p => new SigPoint(p.X, p.Y)).ToList()
                )
            ).ToList()
        );

        return JsonSerializer.Serialize(data);
    }

    #endregion
}

#region Events (tiny, optional)

public record JobCompletedEvent(Guid JobId);

public interface IAppEventBus
{
    Task PublishAsync<T>(T message);
    void Subscribe<T>(object subscriber, Action<T> handler);
    void Unsubscribe(object subscriber);
}

#endregion

#region Signature JSON Models

public record SigPoint(float X, float Y);
public record SignatureStroke(List<SigPoint> Points);
public record SignatureData(List<SignatureStroke> Strokes);

#endregion

#region Drawable

public class SignatureDrawable : IDrawable
{
    private readonly List<List<PointF>> _strokes;
    public Action? InvalidateRequested;

    public SignatureDrawable(List<List<PointF>> strokes)
    {
        _strokes = strokes;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.FillColor = Colors.Transparent;
        canvas.StrokeColor = Colors.Black;
        canvas.StrokeSize = 3;

        foreach (var stroke in _strokes)
        {
            for (int i = 1; i < stroke.Count; i++)
                canvas.DrawLine(stroke[i - 1], stroke[i]);
        }
    }
}

#endregion